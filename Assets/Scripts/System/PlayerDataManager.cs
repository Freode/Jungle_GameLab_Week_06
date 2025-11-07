using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;

    // 플레이어의 지속적인 데이터
    public float maxHealth = 100f; // 플레이어의 최대 체력
    public float currentHealth;     // 플레이어의 현재 체력
    public int attackPower = 5; // 플레이어의 공격력

    public int currentLevel = 1;
    public int currentExp = 0;
    public int maxExp = 30;

    public int maxEnergy = 100;
    public int currentEnergy;

    public int maxWater = 100;
    public int currentWater;

    public float maxRange = 10f;
    public float currentRange;

    // 데이터 변경을 알리는 이벤트
    public event System.Action OnPlayerStatsChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // 게임 시작 시 현재 체력, 에너지, 물, 범위 초기화
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
            currentWater = maxWater;
            currentRange = maxRange;

            StartCoroutine(ReduceEnergyAndWaterCoroutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 플레이어에게 데미지를 입히는 메서드
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 체력이 0 미만, 최대 체력 초과 방지
        Debug.Log($"플레이어 데이터 매니저 체력: {currentHealth} / {maxHealth}");
        OnPlayerStatsChanged?.Invoke(); // 데이터 변경 알림

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            SceneManager.LoadScene("RestScene");
        }
    }

    // 에너지와 물을 주기적으로 감소시키는 코루틴
    IEnumerator ReduceEnergyAndWaterCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            currentEnergy = Mathf.Max(0, currentEnergy - 1);
            currentWater = Mathf.Max(0, currentWater - 1);

            if (currentEnergy == 0)
                currentHealth -= 1;

            if (currentWater == 0)
                currentHealth -= 1;

            OnPlayerStatsChanged?.Invoke(); // 데이터 변경 알림
        }
    }

    // 경험치 추가
    public void AddExp(int amount)
    {
        currentExp += amount;
        OnPlayerStatsChanged?.Invoke(); // 데이터 변경 알림

        if (currentExp >= maxExp)
            LevelUp();
    }

    // 레벨업 로직
    void LevelUp()
    {
        currentExp -= maxExp;
        maxExp += 5;
        currentLevel++;
        attackPower += 3;
        maxHealth += 5;
        currentHealth += 5; // 레벨업 시 체력 회복
        
        OnPlayerStatsChanged?.Invoke(); // 데이터 변경 알림
    }

    // --- 데이터 조회 메서드 --- //
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public int GetAttackPower() { return attackPower; }
    public int GetCurrentLevel() { return currentLevel; }
    public int GetCurrentExp() { return currentExp; }
    public int GetMaxExp() { return maxExp; }
    public int GetCurrentEnergy() { return currentEnergy; }
    public int GetMaxEnergy() { return maxEnergy; }
    public int GetCurrentWater() { return currentWater; }
    public int GetMaxWater() { return maxWater; }
    public float GetCurrentRange() { return currentRange; }
    public float GetMaxRange() { return maxRange; }

    // --- 데이터 설정 메서드 (필요하다면) --- //
    public void SetCurrentHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnPlayerStatsChanged?.Invoke();
    }
    public void SetCurrentEnergy(int newEnergy)
    {
        currentEnergy = Mathf.Clamp(newEnergy, 0, maxEnergy);
        OnPlayerStatsChanged?.Invoke();
    }
    public void SetCurrentWater(int newWater)
    {
        currentWater = Mathf.Clamp(newWater, 0, maxWater);
        OnPlayerStatsChanged?.Invoke();
    }

    // --- 데이터 값 변화 메서드 --- //
    public void IncreaseCurrentHealth(float amount)
    {
        float newHealth = currentHealth + amount;
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnPlayerStatsChanged?.Invoke();
    }
    public void IncreaseCurrentEnergy(int amount)
    {
        int newEnergy = currentEnergy + amount; 
        currentEnergy = Mathf.Clamp(newEnergy, 0, maxEnergy);
        OnPlayerStatsChanged?.Invoke();
    }
    public void IncreaseCurrentWater(int amount)
    {
        int newWater = currentWater + amount;
        currentWater = Mathf.Clamp(newWater, 0, maxWater);
        OnPlayerStatsChanged?.Invoke();
    }
}




