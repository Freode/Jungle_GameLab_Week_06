using UnityEngine;

// 모든 객체에 대해 레이어 변경
public static class FuncSystem
{
    public static void UpdateGameObjectLayerAll(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            UpdateGameObjectLayerAll(child.gameObject, newLayer);
        }
    }
}
