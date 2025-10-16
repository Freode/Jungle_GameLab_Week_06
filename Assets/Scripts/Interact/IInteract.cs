// 마우스와 상호작용이 가능한 모든 객체가 구현해야 하는 인터페이스
public interface IInteract
{
    // 마우스 커서가 객체 위로 올라왔을 때 호출될 함수입니다.
    void OnHoverEnter();

    // 마우스 커서가 객체 위에서 벗어났을 때 호출될 함수입니다.
    void OnHoverExit();

    // 객체를 마우스로 클릭했을 때 호출될 함수입니다.
    void OnClick();
}
