using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMapController : MonoBehaviour
{
    public WorldMapUI worldMapUI;
    public KeyCode mapToggleKey = KeyCode.M;

    void Update()
    {
        if (Input.GetKeyDown(mapToggleKey))
        {
            if (worldMapUI.mapPanel.activeSelf)
            {
                worldMapUI.CloseMap();
            }
            else
            {
                // 맵을 열 때 플레이어의 현재 로컬 좌표를 전달
                worldMapUI.OpenMap(transform.position);
            }
        }
    }
}