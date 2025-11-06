using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject mainCameraPrefab; // 메인 카메라 프리팹
    public List<Transform> spawnPoints;
    public List<GameObject> escapePoints;
    public int activeEscapePointsCount = 2;

    public WorldMapUI sceneWorldMapUI; // 씬에 있는 WorldMapUI를 Inspector에서 할당해주세요.

    void Awake()
    {
        // 메인 카메라 생성
        FollowCamera followCamera = InstantiateMainCamera();

        // 플레이어 스폰
        GameObject playerInstance = SpawnPlayer();

        // 카메라에 플레이어 할당
        if (followCamera != null && playerInstance != null)
        {
            followCamera.SetTarget(playerInstance.transform);
            Debug.Log("FollowCamera에 플레이어 타겟을 설정했습니다.");
        }
        else if (followCamera == null)
        {
            Debug.LogWarning("메인 카메라 프리팹에 FollowCamera 컴포넌트가 없거나 카메라 프리팹이 할당되지 않았습니다.");
        }

        // PlayerMapController에 WorldMapUI 할당
        PlayerMapController playerMapController = playerInstance.GetComponent<PlayerMapController>();
        if (playerMapController != null)
        {
            if (sceneWorldMapUI != null)
            {
                playerMapController.worldMapUI = sceneWorldMapUI;
                Debug.Log("PlayerMapController에 WorldMapUI를 자동으로 할당했습니다.");
            }
            else
            {
                Debug.LogWarning("SpawnManager에 WorldMapUI가 할당되지 않았습니다. PlayerMapController에 할당할 수 없습니다.");
            }
        }

        // 탈출 지점 활성화
        ActivateEscapePoints();
    }

    private FollowCamera InstantiateMainCamera()
    {
        if (mainCameraPrefab == null)
        {       
            Debug.LogError("Main Camera Prefab이 할당되지 않았습니다.");
            return null;
        }

        GameObject cameraInstance = Instantiate(mainCameraPrefab);
        Debug.Log("메인 카메라 프리팹을 생성했습니다.");
        return cameraInstance.GetComponent<FollowCamera>();
    }

    private GameObject SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab이 할당되지 않았습니다.");
            return null;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("스폰 포인트가 설정되지 않았습니다.");
            return null;
        }

        // 랜덤 스폰 포인트 선택
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform selectedSpawnPoint = spawnPoints[randomIndex];

        // 플레이어 인스턴스화 및 위치 설정
        GameObject playerInstance = Instantiate(playerPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        Debug.Log($"플레이어가 {selectedSpawnPoint.name}에 스폰되었습니다.");

        // PlayerMapController에 WorldMapUI 할당
        PlayerMapController playerMapController = playerInstance.GetComponent<PlayerMapController>();
        if (playerMapController != null)
        {
            if (sceneWorldMapUI != null)
            {
                playerMapController.worldMapUI = sceneWorldMapUI;
                Debug.Log("PlayerMapController에 WorldMapUI를 자동으로 할당했습니다.");
            }
            else
            {
                Debug.LogWarning("SpawnManager에 WorldMapUI가 할당되지 않았습니다. PlayerMapController에 할당할 수 없습니다.");
            }
        }

        return playerInstance;
    }

    private void ActivateEscapePoints()
    {
        if (escapePoints == null || escapePoints.Count == 0)
        {
            Debug.LogWarning("탈출 지점이 설정되지 않았습니다.");
            return;
        }

        // 모든 탈출 지점 비활성화
        foreach (var ep in escapePoints)
        {
            if (ep != null) ep.SetActive(false);
        }

        // 활성화할 탈출 지점 수 조정
        int countToActivate = Mathf.Min(activeEscapePointsCount, escapePoints.Count);

        // 랜덤으로 탈출 지점 선택 및 활성화
        List<GameObject> availableEscapePoints = new List<GameObject>(escapePoints);
        for (int i = 0; i < countToActivate; i++)
        {
            if (availableEscapePoints.Count == 0) break;

            int randomIndex = Random.Range(0, availableEscapePoints.Count);
            GameObject selectedEscapePoint = availableEscapePoints[randomIndex];

            if (selectedEscapePoint != null)
            {
                selectedEscapePoint.SetActive(true);
                // EscapePointTrigger 컴포넌트 추가 또는 가져오기
                if (selectedEscapePoint.GetComponent<EscapePointTrigger>() == null)
                {
                    selectedEscapePoint.AddComponent<EscapePointTrigger>();
                }
                Debug.Log($"탈출 지점 {selectedEscapePoint.name}이 활성화되었습니다.");
            }
            availableEscapePoints.RemoveAt(randomIndex);
        }
    }
}
