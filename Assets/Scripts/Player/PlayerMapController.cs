using UnityEngine;

public class PlayerMapController : MonoBehaviour
{
    public WorldMapUI worldMapUI;
    public KeyCode mapToggleKey = KeyCode.M;

    void Start()
    {
        // 맵 UI에 플레이어 자신의 Transform 정보를 넘겨줌
        if (worldMapUI != null)
        {
            worldMapUI.playerTransform = this.transform;
        }
    }

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
                // OpenMap을 호출하기만 하면 됨 (위치 전달 필요 없음)
                worldMapUI.OpenMap();
            }
        }
    }
}