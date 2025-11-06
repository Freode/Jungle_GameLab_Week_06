using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    public Transform player;                                // 카메라를 따라갈 대상
    public Vector3 offset = new Vector3(0f, 20f, 0f);    // 카메라 오프셋

    // 외부에서 플레이어 Transform을 설정할 수 있는 메서드
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }

    // LateUpdate는 모든 Update() 호출이 끝난 후에 실행됩니다.
    // 플레이어가 먼저 움직이고 카메라가 그 뒤에 위치를 업데이트해야
    // 화면이 떨리는 현상이 발생하지 않습니다.
    void LateUpdate()
    {
        // 카메라의 위치를 (현재 플레이어 위치 + 처음 계산한 거리)로 계속 업데이트합니다.
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}
