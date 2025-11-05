using UnityEngine;
using UnityEngine.AI;

// 제네릭 State 클래스를 상속받습니다.
public class PatrolState : State<EnemyAI>
{
    private float waitTimer;
    private bool destinationSet;

    // 생성자도 부모 클래스에 맞춰줍니다.
    public PatrolState(EnemyAI owner, StateMachine<EnemyAI> stateMachine) : base(owner, stateMachine)
    {
    }

    public override void Enter()
    {
        owner.agent.speed = owner.patrolSpeed;
        waitTimer = owner.patrolWaitTime;
        destinationSet = false;
        Debug.Log("Entering Patrol State");
    }

    public override void Execute()
    {
        if(owner.IsDie())
        {
            stateMachine.ChangeState(new DieState(owner, stateMachine));
            return;
        }

        if (owner.IsPlayerInSight())
        {
            // 상태를 전환할 때도 제네릭 타입에 맞춰줍니다.
            stateMachine.ChangeState(new ChaseState(owner, stateMachine));
            return;
        }

        if (destinationSet)
        {
            if (owner.agent.remainingDistance <= owner.agent.stoppingDistance)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    destinationSet = false;
                    waitTimer = owner.patrolWaitTime;
                }
            }
        }
        else
        {
            SearchNewPatrolPoint();
        }
    }

    private void SearchNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * owner.patrolRadius;
        randomDirection += owner.startPoint;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, owner.patrolRadius, 1))
        {
            owner.agent.SetDestination(hit.position);
            destinationSet = true;
        }
    }
}


