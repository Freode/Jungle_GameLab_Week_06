using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// JsonUtility가 List<MapMarkerData>를 직접 처리하지 못하는 문제를 해결하기 위한 래퍼 클래스
[System.Serializable]
public class MapMarkerDataList
{
    public List<MapMarkerData> markers = new List<MapMarkerData>();
}

public class MarkerManager : MonoBehaviour
{
    public static MarkerManager instance;
    public GameObject flagPrefab; // 깃발 프리팹을 할당하기 위한 public 변수

    public static event System.Action OnMarkerAdded; // 마커가 추가될 때 알리는 이벤트

    // Key: 씬 이름, Value: 해당 씬의 깃발 데이터 리스트를 담고 있는 래퍼 클래스
    private Dictionary<string, MapMarkerDataList> _allMarkers = new Dictionary<string, MapMarkerDataList>();

    private const string SAVE_KEY = "MapMarkerData";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMarkers();
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 시 이벤트 구독
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AddMarker(Vector3 position, string text, Color color)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        MapMarkerData newMarker = new MapMarkerData { position = position, text = text, color = color };

        if (!_allMarkers.ContainsKey(currentScene))
        {
            _allMarkers[currentScene] = new MapMarkerDataList();
        }
        Debug.Log("Add Marker pos : " + position);
        _allMarkers[currentScene].markers.Add(newMarker);
        SaveMarkers();
        OnMarkerAdded?.Invoke(); // 마커가 추가되었음을 알리는 이벤트 발생
    }

    public List<MapMarkerData> GetMarkersForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (_allMarkers.ContainsKey(currentScene))
        {
            return _allMarkers[currentScene].markers;
        }
        return new List<MapMarkerData>();
    }

    // 특정 씬의 모든 마커 데이터를 반환하는 메서드
    public List<MapMarkerData> GetAllMarkersForScene(string sceneName)
    {
        if (_allMarkers.ContainsKey(sceneName))
        {
            return _allMarkers[sceneName].markers;
        }
        return new List<MapMarkerData>();
    }

    private void SaveMarkers()
    {
        var serialization = new Serialization<string, MapMarkerDataList>(_allMarkers);
        string json = JsonUtility.ToJson(serialization);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("깃발 데이터가 저장되었습니다.");
    }

    private void LoadMarkers()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            try
            {
                var serialization = new Serialization<string, MapMarkerDataList>(json);
                _allMarkers = serialization.ToDictionary();
                Debug.Log("저장된 깃발 데이터를 불러왔습니다.");

                // 현재 씬의 마커들을 인스턴스화합니다.
                InstantiateMarkersForCurrentScene();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"깃발 데이터 로딩 실패: 데이터 형식이 잘못되었을 수 있습니다. 에러: {e.Message}");
                // 로딩에 실패하면 기존 데이터를 삭제하여 다음 실행 시 문제를 방지
                PlayerPrefs.DeleteKey(SAVE_KEY);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드될 때마다 해당 씬의 마커들을 인스턴스화합니다.
        InstantiateMarkersForCurrentScene();
    }

    private void InstantiateMarkersForCurrentScene()
    {
        if (flagPrefab == null)
        {
            Debug.LogWarning("Flag Prefab이 할당되지 않았습니다.");
            return;
        }

        List<MapMarkerData> currentSceneMarkers = GetMarkersForCurrentScene();
        foreach (var markerData in currentSceneMarkers)
        {
            GameObject flag = Instantiate(flagPrefab, markerData.position, Quaternion.identity);

            MeshRenderer[] flagRenderer = flag.GetComponentsInChildren<MeshRenderer>();
            if (flagRenderer != null)
            {
                foreach (MeshRenderer renderer in flagRenderer)
                    renderer.material.color = markerData.color;
            }
        }
    }
}

