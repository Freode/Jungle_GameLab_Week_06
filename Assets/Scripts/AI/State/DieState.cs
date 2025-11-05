using UnityEngine;

public class DieState : State<EnemyAI>
{
    public DieState(EnemyAI owner, StateMachine<EnemyAI> stateMachine) : base(owner, stateMachine)
    {

    }

    public override void Enter()
    {
        owner.agent.SetDestination(owner.transform.position);

        MeshRenderer renderer = owner.gameObject.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
            renderer.material.color = Color.black;

        if (owner.player != null)
            GameManager.instance.AnnounceExpAcquire(15);

        // 루팅 활성화
        owner.gameObject.TryGetComponent(out BaseInteract baseInteract);
        if (baseInteract != null)
        {
            FuncSystem.UpdateGameObjectLayerAll(owner.gameObject, LayerMask.NameToLayer("Interact"));
            baseInteract.enabled = true;
        }

        owner.SetUpdate(false);
    }
}
