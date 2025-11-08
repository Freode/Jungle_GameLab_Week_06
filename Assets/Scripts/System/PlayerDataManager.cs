using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;

    // 플레이어의 지속적인 데이터
    public float currentMaxHealth = 100f;       // 플레이어의 현재 최대 체력
    public float totalMaxHealth                 // 플레이어의 전체 최대 체력
    {
        get { return currentMaxHealth + stagePersistMaxHealth + stageBuffMaxHealth; }
    }
    public float currentHealth = 0f;            // 플레이어의 현재 체력
    public float stagePersistMaxHealth = 0f;    // 플레이어가 현재 스테이지에서 먹은 체력 (탈출 시, 영구)
    public float stageBuffMaxHealth = 0f;       // 플레이어가 현재 스테이지에서 얻은 버프 (일정 시간 유지)

    public float currentAttackPower = 5f;          // 플레이어의 현재 공격력
    public float totalAttackPower                 // 플레이어의 전체 공격력
    {
        get { return Mathf.Max(0, currentAttackPower + stagePersistPower + stageBuffPower); }
    }
    public float stagePersistPower = 0f;           // 플레이어가 현재 스테이지에서 얻은 공격력 (탈출 시, 영구)
    public float stageBuffPower = 0f;              // 플레이어가 현재 스테이지에서 얻은 버프 (일정 시간 유지)

    public float playerTotalPower = 0f;            // 플레이어의 현재 전투력

    public int currentLevel = 1;
    public int currentExp = 0;
    public int maxExp = 30;

    public int maxEnergy = 100;             
    public int currentEnergy;

    public int maxWater = 100;
    public int currentWater;

    public float maxRange = 10f;
    public float currentRange;

    public int _recoverPlaceCount = 0;

    

    // 데이터 변경을 알리는 이벤트
    public event Action OnPlayerStatsChanged;
    public event Action OnRecoverPlaceCountChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // 게임 시작 시 현재 체력, 에너지, 물, 범위 초기화
            currentHealth = totalMaxHealth;
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
        currentHealth = Mathf.Clamp(currentHealth, 0, totalMaxHealth); // 체력이 0 미만, 최대 체력 초과 방지
        Debug.Log($"플레이어 데이터 매니저 체력: {currentHealth} / {totalMaxHealth}");
        OnPlayerStatsChanged?.Invoke(); // 데이터 변경 알림

        // 플레이어 사망
        if (currentHealth <= 0)
        {
            currentHealth = totalMaxHealth;
            SaveStageData(false);
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
        currentAttackPower += 3f;
        currentMaxHealth += 5;
        currentHealth += 5; // 레벨업 시 체력 회복
        
        OnPlayerStatsChanged?.Invoke(); // 데이터 변경 알림
    }

    // --- 데이터 조회 메서드 --- //
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return totalMaxHealth; }
    public float GetAttackPower() { return totalAttackPower; }
    public int GetCurrentLevel() { return currentLevel; }
    public int GetCurrentExp() { return currentExp; }
    public int GetMaxExp() { return maxExp; }
    public int GetCurrentEnergy() { return currentEnergy; }
    public int GetMaxEnergy() { return maxEnergy; }
    public int GetCurrentWater() { return currentWater; }
    public int GetMaxWater() { return maxWater; }
    public float GetCurrentRange() { return currentRange; }
    public float GetMaxRange() { return maxRange; }
    public int GetRecoverPlaceCount() { return _recoverPlaceCount;}

    // --- 데이터 설정 메서드 (필요하다면) --- //
    public void SetCurrentHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, totalMaxHealth);
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
        currentHealth = Mathf.Clamp(newHealth, 0, totalMaxHealth);
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
    
    public void IncreaseRecoverPlaceCount(int amount)
    {
        int newCount = _recoverPlaceCount + amount;
        _recoverPlaceCount = Mathf.Max(0, newCount);
        OnRecoverPlaceCountChanged?.Invoke();
    }

    public void IncreaseCurrentMaxHealth(float amount)
    {
        if (amount == 0) return;
        currentMaxHealth += amount;
        // Also increase current health if we're adding max health
        if (amount > 0)
        {
            currentHealth += amount;
        }
        currentHealth = Mathf.Clamp(currentHealth, 0, totalMaxHealth);
        OnPlayerStatsChanged?.Invoke();
    }

    public void IncreaseCurrentAttackPower(float amount)
    {
        if (amount == 0) return;
        currentAttackPower += amount;
        OnPlayerStatsChanged?.Invoke();
    }

    public void AddStagePersistMaxHealth(float amount)
    {
        float oldStagePersistMaxHealth = stagePersistMaxHealth;
        stagePersistMaxHealth = Mathf.Max(0, stagePersistMaxHealth + amount);
        float delta = stagePersistMaxHealth - oldStagePersistMaxHealth;

        if (delta == 0) return;

        if (delta > 0)
        {
            currentHealth += delta;
        }
        else
        {
            if (currentHealth > totalMaxHealth)
            {
                currentHealth = totalMaxHealth;
            }
        }
        
        currentHealth = Mathf.Clamp(currentHealth, 0, totalMaxHealth);

        OnPlayerStatsChanged?.Invoke();
    }

    public void AddStageBuffMaxHealth(float amount)
    {
        float oldStageBuffMaxHealth = stageBuffMaxHealth;
        stageBuffMaxHealth = Mathf.Max(0, stageBuffMaxHealth + amount);
        float delta = stageBuffMaxHealth - oldStageBuffMaxHealth;

        if (delta == 0) return;

        if (delta > 0)
        {
            currentHealth += delta;
        }
        else
        {
            if (currentHealth > totalMaxHealth)
            {
                currentHealth = totalMaxHealth;
            }
        }
        
        currentHealth = Mathf.Clamp(currentHealth, 0, totalMaxHealth);

        OnPlayerStatsChanged?.Invoke();
    }

    public void AddStagePersistPower(float amount)
    {
        float oldVal = stagePersistPower;
        stagePersistPower = Mathf.Max(0, stagePersistPower + amount);

        if (oldVal != stagePersistPower)
            OnPlayerStatsChanged?.Invoke();
    }

    public void AddStageBuffPower(float amount)
    {
        float oldVal = stageBuffPower;
        stageBuffPower = Mathf.Max(0, stageBuffPower + amount);

        if (oldVal != stageBuffPower)
            OnPlayerStatsChanged?.Invoke();
    }

    public void SaveStageData(bool isSave)
    {
        if (isSave)
        {
            // --- Commit exploration data ---
            if (FogOfWarManager.instance != null)
            {
                FogOfWarManager.instance.CommitFogData();
                Debug.Log("[PlayerDataManager] Fog of war data committed.");
            }
            else
            {
                Debug.LogWarning("[PlayerDataManager] Could not find FogOfWarManager instance in the scene to commit data.");
            }

            // Persist stats
            currentMaxHealth += stagePersistMaxHealth;
            currentAttackPower += stagePersistPower;
        }

        // Reset temporary stats
        stagePersistMaxHealth = 0f;
        stagePersistPower = 0f;
        stageBuffMaxHealth = 0f;
        stageBuffPower = 0f;

        // Heal player to full health for the next stage
        currentHealth = totalMaxHealth;

        Debug.Log($"[PlayerDataManager] State : {isSave}, Success to save data. Stats persisted and buffs reset.");
        OnPlayerStatsChanged?.Invoke();
    }
}