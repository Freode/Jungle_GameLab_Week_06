using UnityEngine;

public class ChaseState : State<EnemyAI>
{
    public ChaseState(EnemyAI owner, StateMachine<EnemyAI> stateMachine) : base(owner, stateMachine)
    {
    }

    public override void Enter()
    {
        // 추격 상태에 진입하면 속도를 올립니다.
        owner.agent.speed = owner.chaseSpeed;
        Debug.Log("Entering Chase State");
    }

    public override void Execute()
    {
        if (owner.IsDie())
        {
            stateMachine.ChangeState(new DieState(owner, stateMachine));
            return;
        }

        // 플레이어를 놓치면 순찰 상태로 전환합니다.
        if ((owner.IsPlayerInSight() == false) && (owner._isHitting == false))
        {
            stateMachine.ChangeState(new PatrolState(owner, stateMachine));
            return;
        }

        // 플레이어와 거리가 가까워지면, 공격을 시도합니다.
        if(Vector3.Distance(owner.transform.position, owner.player.position) <= 1.75f)
        {
            stateMachine.ChangeState(new AttackState(owner, stateMachine));
            return;
        }

        // 플레이어를 계속 추격합니다.
        if (owner.player != null)
        {
            owner.agent.SetDestination(owner.player.position);
        }
    }
}

