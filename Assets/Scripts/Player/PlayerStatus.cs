using UnityEngine;

public class PlayerStatus : MonoBehaviour, ITakeDamage
{
    // PlayerStatus는 이제 체력 데이터를 직접 소유하지 않고 PlayerDataManager로부터 가져옵니다.
    // 필요하다면 UI 업데이트를 위한 로직을 여기에 추가할 수 있습니다.

    void Start()
    {
        // PlayerDataManager로부터 현재 체력을 가져와서 UI 등에 반영할 수 있습니다.
        if (PlayerDataManager.instance != null)
        {
            float currentHealth = PlayerDataManager.instance.GetCurrentHealth();
            Debug.Log($"PlayerStatus 초기화: 현재 체력 {currentHealth}");
            // 예: UI 업데이트 로직
        }
    }

    public void TakeDamage(GameObject opponent, float damage)
    {
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.TakeDamage(damage);
            // GameManager.instance.AnnounceHpChanged(damage); // GameManager가 PlayerDataManager의 변경을 구독하도록 변경 가능
            Debug.Log($"플레이어에게 {damage} 데미지 입힘. 남은 체력: {PlayerDataManager.instance.GetCurrentHealth()}");
        }
        else
        {
            Debug.LogWarning("PlayerDataManager 인스턴스를 찾을 수 없어 데미지를 처리할 수 없습니다.");
        }
    }
}
