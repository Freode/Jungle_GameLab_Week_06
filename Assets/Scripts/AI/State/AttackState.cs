using UnityEngine;

public class AttackState : State<EnemyAI>
{
    WeaponStick _weaponStick;

    public AttackState(EnemyAI owner, StateMachine<EnemyAI> stateMachine) : base(owner, stateMachine)
    {

    }

    public override void Enter()
    {
        _weaponStick = owner.GetComponentInChildren<WeaponStick>();
        _weaponStick.Attack();
    }

    public override void Execute()
    {
        if (_weaponStick.IsAttacking())
            return;

        if (owner.IsPlayerInSight())
            stateMachine.ChangeState(new ChaseState(owner, stateMachine));
        else
            stateMachine.ChangeState(new PatrolState(owner, stateMachine));
    }

    public override void Exit()
    {

    }
}
