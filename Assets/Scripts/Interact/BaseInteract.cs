using UnityEngine;
using System.Collections.Generic;

public class BaseInteract : MonoBehaviour, IInteract
{
    // 외곽선 활성화할 객체
    public List<GameObject> outlineObjects;


    void Start()
    {
    }

    void IInteract.OnClick()
    {
        
    }

    void IInteract.OnHoverEnter()
    {
        foreach(GameObject outlineObject in outlineObjects)
            outlineObject.layer = LayerMask.NameToLayer("Outline");
    }

    void IInteract.OnHoverExit()
    {
        foreach (GameObject outlineObject in outlineObjects)
            outlineObject.layer = LayerMask.NameToLayer("Interact");
    }
}
