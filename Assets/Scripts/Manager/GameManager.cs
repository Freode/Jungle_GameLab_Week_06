using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public event Action<float> OnHpChanged;
    public event Action<int> OnExpAcquire;
    public event Func<int> OnPlayerDamageGet;

    void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }


    public void AnnounceHpChanged(float amount)
    {
        OnHpChanged?.Invoke(amount);
    }

    public void AnnounceExpAcquire(int amount)
    {
        OnExpAcquire?.Invoke(amount);
    }

    public int GetPlayerDamage()
    {
        return OnPlayerDamageGet.Invoke();
    }
}
