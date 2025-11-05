using UnityEngine;

public class ColliderEventRelay : MonoBehaviour
{
    public System.Action<Collider> onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke(other);
    }
}
