
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위해 추가

public class DataResetter : MonoBehaviour
{
    // 전장의 안개 데이터가 저장되는 씬 이름 목록
    // 프로젝트의 씬 이름에 맞춰 수정해주세요.
    private string[] fogDataSceneNames = {
        "RestScene",
        "Stage_1",
        "Stage_2",
        "Stage_3"
        // 필요한 경우 다른 씬 이름을 여기에 추가하세요.
    };

    void Update()
    {
        // F10 키를 누르면 데이터 삭제
        if (Input.GetKeyDown(KeyCode.F10))
        {
            DeleteFogOfWarData();
        }
    }

    void DeleteFogOfWarData()
    {
        Debug.Log("F10 키가 눌렸습니다. 전장의 안개 데이터를 삭제합니다.");

        foreach (string sceneName in fogDataSceneNames)
        {
            string key = $"FogData_{sceneName}";
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                Debug.Log($"씬 '{sceneName}'의 안개 데이터 (키: {key})가 삭제되었습니다.");
            }
            else
            {
                Debug.Log($"씬 '{sceneName}'의 안개 데이터 (키: {key})가 존재하지 않습니다.");
            }
        }

        PlayerPrefs.Save(); // 변경사항을 즉시 저장
        Debug.Log("모든 전장의 안개 데이터 삭제 완료 및 PlayerPrefs 저장.");

        // 현재 씬을 다시 로드하여 변경사항을 적용 (선택 사항)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
