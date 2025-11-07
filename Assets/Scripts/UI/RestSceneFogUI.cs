using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RestSceneFogUI : MonoBehaviour
{
    public GameObject sceneMapEntryPrefab;  // 각 씬 맵을 표시할 프리팹
    public Transform contentParent;         // 생성된 프리팹들이 배치될 부모 Transform
    public GameObject toggleUI;             // 토클할 UI
    public KeyCode toggleKey = KeyCode.M;   // UI를 토글할 키

    // FogOfWarManager에서 사용하는 색상과 동일하게 설정
    public Color unexploredColor = Color.black;
    public Color exploredColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    private GameObject _fogPanelContainer; // 전체 UI를 담는 컨테이너 (토글용)

    void Awake()
    {
        // _fogPanelContainer는 이 스크립트가 붙은 GameObject 자체일 수 있습니다.
        _fogPanelContainer = toggleUI;
    }

    void Start()
    {
        if (sceneMapEntryPrefab == null)
        {
            Debug.LogError("Scene Map Entry Prefab이 할당되지 않았습니다.");
            return;
        }
        if (contentParent == null)
        {
            Debug.LogError("Content Parent가 할당되지 않았습니다.");
            return;
        }

        if (FogDataManager.instance == null)
        {
            Debug.LogError("FogDataManager 인스턴스를 찾을 수 없습니다. RestSceneFogUI가 작동하지 않습니다.");
            return;
        }
        if (MarkerManager.instance == null)
        {
            Debug.LogError("MarkerManager 인스턴스를 찾을 수 없습니다. 깃발 데이터를 불러올 수 없습니다.");
            return;
        }

        // 기존에 생성된 맵 엔트리들을 모두 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 각 씬의 안개 맵을 동적으로 생성하여 표시
        for (int i = 0; i < FogDataManager.instance.foggedSceneNames.Count; i++)
        {
            string sceneName = FogDataManager.instance.foggedSceneNames[i];
            Texture2D worldMapPNG = (i < FogDataManager.instance.worldMapPNGs.Count) ? FogDataManager.instance.worldMapPNGs[i] : null;
            Vector2 sceneWorldSize = (i < FogDataManager.instance.sceneWorldSizes.Count) ? FogDataManager.instance.sceneWorldSizes[i] : Vector2.zero;
            List<MapMarkerData> markers = MarkerManager.instance.GetAllMarkersForScene(sceneName);
            float percent = FogDataManager.instance.GetExplorationPercentage(sceneName);

            // 개별 씬의 안개 맵 텍스처 가져오기
            Texture2D sceneFogTexture = FogDataManager.instance.GetIndividualExploredMapTexture(
                sceneName,
                FogDataManager.instance.individualSceneTextureSize,
                unexploredColor,
                exploredColor
            );

            // 프리팹 인스턴스화
            GameObject entryGO = Instantiate(sceneMapEntryPrefab, contentParent);
            SceneMapEntry entry = entryGO.GetComponent<SceneMapEntry>();

            if (entry != null)
            {
                entry.Setup(sceneFogTexture, worldMapPNG, sceneName, markers, sceneWorldSize, percent);
            }
            else
            {
                Debug.LogWarning($"SceneMapEntry 컴포넌트를 찾을 수 없습니다. 프리팹: {sceneMapEntryPrefab.name}");
            }
        }

        // 초기에는 패널 비활성화
        if (_fogPanelContainer != null) _fogPanelContainer.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_fogPanelContainer != null)
            {
                _fogPanelContainer.SetActive(!_fogPanelContainer.activeSelf);
                Debug.Log($"통합 안개 맵 UI 패널 활성화 상태: {_fogPanelContainer.activeSelf}");
            }
        }
    }

    // 경험치 추가 (PlayerDataManager에 위임)
    public void AddExp(int amount)
    {
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.AddExp(amount);
        }
    }

    // 공격력 가져오기 (PlayerDataManager에 위임)
    int GetDamage()
    {
        if (PlayerDataManager.instance != null)
        {
            return (int)PlayerDataManager.instance.GetAttackPower();
        }
        return 0;
    }
}
