using UnityEngine;

public class UnitVision : MonoBehaviour
{
    // 유닛이 생성되거나 활성화될 때 매니저의 리스트에 자신을 추가
    void OnEnable()
    {
        FogOfWarManager.VisionUnits.Add(transform);
    }

    // 유닛이 파괴되거나 비활성화될 때 리스트에서 자신을 제거
    void OnDisable()
    {
        FogOfWarManager.VisionUnits.Remove(transform);
    }
}