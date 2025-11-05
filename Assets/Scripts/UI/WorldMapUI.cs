using UnityEngine;
using UnityEngine.UI;

// 기존 파일 이름(WorldMapUI.cs)과 클래스 이름을 일치시켜 컴파일 오류를 방지합니다.
public class WorldMapUI : MonoBehaviour
{
    [Header("Required Components")]
    [Tooltip("이 씬의 안개를 관리하는 FogOfWarManager를 할당하세요.")]
    public FogOfWarManager fogManager;

    [Tooltip("지도를 표시할 UI Image 컴포넌트를 할당하세요.")]
    public Image mapImage;

    [Tooltip("플레이어 위치를 나타낼 RectTransform을 할당하세요.")]
    public RectTransform playerIcon;

    [Header("UI Panel")]
    public GameObject mapPanel;

    private Material _mapMaterialInstance; // 인스턴스화된 맵 머티리얼

    void Start()
    {
        if (mapImage == null || fogManager == null)
        {
            Debug.LogError("WorldMapUI에 필요한 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        // 런타임에서 사용할 머티리얼 인스턴스를 생성하여 원본 머티리얼에 영향을 주지 않도록 함
        _mapMaterialInstance = Instantiate(mapImage.material);
        mapImage.material = _mapMaterialInstance;

        // FogOfWarManager로부터 안개 텍스처를 받아와서 맵 머티리얼의 _FogTex 속성에 전달
        Texture fogTexture = fogManager.GetExploredMapTexture();
        if (fogTexture != null)
        {
            _mapMaterialInstance.SetTexture("_FogTex", fogTexture);
        }
    }

    // 맵을 여는 함수
    public void OpenMap(Vector3 playerLocalPosition)
    {
        mapPanel.SetActive(true);
        UpdatePlayerIcon(playerLocalPosition);
    }

    // 맵을 닫는 함수
    public void CloseMap()
    {
        mapPanel.SetActive(false);
    }

    // 플레이어 아이콘 위치를 업데이트하는 함수
    private void UpdatePlayerIcon(Vector3 playerLocalPosition)
    {
        if (fogManager == null || playerIcon == null) return;

        // 플레이어의 로컬 좌표를 맵 이미지의 UI 좌표로 변환
        float normalizedX = (playerLocalPosition.x / fogManager.worldSize.x) + 0.5f;
        float normalizedY = (playerLocalPosition.z / fogManager.worldSize.y) + 0.5f;

        // 맵 이미지 RectTransform의 크기를 기준으로 아이콘 위치 계산
        Rect mapRect = (mapImage.transform as RectTransform).rect;
        float iconX = normalizedX * mapRect.width;
        float iconY = normalizedY * mapRect.height;

        playerIcon.anchoredPosition = new Vector2(iconX, iconY);
        playerIcon.gameObject.SetActive(true);
    }
}
