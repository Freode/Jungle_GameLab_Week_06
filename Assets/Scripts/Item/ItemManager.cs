using UnityEngine;
using System.Collections;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyItemEffect(ItemData item)
    {
        if (PlayerDataManager.instance == null || item == null)
        {
            Debug.LogWarning("PlayerDataManager not found or item is null. Cannot apply item effect.");
            return;
        }

        Debug.Log($"Applying effect of item: {item.itemName}");

        switch (item.buffType)
        {
            case BuffType.MaxHealthIncrease:
                PlayerDataManager.instance.IncreaseCurrentMaxHealth(item.value);
                break;

            case BuffType.MaxHealthBuff:
                PlayerDataManager.instance.AddStageBuffMaxHealth(item.value);
                if (item.duration > 0)
                {
                    StartCoroutine(RemoveBuffAfterDuration(item.buffType, item.value, item.duration));
                }
                break;

            case BuffType.HealthRecover:
                PlayerDataManager.instance.IncreaseCurrentHealth(item.value);
                break;

            case BuffType.AttackPowerIncrease:
                PlayerDataManager.instance.IncreaseCurrentAttackPower(item.value);
                break;

            case BuffType.AttackPowerBuff:
                PlayerDataManager.instance.AddStageBuffPower(item.value);
                if (item.duration > 0)
                {
                    StartCoroutine(RemoveBuffAfterDuration(item.buffType, item.value, item.duration));
                }
                break;

            case BuffType.EnergyRecover:
                PlayerDataManager.instance.IncreaseCurrentEnergy((int)item.value);
                break;
            
            // case BuffType.MovementSpeedBuff:
            //     // Player movement logic not available, placeholder
            //     Debug.Log("MovementSpeedBuff not implemented yet.");
            //     break;

            default:
                Debug.LogWarning($"Unhandled BuffType: {item.buffType}");
                break;
        }
    }

    private IEnumerator RemoveBuffAfterDuration(BuffType type, float value, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (PlayerDataManager.instance == null) yield break;

        Debug.Log($"Removing buff of type {type} after {duration} seconds.");

        switch (type)
        {
            case BuffType.MaxHealthBuff:
                PlayerDataManager.instance.AddStageBuffMaxHealth(-value);
                break;
            case BuffType.AttackPowerBuff:
                PlayerDataManager.instance.AddStageBuffPower(-value);
                break;
            // Add cases for other temporary buffs
        }
    }
}
