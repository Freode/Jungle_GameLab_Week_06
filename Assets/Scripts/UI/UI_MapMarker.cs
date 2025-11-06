using UnityEngine;
using TMPro;

public class UI_MapMarker : MonoBehaviour
{
    // TMP_Text를 사용한다면 using TMPro; 추가 후
    // public TMPro.TMP_Text markerText;
    public TextMeshProUGUI markerText; // 깃발 텍스트를 표시할 UI Text 컴포넌트

    public void SetText(string text)
    {
        if (markerText != null)
        {
            markerText.text = text;
        }
    }
}
