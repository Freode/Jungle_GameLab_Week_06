using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    public Transform player;        // 카메라를 따라갈 대상

    private Vector3 _offset;        // 카메라와 플레이어 사이의 초기 거리를 저장할 변수

    void Start()
    {
        // 게임 시작 시, 카메라와 플레이어 사이의 거리를 계산하고 저장합니다.
        // 이 오프셋(offset) 값 덕분에 에디터에서 설정한 카메라 위치가 그대로 유지됩니다.
        _offset = transform.position - player.position;
    }

    // LateUpdate는 모든 Update() 호출이 끝난 후에 실행됩니다.
    // 플레이어가 먼저 움직이고 카메라가 그 뒤에 위치를 업데이트해야
    // 화면이 떨리는 현상이 발생하지 않습니다.
    void LateUpdate()
    {
        // 카메라의 위치를 (현재 플레이어 위치 + 처음 계산한 거리)로 계속 업데이트합니다.
        transform.position = player.position + _offset;
    }
}
