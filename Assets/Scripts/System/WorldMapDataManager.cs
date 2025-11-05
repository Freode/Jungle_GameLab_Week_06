using UnityEngine;

public class WorldMapDataManager : MonoBehaviour
{
    public static WorldMapDataManager instance;

    [Header("World Map Settings")]
    public int worldMapResolution = 2048; // 전체 월드맵 텍스처의 해상도
    public Vector2 totalWorldSize = new Vector2(2048, 2048); // 게임 월드의 전체 크기 (미터 단위)
    public Shader stampShader; // 아래에서 만들 합성용 셰이더

    private RenderTexture _worldMapTexture; // 최종 월드맵 텍스처
    private Material _stampMaterial;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // 1. 최종 결과물이 될 영구적인 RenderTexture 생성
        _worldMapTexture = new RenderTexture(worldMapResolution, worldMapResolution, 0, RenderTextureFormat.ARGB32);
        _worldMapTexture.Create();

        // 2. 최초의 월드맵은 완전히 검은색(미탐험)으로 채움
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = _worldMapTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = activeRT;

        // 3. 합성용 머티리얼 생성
        _stampMaterial = new Material(stampShader);
    }

    // 월드맵 텍스처를 외부(UI)에 제공하는 함수
    public Texture GetWorldMapTexture()
    {
        return _worldMapTexture;
    }

    // 지역 씬의 안개 텍스처를 월드맵에 합성(스탬프)하는 함수
    public void StampFogTexture(Texture localFogTexture, Rect targetArea)
    {
        // targetArea는 월드맵(0~1)의 어느 영역에 스탬프를 찍을지 정의
        _stampMaterial.SetTexture("_StampTex", localFogTexture);
        _stampMaterial.SetVector("_TargetArea", new Vector4(targetArea.x, targetArea.y, targetArea.width, targetArea.height));

        // 임시 RenderTexture를 사용해 스탬프 작업을 수행
        RenderTexture tempRT = RenderTexture.GetTemporary(_worldMapTexture.width, _worldMapTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(_worldMapTexture, tempRT); // 현재 월드맵을 임시 텍스처에 복사
        Graphics.Blit(tempRT, _worldMapTexture, _stampMaterial); // 스탬프 머티리얼을 사용해 월드맵에 덮어쓰기
        RenderTexture.ReleaseTemporary(tempRT);

        Debug.Log($"월드맵에 {localFogTexture.name}을(를) {targetArea} 영역에 스탬핑했습니다.");
    }
}
