using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldMapUI : MonoBehaviour
{
    [Header("Required Components")]
    public FogOfWarManager fogManager;
    public Image mapImage;
    public RectTransform playerIcon;
    public Transform playerTransform; // 실시간 위치 추적을 위한 플레이어 Transform
    public TextMeshProUGUI textScene;   // Scene 이름 출력

    [Header("UI Panel")]
    public GameObject mapPanel;

    [Header("Map Bounds Setup")]
    [Tooltip("맵의 왼쪽 아래 모서리에 위치시킬 오브젝트")]
    public Transform worldBounds_BottomLeft;
    [Tooltip("맵의 오른쪽 위 모서리에 위치시킬 오브젝트")]
    public Transform worldBounds_TopRight;

    [Header("Map Markers")]
    public GameObject markerPrefab;
    public Transform markerParent;

    private Material _mapMaterialInstance;
    private List<GameObject> _activeMarkers = new List<GameObject>();
    private Rect _mapWorldBounds;

    void OnEnable()
    {
        MarkerManager.OnMarkerAdded += UpdateMarkers; // 마커 추가 이벤트 구독
    }

    void OnDisable()
    {
        MarkerManager.OnMarkerAdded -= UpdateMarkers; // 마커 추가 이벤트 구독 해제
    }

    void Awake()
    {
        // 기준점 오브젝트로부터 월드 경계를 자동으로 계산
        if (worldBounds_BottomLeft != null && worldBounds_TopRight != null)
        {
            _mapWorldBounds = new Rect(
                worldBounds_BottomLeft.position.x,
                worldBounds_BottomLeft.position.z, // Y가 아닌 Z를 사용
                worldBounds_TopRight.position.x - worldBounds_BottomLeft.position.x,
                worldBounds_TopRight.position.z - worldBounds_BottomLeft.position.z
            );
        }
    }

    void Start()
    {
        if (mapImage == null || fogManager == null)
        {
            Debug.LogError("SceneMapUI에 필요한 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        textScene.text = SceneManager.GetActiveScene().name;

        _mapMaterialInstance = Instantiate(mapImage.material);
        mapImage.material = _mapMaterialInstance;

        Texture fogTexture = fogManager.GetExploredMapTexture();
        if (fogTexture != null)
        {
            _mapMaterialInstance.SetTexture("_FogTex", fogTexture);
        }
    }

    // 실시간 업데이트를 위한 Update 함수 추가
    void Update()
    {
        // 맵 패널이 활성화되어 있을 때만 플레이어 아이콘 위치를 계속 업데이트
        if (mapPanel.activeSelf && playerTransform != null)
        {
            UpdatePlayerIcon(playerTransform.position);
        }
    }

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        if (playerTransform != null) UpdatePlayerIcon(playerTransform.position);
        UpdateMarkers();
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
    }

    private void UpdatePlayerIcon(Vector3 playerWorldPosition)
    {
        if (playerIcon == null) return;
        SetIconPosition(playerIcon, playerWorldPosition);
        playerIcon.gameObject.SetActive(true);
    }

    private void UpdateMarkers()
    {
        foreach (var marker in _activeMarkers)
        {
            Destroy(marker);
        }
        _activeMarkers.Clear();

        if (MarkerManager.instance == null || markerPrefab == null || markerParent == null) return;

        List<MapMarkerData> markers = MarkerManager.instance.GetMarkersForCurrentScene();

        foreach (var markerData in markers)
        {
            GameObject markerGO = Instantiate(markerPrefab, markerParent);
            UI_MapMarker markerUI = markerGO.GetComponent<UI_MapMarker>();
            if (markerUI != null) markerUI.SetText(markerData.text);

            // [디버그] 로드된 위치 좌표를 정확히 기록
            Debug.Log($"[LOAD] 깃발 아이콘 생성 시도. 저장된 위치: {markerData.position.ToString("F4")}");

            SetIconPosition(markerGO.GetComponent<RectTransform>(), markerData.position);
            _activeMarkers.Add(markerGO);
        }
    }

    private void SetIconPosition(RectTransform iconRect, Vector3 worldPosition)
    {
        float normalizedX = (worldPosition.x - _mapWorldBounds.x) / _mapWorldBounds.width;
        float normalizedY = (worldPosition.z - _mapWorldBounds.y) / _mapWorldBounds.height;

        // Set anchors to bottom-left for direct anchoredPosition calculation
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.zero;

        // Calculate position relative to the map panel's bottom-left corner
        RectTransform mapRect = mapImage.rectTransform;
        float newX = normalizedX * mapRect.rect.width;
        float newY = normalizedY * mapRect.rect.height;
        
        iconRect.anchoredPosition = new Vector2(newX, newY);
    }
}