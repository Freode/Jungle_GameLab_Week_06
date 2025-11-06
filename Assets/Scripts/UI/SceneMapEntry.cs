using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneMapEntry : MonoBehaviour
{
    public RawImage sceneMapImage;
    public TextMeshProUGUI sceneNameText;

    public void Setup(Texture2D mapTexture, string name)
    {
        if (sceneMapImage != null)
        {
            sceneMapImage.texture = mapTexture;
        }
        if (sceneNameText != null)
        {
            sceneNameText.text = name;
        }
    }
}
