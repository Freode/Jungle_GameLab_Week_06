using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ExplorationStatManager : MonoBehaviour
{
    public static ExplorationStatManager instance;

    // Stats increase per percentage of exploration
    [Header("Stat Boost per Exploration %")]
    [SerializeField] private float healthBoostPerPercent = 0.5f;
    [SerializeField] private float attackBoostPerPercent = 0.1f;

    private bool initialBoostApplied = false;

    private List<string> explorableScenes = new List<string> { "Stage_1", "Stage_2", "Stage_3" };

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
        if (!initialBoostApplied)
        {
            ApplyInitialStatBoost();
            initialBoostApplied = true;
        }
    }

    private void ApplyInitialStatBoost()
    {
        long totalExploredPixels = 0;
        long totalPixels = 0;

        foreach (string sceneName in explorableScenes)
        {
            string key = $"FogData_{sceneName}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                try
                {
                    // We need access to FogOfWarManager.FogData
                    FogOfWarManager.FogData data = JsonUtility.FromJson<FogOfWarManager.FogData>(json);
                    if (data != null && data.isExplored != null)
                    {
                        totalPixels += data.isExplored.Length;
                        for (int i = 0; i < data.isExplored.Length; i++)
                        {
                            if (data.isExplored[i])
                            {
                                totalExploredPixels++;
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ExplorationStatManager] Failed to parse FogData for scene {sceneName}: {e.Message}");
                }
            }
        }

        if (totalPixels == 0) return;

        float totalExplorationPercentage = ((float)totalExploredPixels / totalPixels) * 100f;

        float healthBoost = totalExplorationPercentage * healthBoostPerPercent;
        float attackBoost = totalExplorationPercentage * attackBoostPerPercent;

        if (PlayerDataManager.instance != null)
        {
            Debug.Log($"[ExplorationStatManager] Applying initial stat boost. Total Exploration: {totalExplorationPercentage:F2}%. Health: +{healthBoost}, Attack: +{attackBoost}");
            PlayerDataManager.instance.currentMaxHealth += healthBoost;
            PlayerDataManager.instance.currentHealth += healthBoost; // Also increase current health
            PlayerDataManager.instance.currentAttackPower += attackBoost;
        }
    }

    public void OnPixelExplored(int totalPixelsInScene)
    {
        if (totalPixelsInScene == 0) return;

        // Calculate the stat increase for one pixel
        float singlePixelPercentage = (1.0f / totalPixelsInScene) * 100f;
        float healthIncrease = singlePixelPercentage * healthBoostPerPercent;
        float attackIncrease = singlePixelPercentage * attackBoostPerPercent;

        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.AddStagePersistMaxHealth(healthIncrease);
            PlayerDataManager.instance.AddStagePersistPower(attackIncrease);
        }
    }
}
