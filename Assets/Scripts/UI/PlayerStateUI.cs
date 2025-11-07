using TMPro;
using UnityEngine;

public class PlayerStateUI : MonoBehaviour
{
    public TextMeshProUGUI textRemainCamp;
    public TextMeshProUGUI textLevel;
    public TextMeshProUGUI textExp;
    public TextMeshProUGUI textHp;
    public TextMeshProUGUI textEnergy;
    public TextMeshProUGUI textWater;
    public TextMeshProUGUI textAttack;
    public TextMeshProUGUI textRange;

    void Start()
    {
        // PlayerDataManager의 데이터 변경 이벤트 구독
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerStatsChanged += PrintState;
            PlayerDataManager.instance.OnRecoverPlaceCountChanged += PrintRemainCamp;
        }
        // GameManager 이벤트 구독 해제 (이제 PlayerDataManager가 관리)
        if (GameManager.instance != null)
        {
            // 기존 구독 해제 (GameManager가 PlayerDataManager의 변경을 구독하도록 변경 필요)
            // GameManager.instance.OnHpChanged -= ChangeHp;
            // GameManager.instance.OnExpAcquire -= AddExp;
            // GameManager.instance.OnPlayerDamageGet -= GetDamage;
        }

        PrintState(); // 초기 상태 출력
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerStatsChanged -= PrintState;
            PlayerDataManager.instance.OnRecoverPlaceCountChanged -= PrintRemainCamp;
        }
    }

    // PlayerDataManager로부터 데이터를 가져와 UI에 출력
    void PrintState()
    {
        if (PlayerDataManager.instance == null) return;

        textLevel.text = $"Lv : <color=#00FF00>{PlayerDataManager.instance.GetCurrentLevel()}</color>";
        textExp.text = $"Exp :  <color=#00FF00>{PlayerDataManager.instance.GetCurrentExp()}</color>/{PlayerDataManager.instance.GetMaxExp()}";

        textHp.text = $"HP : <color=#00FF00>{PlayerDataManager.instance.GetCurrentHealth():F1}</color>/{PlayerDataManager.instance.GetMaxHealth():F1}";
        textEnergy.text = $"Energy :  <color=#00FF00>{PlayerDataManager.instance.GetCurrentEnergy()}</color>/{PlayerDataManager.instance.GetMaxEnergy()}";
        textWater.text = $"Water :  <color=#00FF00>{PlayerDataManager.instance.GetCurrentWater()}</color>/{PlayerDataManager.instance.GetMaxWater()}";
        textAttack.text = $"Attack :  <color=#00FF00>{PlayerDataManager.instance.GetAttackPower()}</color>";
        textRange.text = $"Range :  <color=#00FF00>{PlayerDataManager.instance.GetCurrentRange()}</color>";
    }

    private void PrintRemainCamp()
    {
        int count = PlayerDataManager.instance.GetRecoverPlaceCount();
        textRemainCamp.text = $"<color=#00FF00>E</color> - Remain Camp : <color=#00FF00>{count}</color>";
    }

    // 경험치 추가 (PlayerDataManager에 위임)
    public void AddExp(int amount)
    {
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.AddExp(amount);
        }
    }

    // 공격력 가져오기 (PlayerDataManager에 위임)
    int GetDamage()
    {
        if (PlayerDataManager.instance != null)
        {
            return (int)PlayerDataManager.instance.GetAttackPower();
        }
        return 0;
    }
}
