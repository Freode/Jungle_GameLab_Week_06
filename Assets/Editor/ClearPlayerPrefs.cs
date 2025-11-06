using UnityEditor;
using UnityEngine;

public class ClearPlayerPrefs
{
    [MenuItem("Tools/Clear All PlayerPrefs")]
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("모든 PlayerPrefs 데이터가 삭제되었습니다.");
    }
}
