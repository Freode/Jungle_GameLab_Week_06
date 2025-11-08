using UnityEngine;
using System.Collections.Generic;

public class BaseInteract : MonoBehaviour, IInteract
{
    // 외곽선 활성화할 객체
    public List<GameObject> outlineObjects;

    protected bool _canInteract = false;

    void Start()
    {
        
    }

    void IInteract.OnClick()
    {
        
    }

    void IInteract.OnHoverEnter()
    {
        if (_canInteract)
        {
            foreach (GameObject outlineObject in outlineObjects)
                outlineObject.layer = LayerMask.NameToLayer("Outline");
        }
    }

    void IInteract.OnHoverExit()
    {
        foreach (GameObject outlineObject in outlineObjects)
            outlineObject.layer = LayerMask.NameToLayer("Interact");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("PlayerInteractBoundary"))
        {
            _canInteract = true;
            ((IInteract)this).OnHoverEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerInteractBoundary"))
        {
            _canInteract = false;
            ((IInteract)this).OnHoverExit();
        }
    }
}
