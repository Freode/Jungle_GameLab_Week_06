using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldMapUI : MonoBehaviour
{
    [Header("World Map Components")]
    public GameObject mapPanel;
    public Image mapImage;
    public RectTransform playerIcon;
    public Transform markerParent;

    [Header("Minimap Components")]
    public GameObject minimapPanel;
    public Image minimapImage;
    public RectTransform minimapPlayerIcon;
    public Transform minimapMarkerParent;

    [Header("Common Components")]
    public FogOfWarManager fogManager;
    public Transform playerTransform;
    public TextMeshProUGUI textScene;
    public GameObject markerPrefab;
    public TextMeshProUGUI explorationPercentageText;

    [Header("Map Bounds Setup")]
    [Tooltip("맵의 왼쪽 아래 모서리에 위치시킬 오브젝트")]
    public Transform worldBounds_BottomLeft;
    [Tooltip("맵의 오른쪽 위 모서리에 위치시킬 오브젝트")]
    public Transform worldBounds_TopRight;

    private Material _mapMaterialInstance;
    private Material _minimapMaterialInstance;
    private List<GameObject> _activeMarkers = new List<GameObject>();
    private List<GameObject> _activeMinimapMarkers = new List<GameObject>();
    private Rect _mapWorldBounds;

    void OnEnable()
    {
        MarkerManager.OnMarkerAdded += UpdateAllMarkers; // 마커 추가 이벤트 구독
    }

    void OnDisable()
    {
        MarkerManager.OnMarkerAdded -= UpdateAllMarkers; // 마커 추가 이벤트 구독 해제
    }

    void Awake()
    {
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
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (mapImage == null || minimapImage == null || fogManager == null)
        {
            Debug.LogError("WorldMapUI에 필요한 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        textScene.text = SceneManager.GetActiveScene().name;

        // Create separate material instances for the world map and minimap
        _mapMaterialInstance = Instantiate(mapImage.material);
        mapImage.material = _mapMaterialInstance;

        _minimapMaterialInstance = Instantiate(minimapImage.material);
        minimapImage.material = _minimapMaterialInstance;

        Texture fogTexture = fogManager.GetExploredMapTexture();
        if (fogTexture != null)
        {
            _mapMaterialInstance.SetTexture("_FogTex", fogTexture);
            _minimapMaterialInstance.SetTexture("_FogTex", fogTexture);
        }

        minimapPanel.SetActive(true);
        mapPanel.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Always update the minimap
        UpdateMinimap();

        // Only update the world map if it's active
        if (mapPanel.activeSelf)
        {
            UpdateWorldMap();
        }

        if (fogManager != null && explorationPercentageText != null)
        {
            float percentage = fogManager.GetExplorationPercentage();
            explorationPercentageText.text = $"Explored: <color=#00FF00>{percentage:F2}%</color>";
        }
    }

    void UpdateWorldMap()
    {
        // World map logic: player icon moves, map is static
        SetIconPosition(playerIcon, playerTransform.position, mapImage.rectTransform);
        UpdateMarkers(markerParent, ref _activeMarkers, false);
    }

    void UpdateMinimap()
    {
        // Minimap logic: map moves, player icon is static in the center of the mask
        RectTransform minimapImageRect = minimapImage.rectTransform;

        float normalizedX = (playerTransform.position.x - _mapWorldBounds.x) / _mapWorldBounds.width;
        float normalizedY = (playerTransform.position.z - _mapWorldBounds.y) / _mapWorldBounds.height;

        // This calculation assumes the minimapImage has a center pivot (0.5, 0.5).
        // It moves the map so the player's normalized position on the map is at the center of the panel.
        float newX = -(normalizedX - 0.5f) * minimapImageRect.rect.width;
        float newY = -(normalizedY - 0.5f) * minimapImageRect.rect.height;

        minimapImage.rectTransform.anchoredPosition = new Vector2(newX, newY);

        UpdateMarkers(minimapMarkerParent, ref _activeMinimapMarkers, true);
    }

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        minimapPanel.SetActive(false);
        UpdateWorldMap();
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
        minimapPanel.SetActive(true);
    }

    void UpdateAllMarkers()
    {
        UpdateMarkers(markerParent, ref _activeMarkers, false);
        UpdateMarkers(minimapMarkerParent, ref _activeMinimapMarkers, true);
    }

    void UpdateMarkers(Transform parent, ref List<GameObject> activeMarkers, bool isMinimap)
    {
        foreach (var marker in activeMarkers)
        {
            Destroy(marker);
        }
        activeMarkers.Clear();

        if (MarkerManager.instance == null || markerPrefab == null || parent == null) return;

        List<MapMarkerData> markers = MarkerManager.instance.GetMarkersForCurrentScene();

        foreach (var markerData in markers)
        {
            GameObject markerGO = Instantiate(markerPrefab, parent);
            UI_MapMarker markerUI = markerGO.GetComponent<UI_MapMarker>();
            if (markerUI != null)
            {
                markerUI.SetText(markerData.text);
                markerUI.SetColor(markerData.color);
            }

            SetIconPosition(markerGO.GetComponent<RectTransform>(), markerData.position, isMinimap ? minimapImage.rectTransform : mapImage.rectTransform);
            activeMarkers.Add(markerGO);
        }
    }

    void SetIconPosition(RectTransform iconRect, Vector3 worldPosition, RectTransform mapRect)
    {
        // This logic calculates the marker's position on its parent map texture.
        // It requires the marker parent to be a child of the map image.
        float normalizedX = (worldPosition.x - _mapWorldBounds.x) / _mapWorldBounds.width;
        float normalizedY = (worldPosition.z - _mapWorldBounds.y) / _mapWorldBounds.height;

        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.zero;

        float newX = normalizedX * mapRect.rect.width;
        float newY = normalizedY * mapRect.rect.height;

        iconRect.anchoredPosition = new Vector2(newX, newY);
    }
}