using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject cubeGameObject;
    public string sceneToLoad = "SampleScene";
    public string playerTag = "Player"; // 플레이어 오브젝트의 태그
    public TextMeshPro textStage;
    public bool isLoadActive = true;

    private bool _isActive = true;

    private void Start()
    {
        _isActive = isLoadActive;
        textStage.text = $"Move to\n{sceneToLoad}";

        if (_isActive == false)
        {
            textStage.text = textStage.text + "\n(Inactive)";

            _isActive = false;
            MeshRenderer meshRenderer = cubeGameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.material.color = Color.black;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그를 가진 오브젝트가 트리거에 진입하면 씬 로드
        if (other.CompareTag(playerTag) && _isActive)
        {
            Debug.Log($"플레이어가 트리거에 진입하여 {sceneToLoad} 씬으로 이동합니다.");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
