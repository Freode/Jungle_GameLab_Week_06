using System.Collections;
using UnityEngine;

public class RecoverInBase : MonoBehaviour
{
    public string playerTag = "Player"; // 플레이어 오브젝트의 태그

    private Coroutine _recoverCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) == false)
            return;

        _recoverCoroutine = StartCoroutine(RecoverState());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) == false)
            return;

        StopCoroutine(_recoverCoroutine);
    }

    IEnumerator RecoverState()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);

            PlayerDataManager.instance.IncreaseCurrentHealth(5);
            PlayerDataManager.instance.IncreaseCurrentEnergy(5);
            PlayerDataManager.instance.IncreaseCurrentWater(5);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
