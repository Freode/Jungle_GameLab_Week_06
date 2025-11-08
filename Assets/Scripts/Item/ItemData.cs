using UnityEngine;

// 아이템의 등급을 나타내는 Enum (열거형)
public enum ItemGrade { Common, Rare, Epic, Legendary }

// 아이템이 제공하는 효과의 종류를 나타내는 Enum
public enum BuffType
{
    // 체력 관련
    MaxHealthIncrease,      // 최대 체력 영구 증가
    MaxHealthBuff,          // 최대 체력 일시적 증가 (버프)
    HealthRecover,          // 현재 체력 즉시 회복

    // 공격력 관련
    AttackPowerIncrease,    // 공격력 영구 증가
    AttackPowerBuff,        // 공격력 일시적 증가 (버프)

    // 기타 (예시)
    MovementSpeedBuff,      // 이동 속도 증가
    EnergyRecover           // 에너지 회복
}

// 각 아이템의 속성을 정의하는 클래스
[System.Serializable] // Unity 인스펙터에서 보기 위함
public class ItemData
{
    public string itemName;         // 아이템 이름
    public string description;      // 아이템 설명
    public ItemGrade grade;         // 아이템 등급 (색상이나 이펙트 등에 활용)

    [Header("효과")]
    public BuffType buffType;       // 어떤 종류의 효과인지
    public float value;             // 효과의 수치 (예: 체력 20 증가)
    public float duration;          // 지속 시간 (0이면 영구적인 효과)

    [Header("확률")]
    [Range(0, 100)]
    public float probabilityWeight; // 아이템 등장 확률 가중치
}
