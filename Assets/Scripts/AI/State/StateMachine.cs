using UnityEngine;

// 제네릭 상태 머신 클래스
public class StateMachine<T> where T : MonoBehaviour
{
    public State<T> CurrentState { get; private set; }
    private T owner;

    public StateMachine(T owner)
    {
        this.owner = owner;
        CurrentState = null;
    }

    // 상태 머신 초기화
    public void Initialize(State<T> startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }

    // 상태 머신 변경
    public void ChangeState(State<T> newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        newState.Enter();
    }

    // 상태를 실행
    public void Execute()
    {
        CurrentState?.Execute();
    }
}

