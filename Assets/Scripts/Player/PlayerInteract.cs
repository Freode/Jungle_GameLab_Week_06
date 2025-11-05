using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public LayerMask interactMask;          // 상호작용할 마스크

    private Camera _mainCamera;
    private IInteract _lastInteractable;    // 이전에 마우스가 올라가 있던 객체


    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (MouseHoverEnter() == false)
            MouseHoverExit();
    }

    // 마우스와 상호작용
    bool MouseHoverEnter()
    {
        bool result = MouseUtility.MousePositionOnPlane(_mainCamera, interactMask, out RaycastHit hitInfo);
        if (result == false) return false;

        IInteract curInteractable = hitInfo.collider.GetComponent<IInteract>();

        // 둘 다 없으면, 무시
        if (_lastInteractable == null && curInteractable == null)
            return true;

        // 이전 객체와 동일하면, 무시
        if (_lastInteractable == curInteractable)
            return true;

        // 이전 객체 마우스 효과 비활성화
        if (_lastInteractable != null)
            _lastInteractable.OnHoverExit();

        // 마우스 효과 활성화
        curInteractable.OnHoverEnter();
        _lastInteractable = curInteractable;
        return true;
    }

    // 마우스와 상호작용 종료
    void MouseHoverExit()
    {
        if (_lastInteractable == null)
            return;

        _lastInteractable.OnHoverExit();
        _lastInteractable = null;
    }
}
