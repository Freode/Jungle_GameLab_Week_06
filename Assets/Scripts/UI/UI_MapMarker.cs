using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_MapMarker : MonoBehaviour
{
    public TextMeshProUGUI markerText; // 깃발 텍스트를 표시할 UI Text 컴포넌트
    public Image markerImage; // 마커의 배경 이미지 또는 아이콘

    public void SetText(string text)
    {
        if (markerText != null)
        {
            markerText.text = text;
        }
    }

    public void SetColor(Color color)
    {
        if (markerImage != null)
        {
            markerImage.color = color;
        }
    }
}
