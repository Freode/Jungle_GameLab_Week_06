using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FogOfWarManager : MonoBehaviour
{
    // 인스펙터에서 설정할 변수들
    [Header("Fog GameObject & Size")]
    public Transform fogQuadTransform;
    public Material fogDisplayMaterial;     // (최종) 안개를 화면에 보여줄 Quad의 머티리얼
    public Material fogCombineMaterial;     // (2단계)합성용 머티리얼
    public Vector2 worldSize;               // 게임 월드의 실제 크기 (Quad 크기와 일치)
    public int textureSize = 512;           // 안개 텍스처의 해상도 (높을수록 정교함)

    [Header("Fog Color")]
    // 안개 색상 정의
    public Color unexploredColor = Color.black;
    public Color exploredColor = Color.white;
    private readonly Color _visibleColor = new Color(0, 0, 0, 0); // 시야 안 (완전 투명)

    [Header("View Quality")]
    public LayerMask viewBlockerLayer;          // 인스펙터에서 'ViewBlocker' 레이어를 선택해주세요.
    public int rayCount = 36;                   // 원형 시야를 위한 레이캐스트 개수 (높을수록 정밀하지만 부하 증가)
    public float rayCircleMaxDistance = 20f;    // 원형 시야 레이캐스트 최대 거리
    public float rayForwardMaxDistance = 40f;   // 전방 시야 레이캐스트 최대 거리 (캐릭터 시야 반경보다 충분히 길게)
    public float rayRotateRange = 360f;         // 레이캐스트 회전 최대 각도
    public float visionAngleBonus = 0f;    // 버프 등을 위한 시야각 보너스 (덧셈)

    [Header("Render Textures")]
    public RenderTexture visibilityRenderTexture;   // 가시성 메시를 그릴 렌더 텍스처

    [Header("Not visibility on half-fog")]
    public LayerMask enemyLayer;                // 적 유닛을 위한 레이어 마스크

    [Header("Debugging")]
    public bool logVisibilityChanges = true;            // 콘솔에서 시야 변경 로그를 출력할지 결정
    public bool showVisibleEnemyGizmos = true;          // Scene 뷰에서 보이는 적 위에 원을 표시할
    public Color gizmoColor = Color.red;

    // === 내부 관리 변수 ===
    private Texture2D _exploredStatusTexture;                       // 탐험 상태만 저장하는 CPU 텍스처
    private RenderTexture _finalFogTexture;                         // 최종 결과물 렌더 텍스처
    private Color[] _exploredPixels;
    private bool[] _isExplored;
    private int _exploredPixelCount = 0;
    private Queue<int> _updatePixels = new Queue<int>();            // 업데이트 된 픽셀 위치
    private List<int> _newlyExploredPixelIndices = new List<int>(); // NEW: Track newly explored pixels
    private List<int> _pixelsToCommitOnSceneEnd = new List<int>(); // NEW: Pixels to commit when scene ends

    // 시야 영역을 그려주는 내부 mesh 변수
    private GameObject _visibilityMeshObject;
    private Mesh _visibilityMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private Vector3[] _vertices;
    private int[] _triangles;

    private Camera _mainCamera;
    // 시야를 밝혀주는 모든 유닛의 Transform 리스트
    public static List<Transform> VisionUnits = new List<Transform>();

    // 현재 프레임에 보이는 모든 적 유닛의 목록 (HashSet으로 중복 방지)
    private HashSet<Transform> _visibleEnemies = new HashSet<Transform>();
    private LayerMask _combinedLayerMask;       // 종합적으로 감지할 레이어 마스크

    // 안개 데이터 저장을 위한 직렬화 가능한 클래스
    [System.Serializable]
    public class FogData
    {
        public Color32[] exploredPixels32;
        public bool[] isExplored;

        public FogData(Color[] pixels, bool[] exploredStatus)
        {
            exploredPixels32 = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                exploredPixels32[i] = pixels[i];
            }
            isExplored = exploredStatus;
        }

        public Color[] ToColorArray()
        {
            Color[] pixels = new Color[exploredPixels32.Length];
            for (int i = 0; i < exploredPixels32.Length; i++)
            {
                pixels[i] = exploredPixels32[i];
            }
            return pixels;
        }
    }

    public static FogOfWarManager instance;

    void Awake()
    {
        instance = this;

        Debug.Log("[FogOfWar] FogOfWarManager Awake: 씬 로드/언로드 이벤트 구독 시도.");
        _mainCamera = Camera.main;
        // 씬 로드/언로드 이벤트 구독
        //SceneManager.sceneLoaded += OnSceneLoaded;
        //SceneManager.sceneUnloaded += OnSceneUnloaded;

        Scene currentScene = SceneManager.GetActiveScene();
        OnSceneLoaded(currentScene, LoadSceneMode.Single);

        // 초기화는 OnSceneLoaded에서 처리

    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        // 오브젝트 파괴 시 이벤트 구독 해제
        //SceneManager.sceneLoaded -= OnSceneLoaded;
        //SceneManager.sceneUnloaded -= OnSceneUnloaded;

        Scene currentScene = gameObject.scene;
        OnSceneUnloaded(currentScene);

        // 코루틴 정지
        StopAllCoroutines();
    }

    void Start()
    {
        // StartCoroutine(UpdateFogCoroutine()); // OnSceneLoaded에서 시작
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _newlyExploredPixelIndices.Clear(); // NEW: Clear on scene load
        LoadFogData(scene.name);
        StartCoroutine(UpdateFogCoroutine());
    }

    private void OnSceneUnloaded(Scene scene)
    {
        StopAllCoroutines();
        // 씬 언로드 시 지연된 안개 데이터를 저장합니다.
        // commitAll은 PlayerDataManager 등 외부에서 설정되어야 합니다 (예: 게임 클리어 여부).
        // 현재는 기본값 false로 호출합니다.
        CommitDeferredFogData(false); // Assuming not a game clear by default
        Debug.Log($"[FogOfWar] {scene.name}: Scene unloaded. Fog update coroutine stopped and deferred data committed.");
    }

    /// <summary>
    /// 현재 씬의 탐험 데이터를 영구적으로 저장합니다. 스테이지 탈출 성공 시 호출해야 합니다.
    /// </summary>
    public void CommitFogData()
    {
        SaveFogData(SceneManager.GetActiveScene().name);
        _newlyExploredPixelIndices.Clear(); // NEW: Clear after full commit
    }

    /// <summary>
    /// 현재 씬에서 새로 탐험된 픽셀 중 일부를 영구적으로 저장할 버퍼로 이동합니다.
    /// 실제 저장은 씬 종료 시 CommitDeferredFogData를 통해 이루어집니다.
    /// </summary>
    /// <param name="percentage">전체 맵 픽셀 중 저장할 픽셀의 비율 (0.0 ~ 1.0)</param>
    public void CommitPartialFogData(float percentage)
    {
        // Calculate total map pixels (textureSize * textureSize)
        int totalMapPixels = textureSize * textureSize;

        // Calculate how many pixels we *want* to move to the deferred buffer based on the percentage of the total map
        int targetMovePixelCount = Mathf.FloorToInt(totalMapPixels * Mathf.Clamp01(percentage));

        // Calculate how many pixels we *can* move from the newly explored buffer
        int pixelsToActuallyMove = Mathf.Min(targetMovePixelCount, _newlyExploredPixelIndices.Count);

        // Move selected pixels from _newlyExploredPixelIndices to _pixelsToCommitOnSceneEnd
        for (int i = 0; i < pixelsToActuallyMove; i++)
        {
            _pixelsToCommitOnSceneEnd.Add(_newlyExploredPixelIndices[i]);
        }

        // Remove moved pixels from _newlyExploredPixelIndices
        if (pixelsToActuallyMove > 0)
        {
            _newlyExploredPixelIndices.RemoveRange(0, pixelsToActuallyMove);
        }
        
        Debug.Log($"[FogOfWar] {SceneManager.GetActiveScene().name}: {pixelsToActuallyMove}개의 픽셀이 지연 저장 버퍼로 이동되었습니다 (요청: {percentage * 100f}% of total map).");
    }

    /// <summary>
    /// 씬 종료 시 지연된 안개 데이터를 영구적으로 저장합니다.
    /// </summary>
    /// <param name="commitAll">true이면 새로 탐험된 모든 픽셀과 지연된 픽셀을 저장합니다 (예: 게임 클리어 시).</param>
    public void CommitDeferredFogData(bool commitAll = false)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // 1. 기존에 저장된 (영구적인) 안개 데이터를 로드합니다.
        FogData existingSavedData = null;
        if (PlayerPrefs.HasKey($"FogData_{sceneName}"))
        {
            string json = PlayerPrefs.GetString($"FogData_{sceneName}");
            try
            {
                existingSavedData = JsonUtility.FromJson<FogData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FogOfWar] {sceneName}: 기존 안개 데이터 로드 실패 (JSON 파싱 오류): {e.Message}. 지연 저장에 실패했습니다.");
                return;
            }
        }

        // 기존 데이터가 없으면 현재 _isExplored를 기반으로 초기화
        if (existingSavedData == null || existingSavedData.isExplored == null || existingSavedData.isExplored.Length == 0)
        {
            existingSavedData = new FogData(new Color[textureSize * textureSize], new bool[textureSize * textureSize]);
            for (int i = 0; i < existingSavedData.isExplored.Length; i++)
            {
                existingSavedData.isExplored[i] = false; // 모두 미탐험으로 초기화
            }
        }

        // 2. 지연된 픽셀들을 기존 저장 데이터에 병합합니다.
        foreach (int index in _pixelsToCommitOnSceneEnd)
        {
            if (index >= 0 && index < existingSavedData.isExplored.Length)
            {
                existingSavedData.isExplored[index] = true;
            }
        }

        // 3. commitAll이 true이면 새로 탐험된 모든 픽셀도 병합합니다.
        if (commitAll)
        {
            foreach (int index in _newlyExploredPixelIndices)
            {
                if (index >= 0 && index < existingSavedData.isExplored.Length)
                {
                    existingSavedData.isExplored[index] = true;
                }
            }
        }

        // 4. 병합된 데이터를 PlayerPrefs에 저장합니다.
        string jsonToSave = JsonUtility.ToJson(existingSavedData);
        PlayerPrefs.SetString($"FogData_{sceneName}", jsonToSave);
        PlayerPrefs.Save();
        Debug.Log($"[FogOfWar] {sceneName}: 지연된 안개 데이터가 영구적으로 저장되었습니다. (Commit All: {commitAll})");

        // 5. 버퍼를 클리어합니다.
        _pixelsToCommitOnSceneEnd.Clear();
        _newlyExploredPixelIndices.Clear();
    }

    private void SaveFogData(string sceneName)
    {
        if (_exploredPixels == null || _isExplored == null) 
        {
            Debug.LogWarning($"[FogOfWar] {sceneName}: 저장할 안개 데이터가 없습니다. 저장 건너뜀.");
            return; // 데이터가 없으면 저장하지 않음
        }

        FogData data = new FogData(_exploredPixels, _isExplored);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"FogData_{sceneName}", json);
        PlayerPrefs.Save();
        Debug.Log($"[FogOfWar] {sceneName}: 안개 데이터 저장됨. 픽셀 수: {_exploredPixels.Length}, JSON 길이: {json.Length}");
        // Debug.Log($"[FogOfWar] {sceneName}: 저장된 JSON: {json}"); // 너무 길 수 있으므로 필요할 때만 활성화
    }

    private void LoadFogData(string sceneName)
    {
        Debug.Log($"[FogOfWar] {sceneName}: 안개 데이터 로드 시도...");
        if (PlayerPrefs.HasKey($"FogData_{sceneName}"))
        {
            string json = PlayerPrefs.GetString($"FogData_{sceneName}");
            Debug.Log($"[FogOfWar] {sceneName}: 저장된 안개 데이터 발견. JSON 길이: {json.Length}");
            // Debug.Log($"[FogOfWar] {sceneName}: 불러온 JSON: {json}"); // 너무 길 수 있으므로 필요할 때만 활성화
            try
            {
                FogData data = JsonUtility.FromJson<FogData>(json);

                // 초기화 및 데이터 적용
                InitializeFogInternal(); // Initialize _exploredPixels and _isExplored arrays
                _isExplored = data.isExplored; // Load the boolean explored status

                // Reconstruct _exploredPixels based on the loaded _isExplored status
                for (int i = 0; i < _isExplored.Length; i++)
                {
                    if (_isExplored[i])
                    {
                        _exploredPixels[i] = exploredColor; // Set to explored color if true
                    }
                    else
                    {
                        _exploredPixels[i] = unexploredColor; // Set to unexplored color (black) if false
                    }
                }

                CalculateInitialExploredCount();
                UpdateExploredStatusTextureOnGPU();
                Debug.Log($"[FogOfWar] {sceneName}: 안개 데이터 불러옴. 픽셀 수: {_exploredPixels.Length}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FogOfWar] {sceneName}: 안개 데이터 로드 실패 (JSON 파싱 오류): {e.Message}. 새로운 데이터로 초기화합니다.");
                PlayerPrefs.DeleteKey($"FogData_{sceneName}"); // 손상된 데이터 삭제
                InitializeFogInternal();
                InitializeFogState(); // 모든 픽셀을 미탐험 상태로 초기화
            }
        }
        else
        {
            // 저장된 데이터가 없으면 새로 초기화
            Debug.Log($"[FogOfWar] {sceneName}: 저장된 안개 데이터 없음. 새로운 데이터로 초기화합니다.");
            InitializeFogInternal();
            InitializeFogState(); // 모든 픽셀을 미탐험 상태로 초기화
        }
        InitializeView(); // 뷰 초기화는 항상 필요
    }

    private void InitializeFogInternal()
    {
        // 1. 최종 결과물 렌더 텍스처 생성
        _finalFogTexture = new RenderTexture(textureSize, textureSize, 0);
        fogDisplayMaterial.mainTexture = _finalFogTexture;

        // 2. CPU에서 탐험 상태를 관리할 텍스처와 배열 초기화
        _exploredStatusTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        _exploredPixels = new Color[textureSize * textureSize];
        _isExplored = new bool[textureSize * textureSize];

        // 머티리얼에 생성된 텍스처를 연결.
        fogDisplayMaterial.SetTexture("_BaseMap", _exploredStatusTexture);
    }

    private void InitializeFogState()
    {
        _exploredPixelCount = 0;
        // 모든 픽셀을 '미탐험' 상태로 초기화
        for (int i = 0; i < _exploredPixels.Length; i++)
        {
            _exploredPixels[i] = unexploredColor;
            _isExplored[i] = false;
        }
        UpdateExploredStatusTextureOnGPU();
    }

    private void LateUpdate()
    {
        // RenderTexture가 존재하면 전역 셰이더 변수로 전달
        if (visibilityRenderTexture != null)
        {
            Shader.SetGlobalTexture("_FogTex", visibilityRenderTexture);
        }

        Shader.SetGlobalFloat("_FogScale", textureSize);
        Shader.SetGlobalVector("_FogOffset", Vector3.zero);

        var tex = Shader.GetGlobalTexture("_FogTex");
    }

    // 전장의 안개 관련 초기화 (이전 InitializeFog를 분리)
    void InitializeFog()
    {
        // 이 메서드는 이제 LoadFogData에서 호출되거나, LoadFogData가 없을 때 직접 호출됨
        // InitializeFogInternal과 InitializeFogState로 분리됨
    }

    // 시야 초기화
    void InitializeView()
    {
        // 감지할 레이어 마스크 제작
        _combinedLayerMask = viewBlockerLayer | enemyLayer;

        // 가시성 메시를 위한 오브젝트 생성
        if (_visibilityMeshObject == null)
        {
            _visibilityMeshObject = new GameObject("VisibilityMesh");
            _visibilityMeshObject.transform.SetParent(transform);
            _visibilityMeshObject.layer = LayerMask.NameToLayer("FogOfVisible");

            _meshFilter = _visibilityMeshObject.AddComponent<MeshFilter>();
            _meshRenderer = _visibilityMeshObject.AddComponent<MeshRenderer>();

            // 메시에 사용할 머티리al 설정 (중요)
            _meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _meshRenderer.material.color = Color.white;
            _meshRenderer.enabled = false;

            _visibilityMesh = new Mesh();
            _meshFilter.mesh = _visibilityMesh;

            // 메시 배열 초기화 (rayCount에 따라 크기 결정)
            _vertices = new Vector3[rayCount + 1]; // 중심점(1) + 광선 끝점(rayCount)
            _triangles = new int[rayCount * 3];
        }
    }

    private IEnumerator UpdateFogCoroutine()
    {
        while (true)
        {
            // 이전 위치를 모두 탐험한 색깔로 처리
            UpdatePreviousLoc();

            // 1. 유닛들의 현재 위치를 기반으로 실시간 가시성 메시를 업데이트
            UpdateVisibilityMesh();

            // 2. 생성된 메시를 기반으로 CPU에서 직접 탐험 상태를 기록
            UpdateExploredStatusFromMeshCPU();
            UpdateExploredStatusTextureOnGPU();

            yield return new WaitForFixedUpdate();
        }
    }

    void UpdateVisibilityMesh()
    {
        // 시야 유닛이 없을 때, 모두 제거
        if (VisionUnits.Count == 0)
        {
            foreach (var enemy in _visibleEnemies)
            {
                if (enemy != null && logVisibilityChanges)
                    Debug.Log($"[시야 소실] {enemy.name}이(가) 사라졌습니다.");
            }
            _visibilityMesh.Clear();
            _visibleEnemies.Clear();
            return;
        }

        // 현재 프레임에서 시야에 들어온 유닛들을 임시로 저장할 HashSet 생성
        HashSet<Transform> newlyFoundUnits = new HashSet<Transform>();

        // 현재는 첫 번째 유닛만 지원. 여러 유닛을 지원하려면 로직 수정 필요
        Transform unit = VisionUnits[0];
        float sightRadiusWorld = 15f;

        _vertices[0] = unit.position; // 메시의 중심점은 항상 유닛의 시야 기준점
        float angleIncrement = 360 / rayCount;

        // 캐릭터의 정면 방향을 가져옵니다. (Y축은 무시하여 수평 방향만 사용)
        Vector3 forwardDirection = unit.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;

            // 설정한 각도에 따라 전방과 원형 시야 거리 다르게 설정
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            float angleToForward = Vector3.Angle(forwardDirection, direction);
            if (angleToForward <= (rayRotateRange + visionAngleBonus))
                sightRadiusWorld = rayForwardMaxDistance;
            else
                sightRadiusWorld = rayCircleMaxDistance;

            // 1. RaycastAll을 사용하여 광선에 닿는 모든 오브젝트를 가져옵니다.
            RaycastHit[] hits = Physics.RaycastAll(_vertices[0], direction, sightRadiusWorld, _combinedLayerMask);

            // 2. 시야를 막는 가장 가까운 장애물의 거리를 찾습니다.
            float closestBlockerDistance = sightRadiusWorld; // 기본값은 최대 시야 거리
            foreach (var hit in hits)
            {
                // 부딪힌 오브젝트가 'ViewBlocker' 레이어라면
                if (((1 << hit.collider.gameObject.layer) & viewBlockerLayer) != 0)
                {
                    // 가장 가까운 거리를 갱신합니다.
                    if (hit.distance < closestBlockerDistance)
                    {
                        closestBlockerDistance = hit.distance;
                    }
                }
            }

            // 3. 감지된 모든 적에 대해, 시야를 막는 장애물보다 앞에 있는지 확인합니다.
            foreach (var hit in hits)
            {
                // 부딪힌 오브젝트가 'Enemy' 레이어이고,
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
                {
                    // 가장 가까운 장애물보다 앞에 있다면,
                    if (hit.distance < closestBlockerDistance)
                    {
                        Debug.Log(hit);
                        // 보이는 적 목록에 추가합니다.
                        newlyFoundUnits.Add(hit.transform);
                    }
                }
            }

            // 4. 최종 시야 메시의 정점 위치는 가장 가까운 장애물까지로 설정합니다.
            Vector3 vertexPosition = _vertices[0] + direction * closestBlockerDistance;
            _vertices[i + 1] = vertexPosition;
        }

        // 2-1. 새로 시야에 들어온 적을 찾아서 출력합니다.
        foreach (var enemy in newlyFoundUnits)
        {
            // '새로 찾은 목록'에는 있는데 '이전 목록'에는 없다면 => 새로 나타난 적입니다.
            if (_visibleEnemies.Contains(enemy))
                continue;

            if (logVisibilityChanges)
                Debug.Log($"[시야 포착] {enemy.name}이(가) 나타났습니다.");

            if (enemy.gameObject.TryGetComponent(out IUnitWithFog unitWithFog))
                unitWithFog.OnMeshActive();
        }

        // 2-2. 시야에서 사라진 적을 찾아서 출력합니다.
        foreach (var enemy in _visibleEnemies)
        {
            // '이전 목록'에는 있었는데 '새로 찾은 목록'에는 없다면 => 사라진 적입니다.
            if (newlyFoundUnits.Contains(enemy))
                continue;

            if (enemy == null) // 유닛이 파괴되어 null이 된 경우를 대비
                continue;

            if (logVisibilityChanges)
                Debug.Log($"[시야 소실] {enemy.name}이(가) 사라졌습니다.");

            if (enemy.gameObject.TryGetComponent(out IUnitWithFog unitWithFog))
                unitWithFog.OnMeshInactive();
        }

        // 2-3. 다음 프레임을 위해, 현재 보이는 적 목록(VisibleEnemies)을 최신 상태로 업데이트합니다.
        _visibleEnemies.Clear();
        foreach (var enemy in newlyFoundUnits)
        {
            _visibleEnemies.Add(enemy);
        }

        // 메시 생성
        for (int i = 0; i < rayCount; i++)
        {
            _triangles[i * 3 + 0] = 0;
            _triangles[i * 3 + 1] = i + 1;
            _triangles[i * 3 + 2] = (i == rayCount - 1) ? 1 : i + 2;
        }

        _visibilityMesh.Clear();
        _visibilityMesh.vertices = _vertices;
        _visibilityMesh.triangles = _triangles;
        _visibilityMesh.RecalculateBounds();
    }

    // CPU 데이터를 GPU 텍스처(_exploredStatusTexture)로 업로드하는 함수
    void UpdateExploredStatusTextureOnGPU()
    {
        _exploredStatusTexture.SetPixels(_exploredPixels);
        _exploredStatusTexture.Apply();
    }

    // 메시의 모든 삼각형을 순회하며 래스터라이즈(픽셀화)를 요청하는 메인 함수
    void UpdateExploredStatusFromMeshCPU()
    {
        // 메시의 삼각형 배열과 정점 배열을 가져옴
        int[] triangles = _visibilityMesh.triangles;
        Vector3[] vertices = _visibilityMesh.vertices;

        // 삼각형 배열은 3개의 인덱스가 하나의 삼각형을 의미하므로, 3씩 건너뛰며 순회
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // 현재 삼각형을 구성하는 세 정점의 월드 좌표를 가져옴
            Vector3 v0_world = vertices[triangles[i]];
            Vector3 v1_world = vertices[triangles[i + 1]];
            Vector3 v2_world = vertices[triangles[i + 2]];

            // 각 정점의 월드 좌표를 텍스처 좌표로 변환
            Vector2Int v0_tex = WorldToTextureCoordinates(v0_world);
            Vector2Int v1_tex = WorldToTextureCoordinates(v1_world);
            Vector2Int v2_tex = WorldToTextureCoordinates(v2_world);


            // 변환된 텍스처 좌표를 사용해 삼각형 내부를 픽셀로 채움
            // RasterizeTriangle 내부에서 _updatePixels에 추가하고 _exploredPixels를 업데이트
            RasterizeTriangle(v0_tex, v1_tex, v2_tex);
        }
    }

    // 세 개의 텍스처 좌표로 정의된 삼각형 내부를 픽셀로 채우는 함수 (Scanline Algorithm)
    void RasterizeTriangle(Vector2Int p0, Vector2Int p1, Vector2Int p2)
    {
        // 정점을 Y좌표 기준으로 정렬 (p0가 가장 위, p2가 가장 아래)
        if (p0.y > p1.y) { var temp = p0; p0 = p1; p1 = temp; }
        if (p0.y > p2.y) { var temp = p0; p0 = p2; p2 = temp; }
        if (p1.y > p2.y) { var temp = p1; p1 = p2; p2 = temp; }

        // 채워 넣기
        int total_height = p2.y - p0.y;
        for (int y = p0.y; y <= p2.y; y++)
        {
            if (y < 0 || y >= textureSize) continue;

            bool second_half = y > p1.y || p1.y == p0.y;
            int segment_height = second_half ? p2.y - p1.y : p1.y - p0.y;

            float alpha = (float)(y - p0.y) / total_height;
            float beta = (float)(y - (second_half ? p1.y : p0.y)) / segment_height;

            Vector2 A = p0 + (Vector2)(p2 - p0) * alpha;
            Vector2 B = second_half ? p1 + (Vector2)(p2 - p1) * beta : p0 + (Vector2)(p1 - p0) * beta;

            if (A.x > B.x) { var temp = A; A = B; B = temp; }

            for (int x = (int)A.x; x <= (int)B.x; x++)
            {
                if (x < 0 || x >= textureSize) continue;

                int index = y * textureSize + x;


                if (!_isExplored[index])
                {
                    _exploredPixelCount++;
                    _isExplored[index] = true;
                    _newlyExploredPixelIndices.Add(index); // NEW: Add to list

                    if (ExplorationStatManager.instance != null)
                    {
                        ExplorationStatManager.instance.OnPixelExplored(_isExplored.Length);
                    }
                }

                _updatePixels.Enqueue(index);
                _exploredPixels[index] = _visibleColor;

            }
        }
    }

    // 이전에 업데이트된 위치를 모두 이전에 탐험되었다고 업데이트
    private bool UpdatePreviousLoc()
    {
        while (_updatePixels.Count > 0)
        {
            int idx = _updatePixels.Dequeue();
            _exploredPixels[idx] = exploredColor;
        }

        return true;
    }

    // 월드 좌표를 텍스처 좌표로 변환하는 함수
    private Vector2Int WorldToTextureCoordinates(Vector3 worldPosition)
    {
        float normalizedX = (worldPosition.x / worldSize.x) + 0.5f;
        float normalizedZ = (worldPosition.z / worldSize.y) + 0.5f;
        int x = Mathf.FloorToInt(normalizedX * textureSize);
        int z = Mathf.FloorToInt(normalizedZ * textureSize);
        x = Mathf.Clamp(x, 0, textureSize - 1);
        z = Mathf.Clamp(z, 0, textureSize - 1);
        return new Vector2Int(x, z);
    }

    // --- 월드맵 UI에 텍스처를 제공하기 위한 새로운 함수 ---
    /// <summary>
    /// 현재 씬의 탐험 상태가 기록된 안개 텍스처를 반환합니다.
    /// </summary>
    public Texture2D GetExploredMapTexture()
    {
        return _exploredStatusTexture;
    }

    public float GetExplorationPercentage()
    {
        if (_isExplored == null || _isExplored.Length == 0)
        {
            return 0f;
        }
        return (float)_exploredPixelCount / _isExplored.Length * 100f;
    }

    private void CalculateInitialExploredCount()
    {
        _exploredPixelCount = 0;
        if (_isExplored == null) return;
        for (int i = 0; i < _isExplored.Length; i++)
        {
            if (_isExplored[i])
            {
                _exploredPixelCount++;
            }
        }
    }

    // 씬 뷰에서 디버깅 정보를 시각적으로 그립니다. 게임 빌드에는 포함되지 않습니다.
    private void OnDrawGizmos()
    {
        // 기즈모 표시 옵션이 꺼져있으면 아무것도 그리지 않습니다.
        if (!showVisibleEnemyGizmos) return;

        // VisibleEnemies 목록이 비어있으면 그릴 필요가 없습니다.
        if (_visibleEnemies == null || _visibleEnemies.Count == 0) return;

        // 기즈모의 색상을 설정합니다.
        Gizmos.color = gizmoColor;

        // 현재 보이는 모든 적들의 위치에 원을 그립니다.
        foreach (var enemy in _visibleEnemies)
        {
            if (enemy != null)
            {
                // 적의 위치에 반지름 1.5 크기의 와이어 구체를 그립니다.
                Gizmos.DrawWireSphere(enemy.position, 1.5f);
            }
        }
    }
}