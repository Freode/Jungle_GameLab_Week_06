using System.Collections.Generic;
using UnityEngine;

public class FogDataManager : MonoBehaviour
{
    public static FogDataManager instance;

    public List<string> foggedSceneNames; // 안개 데이터를 관리할 씬 이름 목록
    public int combinedTextureSize = 2048; // 통합 맵 텍스처의 해상도 (예: 2048x2048)
    public int individualSceneTextureSize = 1024; // 개별 씬 안개 맵의 해상도 (예: 1024x1024)

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 모든 씬의 탐험된 안개 데이터를 통합하여 하나의 Texture2D로 반환합니다.
    /// 각 씬의 안개 맵은 combinedTextureSize 내에서 그리드 형태로 배치됩니다.
    /// </summary>
    /// <param name="unexploredColor">미탐험 영역의 색상</param>
    /// <param name="exploredColor">탐험된 영역의 색상</param>
    /// <returns>통합된 안개 맵 Texture2D</returns>
    public Texture2D GetCombinedExploredMapTexture(Color unexploredColor, Color exploredColor)
    {
        // ... (기존 코드 유지)
        // 이 메서드는 이제 사용되지 않거나 다른 용도로 사용될 수 있습니다.
        // 개별 씬 맵을 가져오는 GetIndividualExploredMapTexture를 사용하세요.
        // ...

        // 통합 텍스처 생성
        Texture2D combinedTexture = new Texture2D(combinedTextureSize, combinedTextureSize, TextureFormat.RGBA32, false);
        Color[] combinedPixels = new Color[combinedTextureSize * combinedTextureSize];

        // 모든 픽셀을 미탐험 상태로 초기화
        for (int i = 0; i < combinedPixels.Length; i++)
        {
            combinedPixels[i] = unexploredColor;
        }

        // 그리드 계산 (예: 2048x2048에 1024x1024 맵을 배치하면 2x2 그리드)
        int mapsPerRow = combinedTextureSize / individualSceneTextureSize;
        if (mapsPerRow == 0) mapsPerRow = 1; // 최소 1개

        // 각 씬의 안개 데이터를 불러와 통합 텍스처에 매핑
        for (int sceneIndex = 0; sceneIndex < foggedSceneNames.Count; sceneIndex++)
        {
            string sceneName = foggedSceneNames[sceneIndex];
            if (PlayerPrefs.HasKey($"FogData_{sceneName}"))
            {
                string json = PlayerPrefs.GetString($"FogData_{sceneName}");
                try
                {
                    FogOfWarManager.FogData sceneFogData = JsonUtility.FromJson<FogOfWarManager.FogData>(json);

                    if (sceneFogData.isExplored != null && sceneFogData.isExplored.Length == individualSceneTextureSize * individualSceneTextureSize)
                    {
                        // 통합 텍스처 내에서 현재 씬 맵이 시작될 위치 계산
                        int col = sceneIndex % mapsPerRow;
                        int row = sceneIndex / mapsPerRow;

                        int offsetX = col * individualSceneTextureSize;
                        int offsetY = row * individualSceneTextureSize; // Unity 텍스처는 보통 좌하단이 (0,0)

                        for (int y = 0; y < individualSceneTextureSize; y++)
                        {
                            for (int x = 0; x < individualSceneTextureSize; x++)
                            {
                                int scenePixelIndex = y * individualSceneTextureSize + x;
                                if (sceneFogData.isExplored[scenePixelIndex])
                                {
                                    int targetX = offsetX + x;
                                    int targetY = offsetY + y;
                                    // 통합 텍스처 범위 내에 있는지 확인
                                    if (targetX < combinedTextureSize && targetY < combinedTextureSize)
                                    {
                                        combinedPixels[targetY * combinedTextureSize + targetX] = exploredColor;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[FogDataManager] 씬 '{sceneName}'의 안개 데이터 크기({sceneFogData.isExplored?.Length})가 예상({individualSceneTextureSize * individualSceneTextureSize})과 다릅니다. 통합에 실패했습니다.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[FogDataManager] 씬 '{sceneName}'의 안개 데이터 로드 실패 (JSON 파싱 오류): {e.Message}");
                }
            }
        }

        combinedTexture.SetPixels(combinedPixels);
        combinedTexture.Apply();
        return combinedTexture;
    }

    /// <summary>
    /// 특정 씬의 탐험된 안개 데이터를 Texture2D로 반환합니다.
    /// </summary>
    /// <param name="sceneName">안개 데이터를 가져올 씬 이름</param>
    /// <param name="textureSize">안개 맵 텍스처의 해상도</param>
    /// <param name="unexploredColor">미탐험 영역의 색상</param>
    /// <param name="exploredColor">탐험된 영역의 색상</param>
    /// <returns>씬의 안개 맵 Texture2D</returns>
    public Texture2D GetIndividualExploredMapTexture(string sceneName, int textureSize, Color unexploredColor, Color exploredColor)
    {
        Texture2D sceneTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] scenePixels = new Color[textureSize * textureSize];

        // 모든 픽셀을 미탐험 상태로 초기화
        for (int i = 0; i < scenePixels.Length; i++)
        {
            scenePixels[i] = unexploredColor;
        }

        if (PlayerPrefs.HasKey($"FogData_{sceneName}"))
        {
            string json = PlayerPrefs.GetString($"FogData_{sceneName}");
            try
            {
                FogOfWarManager.FogData sceneFogData = JsonUtility.FromJson<FogOfWarManager.FogData>(json);

                if (sceneFogData.isExplored != null && sceneFogData.isExplored.Length == textureSize * textureSize)
                {
                    for (int i = 0; i < sceneFogData.isExplored.Length; i++)
                    {
                        if (sceneFogData.isExplored[i])
                        {
                            scenePixels[i] = exploredColor; // 탐험된 영역은 표시
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[FogDataManager] 씬 '{sceneName}'의 안개 데이터 크기({sceneFogData.isExplored?.Length})가 예상({textureSize * textureSize})과 다릅니다. 개별 맵 로드에 실패했습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FogDataManager] 씬 '{sceneName}'의 안개 데이터 로드 실패 (JSON 파싱 오류): {e.Message}");
            }
        }

        sceneTexture.SetPixels(scenePixels);
        sceneTexture.Apply();
        return sceneTexture;
    }
}
