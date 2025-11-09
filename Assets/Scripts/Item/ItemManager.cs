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
        public ItemEffectType Type;
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
        if (_playerMove == null)
        {
            GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (playerGameObject != null)
            {
                _playerMove = playerGameObject.GetComponent<PlayerMove>();
            }
        }
    }

    public void ApplyItemEffect(ItemData item)
    {
        if (PlayerDataManager.instance == null || item == null) return;

        Debug.Log($"Applying effect of item: {item.itemName}");

        // 버프(일시적 효과)와 영구/스테이지 효과를 분리하여 처리
        if (item.durationType == DurationType.Buff)
        {
            HandleStackingBuff(item);
        }
        else // Permanent 또는 Stage 타입의 효과 처리
        {
            switch (item.effectType)
            {
                case ItemEffectType.Nothing:
                    break;
                case ItemEffectType.MaxHealth: // 최대 체력 증가 (스테이지)
                    PlayerDataManager.instance.IncreaseCurrentMaxHealth(item.value);
                    break;
                case ItemEffectType.Health: // 현재 체력 회복
                    PlayerDataManager.instance.IncreaseCurrentHealth(item.value);
                    break;
                case ItemEffectType.AttackDamage: // 공격력 증가 (스테이지)
                    PlayerDataManager.instance.IncreaseCurrentAttackPower(item.value);
                    break;
                case ItemEffectType.Energy: // 에너지 회복
                    PlayerDataManager.instance.IncreaseCurrentEnergy((int)item.value);
                    break;
                case ItemEffectType.Water: // 물 회복
                    PlayerDataManager.instance.IncreaseCurrentWater((int)item.value);
                    break;
                case ItemEffectType.StatSave: // 부분 기록 저장
                    HandlePartialSave(item.value);
                    break;
                // KeyMarker, Shelter 등 다른 영구 효과는 여기서 처리 가능
                default:
                    Debug.LogWarning($"Unhandled Permanent/Stage EffectType: {item.effectType}");
                    break;
            }
        }
        OnItemApplied?.Invoke(item);
    }

    private void HandleStackingBuff(ItemData item)
    {
        ActiveBuff existingBuff = _activeBuffs.FirstOrDefault(b => b.Type == item.effectType);

        if (existingBuff != null)
        {
            if(existingBuff.RemovalCoroutine != null) StopCoroutine(existingBuff.RemovalCoroutine);

            if (item.value > existingBuff.Value)
            {
                ApplyBuffValue(item.effectType, item.value, existingBuff.Value);
                existingBuff.Value = item.value;
            }
            
            existingBuff.RemovalCoroutine = StartCoroutine(RemoveBuffAfterDuration(existingBuff, item.duration));
            Debug.Log($"Refreshed {item.effectType} buff. New value: {existingBuff.Value}.");
        }
        else
        {
            ApplyBuffValue(item.effectType, item.value, 0);
            
            var newBuff = new ActiveBuff { Type = item.effectType, Value = item.value };
            newBuff.RemovalCoroutine = StartCoroutine(RemoveBuffAfterDuration(newBuff, item.duration));
            _activeBuffs.Add(newBuff);
            Debug.Log($"Applied new {item.effectType} buff with value {item.value}.");
        }
    }

    private void ApplyBuffValue(ItemEffectType type, float newValue, float oldValue)
    {
        float delta = newValue - oldValue;
        switch (type)
        {
            case ItemEffectType.MoveSpeed:
                if (_playerMove == null) FindPlayerComponents();
                if (_playerMove != null) _playerMove.additiveSpeedBonus += delta;
                break;
            case ItemEffectType.ViewAngle:
                if (FogOfWarManager.instance != null) FogOfWarManager.instance.visionAngleBonus += delta;
                break;
            case ItemEffectType.MaxHealth: // 버프 타입의 최대 체력
                if (PlayerDataManager.instance != null) PlayerDataManager.instance.AddStageBuffMaxHealth(delta);
                break;
            case ItemEffectType.AttackDamage: // 버프 타입의 공격력
                if (PlayerDataManager.instance != null) PlayerDataManager.instance.AddStageBuffPower(delta);
                break;
            case ItemEffectType.FullView:
                 if (FogOfWarManager.instance != null) FogOfWarManager.instance.visionAngleBonus += delta;
                 break;
        }
    }

    private IEnumerator RemoveBuffAfterDuration(ActiveBuff buff, float duration)
    {
        // duration이 0보다 클 때만 대기
        if(duration > 0)
        {
            yield return new WaitForSeconds(duration);
        }
        else // 0 이하면 한 프레임만 대기 후 제거 (혹은 즉시 제거)
        {
            yield return null;
        }

        // 버프 효과 되돌리기
        ApplyBuffValue(buff.Type, 0f, buff.Value); 
        
        _activeBuffs.Remove(buff);
        Debug.Log($"Removed {buff.Type} buff.");
    }

    private void HandlePartialSave(float percentage)
    {
        if (PlayerDataManager.instance == null) return;

        float percentageValue = percentage / 100f; // 0.5% -> 0.005
        Mathf.Clamp01(percentageValue);

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

        Debug.Log($"Partially saved {percentage * 100f}% of exploration progress.");
    }
}