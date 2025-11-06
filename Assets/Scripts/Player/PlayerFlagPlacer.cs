using UnityEngine;

public class PlayerFlagPlacer : MonoBehaviour
{
    public GameObject flagPrefab; // 3D 깃발 모델 프리팹 (Inspector에서 할당)
    public KeyCode placeFlagKey = KeyCode.F;

    void Update()
    {
        if (Input.GetKeyDown(placeFlagKey))
        {
            PlaceFlag();
        }
    }

    private void PlaceFlag()
    {
        // TODO: 깃발 텍스트를 입력받는 UI InputField를 띄우는 로직 필요
        // 여기서는 임시로 "My Flag"라는 텍스트를 사용합니다.
        string flagText = "My Flag";

        // 플레이어의 현재 위치에 깃발 데이터 생성
        Vector3 flagPosition = transform.position;
        
        // [디버그] 저장되는 위치 좌표를 정확히 기록
        Debug.Log($"[SAVE] 깃발 위치 저장 시도: {flagPosition.ToString("F4")}");

        MarkerManager.instance.AddMarker(flagPosition, flagText);

        // 3D 깃발 모델을 월드에 실제로 생성
        if (flagPrefab != null)
        {
            Instantiate(flagPrefab, flagPosition, Quaternion.identity);
        }

        Debug.Log($"위치 {flagPosition}에 '{flagText}' 깃발을 설치했습니다.");
    }
}
