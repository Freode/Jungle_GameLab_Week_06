using UnityEngine;

public class PlayerStatus : MonoBehaviour, ITakeDamage
{

    public void TakeDamage(GameObject opponent, float damage)
    {
        GameManager.instance.AnnounceHpChanged(damage);
    }
}
