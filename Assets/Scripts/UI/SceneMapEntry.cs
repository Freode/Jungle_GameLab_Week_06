using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SceneMapEntry : MonoBehaviour
{
    public RawImage worldMapImage; // 실제 월드맵 PNG를 표시할 RawImage
    public RawImage fogOverlayImage; // 안개 오버레이를 표시할 RawImage (fogTexture 할당)
    public TextMeshProUGUI sceneNameText;

    public GameObject flagIconPrefab; // 깃발 아이콘 프리팹
    public RectTransform flagParent; // 깃발 아이콘들이 배치될 부모 Transform

    private List<GameObject> _instantiatedFlags = new List<GameObject>();

    /// <summary>
    /// 씬 맵 엔트리를 설정합니다.
    /// </summary>
    /// <param name="fogTexture">씬의 탐험된 안개 텍스처</param>
    /// <param name="worldMapPNG">씬의 실제 월드맵 PNG 텍스처</param>
    /// <param name="name">씬 이름</param>
    /// <param name="markers">씬의 깃발 데이터 목록</param>
    /// <param name="sceneWorldSize">씬의 월드 크기 (깃발 위치 변환용)</param>
    public void Setup(Texture2D fogTexture, Texture2D worldMapPNG, string name, List<MapMarkerData> markers, Vector2 sceneWorldSize)
    {
        // 기존 깃발 제거
        foreach (GameObject flag in _instantiatedFlags)
        {
            Destroy(flag);
        }
        _instantiatedFlags.Clear();

        // 월드맵 PNG 설정
        if (worldMapImage != null)
        {
            worldMapImage.texture = worldMapPNG;
        }

        // 안개 오버레이 설정
        if (fogOverlayImage != null)
        {
            fogOverlayImage.texture = fogTexture;
            // 안개 오버레이의 색상과 알파값을 조절하여 PNG가 비치도록 함
            // 탐험된 곳은 투명하게, 미탐험된 곳은 검은색으로 보이도록 셰이더 또는 Material 설정 필요
            // 여기서는 간단하게 알파값을 조절하여 안개 텍스처가 PNG 위에 겹쳐지도록 함
            // fogOverlayImage.color = new Color(1, 1, 1, 0.5f); // 흰색에 50% 투명도 (안개 텍스처가 검은색이면 어둡게 보임)
        }

        // 씬 이름 설정
        if (sceneNameText != null)
        {
            sceneNameText.text = name;
        }

        // 깃발 데이터 표시
        if (flagIconPrefab != null && flagParent != null && markers != null && markers.Count > 0)
        {
            // RawImage의 RectTransform 크기를 가져옴
            RectTransform mapRectTransform = fogOverlayImage.rectTransform; // 또는 worldMapImage.rectTransform
            float mapWidth = mapRectTransform.rect.width;
            float mapHeight = mapRectTransform.rect.height;

            Debug.Log("Test : " + sceneWorldSize.x + ", " + sceneWorldSize.y);

            foreach (MapMarkerData markerData in markers)
            {
                GameObject flagIconGO = Instantiate(flagIconPrefab, flagParent);
                _instantiatedFlags.Add(flagIconGO);

                RectTransform flagRectTransform = flagIconGO.GetComponent<RectTransform>();
                if (flagRectTransform != null)
                {
                    // 월드 좌표를 UI RawImage의 로컬 좌표로 변환
                    // (0,0)이 중앙이라고 가정하고, -0.5 ~ 0.5 범위로 정규화 후 UI 크기 곱함
                    float normalizedX = (markerData.position.x / sceneWorldSize.x) + 0.5f;
                    float normalizedY = (markerData.position.z / sceneWorldSize.y) + 0.5f; // Z축을 Y축으로 사용

                    // UI 좌표는 좌하단이 (0,0)이므로, normalizedY를 1에서 빼주거나, offsetY 계산 시 조정
                    // 여기서는 RawImage의 피벗이 중앙(0.5, 0.5)이라고 가정하고 계산
                    float uiX = (normalizedX - 0.5f) * mapWidth;
                    float uiY = (normalizedY - 0.5f) * mapHeight;

                    flagRectTransform.anchoredPosition = new Vector2(uiX, uiY);
                }

                // 깃발 텍스트 설정
                TextMeshProUGUI flagText = flagIconGO.GetComponentInChildren<TextMeshProUGUI>();
                if (flagText != null)
                {
                    flagText.text = markerData.text;
                }
                else
                {
                    Debug.LogWarning($"깃발 아이콘 프리팹 '{flagIconPrefab.name}'에 TextMeshProUGUI 컴포넌트가 없습니다.");
                }
            }
        }
    }
}
