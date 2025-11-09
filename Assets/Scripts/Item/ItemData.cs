using UnityEngine;

// 아이템 등급
public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

// 아이템 효과 종류
public enum ItemEffectType
{
    MoveSpeed,
    ViewAngle,
    AttackDamage,
    MaxHealth,
    Energy,
    Water,
    Health,
    StatSave,
    KeyMarker,
    Shelter,
    FullView,
    Nothing
}

// 아이템 지속 타입
public enum DurationType
{
    Buff,       // 현재 판(게임 오버 전까지)
    Stage,      // 현재 스테이지
    Permanent   // 영구 적용 (자원 회복, 스탯 저장 등)
}

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    [TextArea]
    public string description;

    [Header("Classification")]
    public ItemRarity rarity;
    public ItemEffectType effectType;
    public DurationType durationType;

    [Header("Values")]
    public float value; // 수치적 효과 (예: +10, +5%)
    public string stringValue; // 문자열 효과 (예: "Stage2_Key")
    public float duration; // 버프 지속시간
}