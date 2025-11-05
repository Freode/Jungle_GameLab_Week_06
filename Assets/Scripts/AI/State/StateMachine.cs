using UnityEngine;

// 제네릭 상태 머신 클래스
public class StateMachine<T> where T : MonoBehaviour
{
    public State<T> currentState { get; private set; }
    private T _owner;

    public StateMachine(T owner)
    {
        this._owner = owner;
        currentState = null;
    }

    // 상태 머신 초기화
    public void Initialize(State<T> startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    // 상태 머신 변경
    public void ChangeState(State<T> newState)
    {
        currentState?.Exit();
        currentState = newState;
        newState.Enter();
    }

    // 상태를 실행
    public void Execute()
    {
        currentState?.Execute();
    }
}

