using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject prefabToPlace;
    public KeyCode placeObjectKey = KeyCode.P;
    public bool isMarkAddition = false;
    public string markText = string.Empty;
    public Color markColor = Color.white;

    void Update()
    {
        if (Input.GetKeyDown(placeObjectKey))
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (prefabToPlace == null)
        {
            Debug.LogError("Prefab to place is not assigned in ObjectPlacer.");
            return;
        }

        if (PlayerDataManager.instance.GetRecoverPlaceCount() == 0)
            return;

        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        // Instantiate the object locally immediately
        Instantiate(prefabToPlace, position, rotation);
        PlayerDataManager.instance.IncreaseRecoverPlaceCount(-1);

        if (isMarkAddition)
            MarkerManager.instance.AddMarker(position, markText, markColor);

        // Save the object's data for persistence
        ObjectManager.instance.AddObject(position, rotation, prefabToPlace.name);

        Debug.Log($"Placed {prefabToPlace.name} at {position}");
    }
}
