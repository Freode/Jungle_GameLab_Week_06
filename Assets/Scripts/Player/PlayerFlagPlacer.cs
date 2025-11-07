using UnityEngine;

public class PlayerFlagPlacer : MonoBehaviour
{
    public GameObject flagPrefab; // 3D 깃발 모델 프리팹 (Inspector에서 할당)
    public KeyCode placeFlagKey = KeyCode.F;
    private FlagCustomizationUI flagCustomizationUI;

    void Start()
    {
        flagCustomizationUI = FindFirstObjectByType<FlagCustomizationUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(placeFlagKey))
        {
            if (flagCustomizationUI == null)
            {
                flagCustomizationUI = FindFirstObjectByType<FlagCustomizationUI>();
            }

            if (flagCustomizationUI != null)
            {
                flagCustomizationUI.Open();
            }
        }
    }

    public void PlaceFlagFromUI(string flagText, Color flagColor)
    {
        // 플레이어의 현재 위치에 깃발 데이터 생성
        Vector3 flagPosition = transform.position;

        MarkerManager.instance.AddMarker(flagPosition, flagText, flagColor);

        // 3D 깃발 모델을 월드에 실제로 생성
        if (flagPrefab != null)
        {
            GameObject newFlag = Instantiate(flagPrefab, flagPosition, Quaternion.identity);
            MeshRenderer[] flagRenderer = newFlag.GetComponentsInChildren<MeshRenderer>();
            if (flagRenderer != null)
            {
                foreach(MeshRenderer renderer in flagRenderer)
                    renderer.material.color = flagColor;
            }
        }

        Debug.Log($"위치 {flagPosition}에 '{flagText}' ({flagColor}) 깃발을 설치했습니다.");
    }
}
