using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("캐릭터의 이동 속도를 설정합니다.")]
    public float baseMoveSpeed = 8f; // 기본 이동 속도
    public float additiveSpeedBonus = 0f; // 버프 등을 위한 추가 이동 속도 (덧셈)
    private float _currentMoveSpeed; // 실제 적용될 이동 속도

    [Header("Mouse Look Settings")]
    public LayerMask groundLayer;       // 마우스가 갑지할 레이더

    // 내부에서 사용할 변수들
    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Camera _mainCamera;
    private Vector3 _mouseLoc;          // 마우스 위치

    void Start()
    {
        // 게임이 시작될 때 필요한 컴포넌트를 미리 찾아둡니다.
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main; // 씬에 있는 Main Camera를 찾습니다.
    }

    void Update()
    {
        // --- 1. 키보드 입력 받기 ---
        // 매 프레임마다 입력을 감지합니다.
        // GetAxisRaw: -1, 0, 1 세 가지 값만 반환하여 즉각적인 반응을 보입니다.
        _moveInput.x = Input.GetAxisRaw("Horizontal"); // A, D 또는 좌우 화살표
        _moveInput.z = Input.GetAxisRaw("Vertical");   // W, S 또는 위아래 화살표

        // 대각선으로 이동할 때 속도가 더 빨라지는 것을 방지하기 위해 정규화(Normalize)합니다.
        _moveInput.Normalize();

        // --- 2. 마우스 방향으로 회전하기 ---
        HitRayAtMouse();
        RotatePlayer();
    }

    void FixedUpdate()
    {
        // 실제 이동 속도 계산
        _currentMoveSpeed = baseMoveSpeed + additiveSpeedBonus;

        // --- 3. 물리 엔진을 통해 캐릭터 이동시키기 ---
        // 물리 관련 처리는 FixedUpdate에서 하는 것이 안정적입니다.
        // Rigidbody의 속도(velocity)를 직접 제어하여 캐릭터를 부드럽게 움직입니다.
        _rb.linearVelocity = _moveInput * _currentMoveSpeed;
    }

    // 마우스 위치로 Ray 발사
    void HitRayAtMouse()
    {
        bool result = MouseUtility.MousePositionOnPlane(_mainCamera, groundLayer, out RaycastHit hitInfo);

        if (result == false) return;

        _mouseLoc = hitInfo.point;
    }

    // 플레이어 회전을 마우스 위치를 바라보도록 변경
    void RotatePlayer()
    {
        // 캐릭터가 충돌 지점을 바라보게 합니다.
        // Y축 값은 캐릭터의 현재 Y값을 유지해야 캐릭터가 위아래로 기울어지지 않습니다.
        transform.LookAt(new Vector3(_mouseLoc.x, transform.position.y, _mouseLoc.z));
    }
}
