using UnityEngine;

// 모든 상태 클래스가 상속받아야 할 제네릭 추상 클래스
public abstract class State<T> where T : MonoBehaviour
{
    protected T owner;
    protected StateMachine<T> stateMachine;

    public State(T owner, StateMachine<T> stateMachine)
    {
        this.owner = owner;
        this.stateMachine = stateMachine;
    }

    // 이 상태에 진입했을 때 한 번 호출되는 함수
    public virtual void Enter() { }

    // 이 상태가 활성화되어 있는 동안 매 프레임 호출되는 함수
    public virtual void Execute() { }

    // 이 상태에서 빠져나갈 때 한 번 호출되는 함수
    public virtual void Exit() { }
}
