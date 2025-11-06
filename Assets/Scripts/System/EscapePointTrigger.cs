using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapePointTrigger : MonoBehaviour
{
    public float timeToEscape = 5f; // 탈출에 필요한 시간 (초)
    public string sceneToLoad = "RestScene"; // 이동할 씬 이름
    public string playerTag = "Player"; // 플레이어 태그

    private float _escapeTimer; // 타이머
    private bool _playerInZone; // 플레이어가 구역 내에 있는지 여부

    void Start()
    {
        _escapeTimer = 0f;
        _playerInZone = false;
    }

    void Update()
    {
        if (_playerInZone)
        {
            _escapeTimer += Time.deltaTime;
            if (_escapeTimer >= timeToEscape)
            {
                Debug.Log($"탈출 구역에서 {timeToEscape}초 경과. {sceneToLoad} 씬으로 이동합니다.");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInZone = true;
            Debug.Log("플레이어가 탈출 구역에 진입했습니다. 타이머 시작...");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInZone = false;
            _escapeTimer = 0f;
            Debug.Log("플레이어가 탈출 구역을 벗어났습니다. 타이머 초기화.");
        }
    }
}
