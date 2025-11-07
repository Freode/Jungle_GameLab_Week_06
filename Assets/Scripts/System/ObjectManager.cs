using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlacedObjectDataList
{
    public List<PlacedObjectData> objects = new List<PlacedObjectData>();
}

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager instance;
    public List<GameObject> placeablePrefabs; // List of all prefabs that can be placed

    private Dictionary<string, PlacedObjectDataList> _allPlacedObjects = new Dictionary<string, PlacedObjectDataList>();
    private const string SAVE_KEY = "PlacedObjectData";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadObjects();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AddObject(Vector3 position, Quaternion rotation, string prefabName)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlacedObjectData newObject = new PlacedObjectData { position = position, rotation = rotation, prefabName = prefabName };

        if (!_allPlacedObjects.ContainsKey(currentScene))
        {
            _allPlacedObjects[currentScene] = new PlacedObjectDataList();
        }
        _allPlacedObjects[currentScene].objects.Add(newObject);
        SaveObjects();
    }

    private void SaveObjects()
    {
        var serialization = new Serialization<string, PlacedObjectDataList>(_allPlacedObjects);
        string json = JsonUtility.ToJson(serialization);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Placed object data saved.");
    }

    private void LoadObjects()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            try
            {
                var serialization = new Serialization<string, PlacedObjectDataList>(json);
                _allPlacedObjects = serialization.ToDictionary();
                Debug.Log("Loaded placed object data.");
                InstantiateObjectsForCurrentScene();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load object data: {e.Message}");
                PlayerPrefs.DeleteKey(SAVE_KEY);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstantiateObjectsForCurrentScene();
    }

    private void InstantiateObjectsForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (_allPlacedObjects.ContainsKey(currentScene))
        {
            foreach (var objectData in _allPlacedObjects[currentScene].objects)
            {
                GameObject prefab = FindPrefabByName(objectData.prefabName);
                if (prefab != null)
                {
                    Instantiate(prefab, objectData.position, objectData.rotation);
                }
                else
                {
                    Debug.LogWarning($"Prefab with name {objectData.prefabName} not found in placeablePrefabs list.");
                }
            }
        }
    }

    private GameObject FindPrefabByName(string prefabName)
    {
        foreach (var prefab in placeablePrefabs)
        {
            if (prefab.name == prefabName)
            {
                return prefab;
            }
        }
        return null;
    }
}
