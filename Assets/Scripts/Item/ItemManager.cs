using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    public event System.Action<ItemData> OnItemApplied;

    private class ActiveBuff
    {
        public BuffType Type;
        public float Value;
        public Coroutine RemovalCoroutine;
    }
    private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();

    private PlayerMove _playerMove;

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

    void Start()
    {
        FindPlayerComponents();
    }

    private void FindPlayerComponents()
    {
        if (_playerMove == null) // Only try to find if not already found
        {
            GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player"); // Assuming "Player" tag
            if (playerGameObject != null)
            {
                _playerMove = playerGameObject.GetComponent<PlayerMove>();
                if (_playerMove == null)
                {
                    Debug.LogWarning("[ItemManager] PlayerMove component not found on GameObject with tag 'Player'.");
                }
                else
                {
                    Debug.Log("[ItemManager] PlayerMove component found on GameObject with tag 'Player'.");
                }
            }
            else
            {
                Debug.LogWarning("[ItemManager] GameObject with tag 'Player' not found in scene. Ensure it has the 'Player' tag.");
            }
        }
    }

    public void ApplyItemEffect(ItemData item)
    {
        if (PlayerDataManager.instance == null || item == null) return;

        Debug.Log($"Applying effect of item: {item.itemName}");

        if (item.buffType == BuffType.MovementSpeedBuff || item.buffType == BuffType.VisionAngleBuff)
        {
            HandleStackingBuff(item);
        }
        else
        {
            switch (item.buffType)
            {
                case BuffType.None:
                    break;
                case BuffType.MaxHealthIncrease:
                    PlayerDataManager.instance.IncreaseCurrentMaxHealth(item.value);
                    break;
                case BuffType.MaxHealthBuff:
                    PlayerDataManager.instance.AddStageBuffMaxHealth(item.value);
                    break;
                case BuffType.HealthRecover:
                    PlayerDataManager.instance.IncreaseCurrentHealth(item.value);
                    break;
                case BuffType.AttackPowerIncrease:
                    PlayerDataManager.instance.IncreaseCurrentAttackPower(item.value);
                    break;
                case BuffType.AttackPowerBuff:
                    PlayerDataManager.instance.AddStageBuffPower(item.value);
                    break;
                case BuffType.EnergyRecover:
                    PlayerDataManager.instance.IncreaseCurrentEnergy((int)item.value);
                    break;
                case BuffType.WaterRecover:
                    PlayerDataManager.instance.IncreaseCurrentWater((int)item.value);
                    break;
                case BuffType.PartialExplorationSave:
                    HandlePartialSave(item.value);
                    break;
                default:
                    Debug.LogWarning($"Unhandled BuffType: {item.buffType}");
                    break;
            }
        }
        OnItemApplied?.Invoke(item);
    }

    private void HandleStackingBuff(ItemData item)
    {
        ActiveBuff existingBuff = _activeBuffs.FirstOrDefault(b => b.Type == item.buffType);

        if (existingBuff != null)
        {
            StopCoroutine(existingBuff.RemovalCoroutine);

            if (item.value > existingBuff.Value)
            {
                ApplyBuffValue(item.buffType, item.value, existingBuff.Value);
                existingBuff.Value = item.value;
            }
            
            existingBuff.RemovalCoroutine = StartCoroutine(RemoveBuffAfterDuration(existingBuff, item.duration));
            Debug.Log($"Refreshed {item.buffType} buff. New value: {existingBuff.Value}.");
        }
        else
        {
            ApplyBuffValue(item.buffType, item.value, 0);
            
            var newBuff = new ActiveBuff { Type = item.buffType, Value = item.value };
            newBuff.RemovalCoroutine = StartCoroutine(RemoveBuffAfterDuration(newBuff, item.duration));
            _activeBuffs.Add(newBuff);
            Debug.Log($"Applied new {item.buffType} buff with value {item.value}.");
        }
    }

    private void ApplyBuffValue(BuffType type, float newValue, float oldValue)
    {
        float delta = newValue - oldValue;
        switch (type)
        {
            case BuffType.MovementSpeedBuff:
                if (_playerMove == null) FindPlayerComponents();
                if (_playerMove != null)
                {
                    _playerMove.additiveSpeedBonus += delta; // Add the delta to the additive bonus
                    Debug.Log($"[ItemManager] Applied MovementSpeedBuff. New additiveSpeedBonus: {_playerMove.additiveSpeedBonus}");
                }
                else
                {
                    Debug.LogWarning("[ItemManager] Cannot apply MovementSpeedBuff, PlayerMove is null.");
                }
                break;
            case BuffType.VisionAngleBuff:
                if (FogOfWarManager.instance != null)
                {
                    FogOfWarManager.instance.visionAngleBonus += delta;
                    Debug.Log($"[ItemManager] Applied VisionAngleBuff. New visionAngleBonus: {FogOfWarManager.instance.visionAngleBonus}");
                }
                else
                {
                    Debug.LogWarning("[ItemManager] Cannot apply VisionAngleBuff, FogOfWarManager is null.");
                }
                break;
        }
    }

    private IEnumerator RemoveBuffAfterDuration(ActiveBuff buff, float duration)
    {
        yield return new WaitForSeconds(duration);

        switch(buff.Type)
        {
            case BuffType.MovementSpeedBuff:
                if (_playerMove != null)
                {
                    _playerMove.additiveSpeedBonus -= buff.Value; // Subtract the buff's value
                    Debug.Log($"[ItemManager] Removed MovementSpeedBuff. New additiveSpeedBonus: {_playerMove.additiveSpeedBonus}");
                }
                break;
            case BuffType.VisionAngleBuff:
                // Revert bonus to 0 by passing 0 as the "new" value
                ApplyBuffValue(buff.Type, 0f, buff.Value); 
                break;
        }
        
        _activeBuffs.Remove(buff);
        Debug.Log($"Removed {buff.Type} buff.");
    }

    private void HandlePartialSave(float percentage)
    {
        if (PlayerDataManager.instance == null) return;

        float percentageValue = Mathf.Clamp01(percentage);

        float healthToSave = PlayerDataManager.instance.stagePersistMaxHealth * percentageValue;
        float powerToSave = PlayerDataManager.instance.stagePersistPower * percentageValue;

        if (healthToSave > 0)
        {
            PlayerDataManager.instance.IncreaseCurrentMaxHealth(healthToSave);
            PlayerDataManager.instance.AddStagePersistMaxHealth(-healthToSave);
        }
        if (powerToSave > 0)
        {
            PlayerDataManager.instance.IncreaseCurrentAttackPower(powerToSave);
            PlayerDataManager.instance.AddStagePersistPower(-powerToSave);
        }

        if (FogOfWarManager.instance != null)
        {
            FogOfWarManager.instance.CommitPartialFogData(percentageValue);
        }

        Debug.Log($"Partially saved {percentageValue * 100f}% of exploration progress.");
    }
}
