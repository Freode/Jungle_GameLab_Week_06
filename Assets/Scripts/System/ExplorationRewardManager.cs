using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// JsonUtility가 List<int>를 직접 처리하지 못하는 문제를 해결하기 위한 래퍼 클래스
[System.Serializable]
public class RewardedMilestonesList
{
    public List<int> milestones = new List<int>();
}

public class ExplorationRewardManager : MonoBehaviour
{
    public static ExplorationRewardManager instance;
    public event Action<int> OnAnnounceReward;

    public int[] rewardMilestones = { 20, 40, 60, 80 }; // Percentage milestones for rewards

    // Key: sceneName, Value: List of rewarded milestones for that scene
    private Dictionary<string, RewardedMilestonesList> _rewardStatus = new Dictionary<string, RewardedMilestonesList>();
    private const string SAVE_KEY = "ExplorationRewardStatus";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRewardStatus();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckAndGiveRewards(string sceneName, float currentPercentage)
    {
        if (!_rewardStatus.ContainsKey(sceneName))
        {
            _rewardStatus[sceneName] = new RewardedMilestonesList();
        }

        foreach (int milestone in rewardMilestones)
        {
            if (currentPercentage >= milestone && !_rewardStatus[sceneName].milestones.Contains(milestone))
            {
                GiveReward(sceneName, milestone);
                _rewardStatus[sceneName].milestones.Add(milestone);
                SaveRewardStatus();
            }
        }
    }

    private void GiveReward(string sceneName, int milestone)
    {
        // --- Implement your actual reward logic here ---
        Debug.Log($"Reward given for {sceneName} at {milestone}% exploration!");
        // Example: Add currency, unlock an item, show a UI notification, etc.
        // PlayerDataManager.instance.AddCurrency(100); // Assuming you have a PlayerDataManager
        // UIManager.instance.ShowNotification($"Explored {milestone}% of {sceneName}!");
        // ------------------------------------------------
        OnAnnounceReward?.Invoke(milestone);
        PlayerDataManager.instance.IncreaseRecoverPlaceCount(1);
    }

    private void SaveRewardStatus()
    {
        var serialization = new Serialization<string, RewardedMilestonesList>(_rewardStatus);
        string json = JsonUtility.ToJson(serialization);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Exploration reward status saved.");
    }

    private void LoadRewardStatus()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            try
            {
                var serialization = new Serialization<string, RewardedMilestonesList>(json);
                _rewardStatus = serialization.ToDictionary();
                Debug.Log("Loaded exploration reward status.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load reward status: {e.Message}");
                PlayerPrefs.DeleteKey(SAVE_KEY);
            }
        }
    }
}

