using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FlagCustomizationUI : MonoBehaviour
{
    public GameObject customizationPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public Button cancelButton;
    public List<Button> colorButtons;

    private PlayerFlagPlacer _playerFlagPlacer;
    private Color _selectedColor = Color.white;

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);

        for (int i = 0; i < colorButtons.Count; i++)
        {
            int index = i;
            colorButtons[i].onClick.AddListener(() => OnColorSelect(colorButtons[index].GetComponent<Image>().color));
        }

        customizationPanel.SetActive(false);
    }

    public void Open()
    {
        customizationPanel.SetActive(true);
        nameInputField.text = "My Flag";
        _selectedColor = Color.white;
    }

    private void OnColorSelect(Color color)
    {
        _selectedColor = color;
    }

    private void OnConfirm()
    {
        if (_playerFlagPlacer == null)
        {
            _playerFlagPlacer = FindFirstObjectByType<PlayerFlagPlacer>();
        }

        if (_playerFlagPlacer != null)
        {
            _playerFlagPlacer.PlaceFlagFromUI(nameInputField.text, _selectedColor);
        }
        customizationPanel.SetActive(false);
    }

    private void OnCancel()
    {
        customizationPanel.SetActive(false);
    }
}
