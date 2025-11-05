using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerStateUI : MonoBehaviour
{
    public TextMeshProUGUI textLevel;
    public TextMeshProUGUI textExp;
    public TextMeshProUGUI textHp;
    public TextMeshProUGUI textEnergy;
    public TextMeshProUGUI textWater;
    public TextMeshProUGUI textAttack;
    public TextMeshProUGUI textRange;

    public float maxHp;
    public int maxEnergy;
    public int maxWater;
    public int maxAttack;
    public float maxRange;

    private int _curLevel;
    private int _curExp;
    private int _maxExp;
    private float _curHp;
    private float _maxHp;
    private int _curEnergy;
    private int _curWater;
    private int _curAttack; 
    private float _curRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _curLevel = 1;
        _curExp = 0;
        _maxExp = 30;
        _curHp = maxHp;
        _maxHp = maxHp;
        _curEnergy = maxEnergy;
        _curWater = maxWater;
        _curAttack = maxAttack;
        _curRange = maxRange;

        StartCoroutine(ReduceEnergyAndWater());
        PrintState();

        GameManager.instance.OnHpChanged += ChangeHp;
        GameManager.instance.OnExpAcquire += AddExp;
        GameManager.instance.OnPlayerDamageGet += GetDamage;
    }

    private void OnDestroy()
    {
        GameManager.instance.OnHpChanged -= ChangeHp;
        GameManager.instance.OnExpAcquire -= AddExp;
        GameManager.instance.OnPlayerDamageGet -= GetDamage;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ReduceEnergyAndWater()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            --_curEnergy;
            --_curWater;
            PrintState();
        }
    }

    void PrintState()
    {
        textLevel.text = $"Lv : <color=#00FF00>{_curLevel}</color>";
        textExp.text = $"Exp :  <color=#00FF00>{_curExp}</color>/{_maxExp}";

        textHp.text = $"HP : <color=#00FF00>{_curHp:F1}</color>/{_maxHp:F1}";
        textEnergy.text = $"Energy :  <color=#00FF00>{_curEnergy}</color>/{maxEnergy}";
        textWater.text = $"Water :  <color=#00FF00>{_curWater}</color>/{maxWater}";
        textAttack.text = $"Attack :  <color=#00FF00>{_curAttack}</color>";
        textRange.text = $"Range :  <color=#00FF00>{_curRange}</color>";
    }

    public void AddExp(int amount)
    {
        _curExp += amount;
        PrintState();

        if (_curExp >= _maxExp)
            LevelUp();
    }

    void LevelUp()
    {
        _curExp -= _maxExp;
        _maxExp += 5;
        ++_curLevel;
        _curAttack += 3;
        _curHp += 5;
        _maxHp += 5;

        PrintState();
    }

    void ChangeHp(float amount)
    {
        _curHp -= amount;
        PrintState();

        if (_curHp <= 0)
            Die();
    }

    void Die()
    {

    }

    int GetDamage()
    {
        return _curAttack;
    }
}
