using UnityEngine;

public class MouseUtility : MonoBehaviour
{
    /// 마우스 위치에 대한 정보를 반환
    public static bool MousePositionOnPlane(Camera cam, LayerMask interactMask, out RaycastHit returnInfo)
    {
        if (cam == null) 
            cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("MousePositionOnYPlane: Camera.main is null.");
            returnInfo = default;
            return false;
        }

        // 메인 카메라에서 마우스 커서 위치로 Ray(광선)를 생성합니다.
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);

        // Raycast를 실행하여 광선이 특정 물체와 충돌했는지 확인합니다.
        // Mathf.Infinity: 광선의 길이를 무한대로 설정
        // groundLayer: 'Ground' 레이어에만 충돌하도록 제한하여 성능을 향상시킵니다.
        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, Mathf.Infinity, interactMask))
        {
            returnInfo = hitInfo;
            return true;
        }

        returnInfo = default;
        return false;
    }
}
