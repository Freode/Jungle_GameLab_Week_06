using UnityEngine;
using System.Collections.Generic;
using System;

public class BaseInteract : MonoBehaviour, IInteract
{
    // 외곽선 활성화할 객체
    public List<GameObject> outlineObjects;
    public event Func<bool> OnCheck;

    public GameObject interactUI;
    protected bool _canInteract = false;
    public bool _isActive = true;

    void Start()
    {
        
    }

    void IInteract.OnClick()
    {
        
    }

    void IInteract.OnHoverEnter()
    {
        if (OnCheck != null && OnCheck.Invoke() == false)
            return;

        if (_isActive == false)
            return;

        if (_canInteract)
        {
            interactUI.gameObject.SetActive(true);

            foreach (GameObject outlineObject in outlineObjects)
                outlineObject.layer = LayerMask.NameToLayer("Outline");
        }
    }

    void IInteract.OnHoverExit()
    {
        if (interactUI.gameObject.activeSelf)
        {
            interactUI.gameObject.SetActive(false);

            foreach (GameObject outlineObject in outlineObjects)
                outlineObject.layer = LayerMask.NameToLayer("Interact");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (OnCheck != null && OnCheck.Invoke() == false)
            return;

        if (other.gameObject.CompareTag("PlayerInteractBoundary"))
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

    public void SetIsActive(bool newActive)
    {
        _isActive = newActive;
    }
}
