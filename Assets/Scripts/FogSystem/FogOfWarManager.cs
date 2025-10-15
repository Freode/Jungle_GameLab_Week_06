using System.Collections.Generic;
using System.Collections;
using UnityEngine;

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
    public Color exploredColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    private readonly Color _visibleColor = new Color(0, 0, 0, 0); // 시야 안 (완전 투명)

    [Header("View Quality")]
    public LayerMask viewBlockerLayer; // 인스펙터에서 'ViewBlocker' 레이어를 선택해주세요.
    public int rayCount = 36;          // 원형 시야를 위한 레이캐스트 개수 (높을수록 정밀하지만 부하 증가)
    public float rayMaxDistance = 50f; // 레이캐스트 최대 거리 (캐릭터 시야 반경보다 충분히 길게)

    [Header("Render Textures")]
    public RenderTexture visibilityRenderTexture; // 가시성 메시를 그릴 렌더 텍스처

    // === 내부 관리 변수 ===
    private Texture2D _exploredStatusTexture; // 탐험 상태만 저장하는 CPU 텍스처
    private RenderTexture _finalFogTexture;   // 최종 결과물 렌더 텍스처
    private Color[] _exploredPixels;
    private bool[] _isExplored;

    // 탐험 상태가 바뀌었을 때만 CPU->GPU로 텍스처를 업데이트하기 위한 최적화 플래그
    private bool _exploredTextureNeedsUpdate = true;

    // 시야 영역을 그려주는 내부 mesh 변수
    private GameObject _visibilityMeshObject;
    private Mesh _visibilityMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private Vector3[] _vertices;
    private int[] _triangles;

    private Camera _mainCamera;
    // 시야를 밝혀주는 모든 유닛의 Transform 리스트 (static으로 선언하여 어디서든 접근 가능)
    public static List<Transform> VisionUnits = new List<Transform>();

    void Start()
    {
        _mainCamera = Camera.main;
        //InitializeSelf();
        InitializeFog();
        InitializeView();
        StartCoroutine(UpdateFogCoroutine());
    }

    // Fog Manager 초기화
    void InitializeSelf()
    {
        // 반드시 월드 원점(0,0,0)에 위치해야 좌표가 어긋나지 않습니다.
        _visibilityMeshObject.transform.position = Vector3.zero;
        _visibilityMeshObject.transform.rotation = Quaternion.identity;
        _visibilityMeshObject.transform.localScale = Vector3.one;
    }

    // 전장의 안개 관련 초기화
    void InitializeFog()
    {
        // 1. 최종 결과물 렌더 텍스처 생성
        _finalFogTexture = new RenderTexture(textureSize, textureSize, 0);
        fogDisplayMaterial.mainTexture = _finalFogTexture;

        // 2. CPU에서 탐험 상태를 관리할 텍스처와 배열 초기화
        _exploredStatusTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        _exploredPixels = new Color[textureSize * textureSize];
        _isExplored = new bool[textureSize * textureSize];

        // 모든 픽셀을 '미탐험' 상태로 초기화
        for (int i = 0; i < _exploredPixels.Length; i++)
        {
            _exploredPixels[i] = unexploredColor;
            _isExplored[i] = false;
        }
        UpdateExploredStatusTextureOnGPU();

        // 머티리얼에 생성된 텍스처를 연결.
        // URP Unlit 셰이더의 기본 텍스처 프로퍼티 이름은 "_BaseMap" 입니다.
        fogDisplayMaterial.SetTexture("_BaseMap", _exploredStatusTexture);
    }

    // 시야 초기화
    void InitializeView()
    {
        // 가시성 메시를 위한 오브젝트 생성
        _visibilityMeshObject = new GameObject("VisibilityMesh");
        _visibilityMeshObject.transform.SetParent(transform);
        _visibilityMeshObject.layer = LayerMask.NameToLayer("VisibleInFog");

        _meshFilter = _visibilityMeshObject.AddComponent<MeshFilter>();
        _meshRenderer = _visibilityMeshObject.AddComponent<MeshRenderer>();

        // 메시에 사용할 머티리얼 설정 (중요)
        // 메시를 그릴 머티리얼은 복잡한 셰이더가 필요 없으므로 단순한 단색 셰이더로 변경
        _meshRenderer.material = new Material(Shader.Find("Unlit/Color"));
        _meshRenderer.material.color = Color.white;

        _visibilityMesh = new Mesh();
        _meshFilter.mesh = _visibilityMesh;

        // 메시 배열 초기화 (rayCount에 따라 크기 결정)
        _vertices = new Vector3[rayCount + 1]; // 중심점(1) + 광선 끝점(rayCount)
        _triangles = new int[rayCount * 3];

    }

    private IEnumerator UpdateFogCoroutine()
    {
        while (true)
        {
            // 1. 유닛들의 현재 위치를 기반으로 실시간 가시성 메시를 업데이트
            UpdateVisibilityMesh();

            // 2. (최적화) 탐험 상태가 변경되었을 때만 CPU 데이터를 GPU 텍스처로 업로드
            if (_exploredTextureNeedsUpdate)
            {
                UpdateExploredStatusTextureOnGPU();
                _exploredTextureNeedsUpdate = false;
            }

            // 3. [핵심] GPU를 사용해 두 텍스처를 최종 안개 텍스처로 합성
            fogCombineMaterial.SetTexture("_MaskTex", visibilityRenderTexture);
            Graphics.Blit(_exploredStatusTexture, _finalFogTexture, fogCombineMaterial);

            yield return new WaitForFixedUpdate();
        }
    }

    void UpdateVisibilityMesh()
    {
        if (VisionUnits.Count == 0)
        {
            _visibilityMesh.Clear();
            return;
        }

        // 현재는 첫 번째 유닛만 지원. 여러 유닛을 지원하려면 로직 수정 필요
        Transform unit = VisionUnits[0];
        float sightRadiusWorld = 15f;

        _vertices[0] = unit.position; // 메시의 중심점은 항상 유닛의 시야 기준점
        float angleIncrement = 360f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

            RaycastHit hit;
            Vector3 vertexPosition;

            if (Physics.Raycast(_vertices[0], direction, out hit, sightRadiusWorld, viewBlockerLayer))
            {
                vertexPosition = hit.point;
            }
            else
            {
                vertexPosition = _vertices[0] + direction * sightRadiusWorld;
            }
            _vertices[i + 1] = vertexPosition;

            // 시야가 닿은 곳까지의 탐험 상태를 CPU 데이터에 기록
            MarkLineAsExplored(WorldToTextureCoordinates(_vertices[0]),WorldToTextureCoordinates(vertexPosition));
        }

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

    // CPU 데이터(배열)에 '탐험됨' 상태를 기록하는 함수
    void MarkLineAsExplored(Vector2Int start, Vector2Int end)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < textureSize && y0 >= 0 && y0 < textureSize)
            {
                int index = y0 * textureSize + x0;
                if (!_isExplored[index])
                {
                    _isExplored[index] = true;
                    _exploredPixels[index] = exploredColor;
                    _exploredTextureNeedsUpdate = true; // 변경이 있었음을 플래그로 알림
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
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
}
