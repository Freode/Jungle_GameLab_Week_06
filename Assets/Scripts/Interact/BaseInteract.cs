using UnityEngine;

public class BaseInteract : MonoBehaviour, IInteract
{

    void Start()
    {
    }

    void IInteract.OnClick()
    {
        
    }

    void IInteract.OnHoverEnter()
    {
        gameObject.layer = LayerMask.NameToLayer("Outline");
    }

    void IInteract.OnHoverExit()
    {
        gameObject.layer = LayerMask.NameToLayer("Interact");
    }
}
