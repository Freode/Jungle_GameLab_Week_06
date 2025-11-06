using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad = "SampleScene";
    public string playerTag = "Player"; // 플레이어 오브젝트의 태그
    public TextMeshPro textStage;

    private void Start()
    {
        textStage.text = sceneToLoad;
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그를 가진 오브젝트가 트리거에 진입하면 씬 로드
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"플레이어가 트리거에 진입하여 {sceneToLoad} 씬으로 이동합니다.");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
