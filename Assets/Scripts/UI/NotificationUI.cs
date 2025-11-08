using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    public GameObject notificationPanel; // Assign a UI Panel in the Inspector

    public TextMeshProUGUI textGrade;
    public TextMeshProUGUI notificationText; // Assign a TextMeshPro object in the Inspector

    private Coroutine _hideCoroutine;

    void Start()
    {
        // Subscribe to the event when this UI object is enabled
        if (ItemManager.instance != null)
        {
            ItemManager.instance.OnItemApplied += ShowNotification;
        }

        // Ensure the panel is hidden at the start
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event when this UI object is destroyed
        if (ItemManager.instance != null)
        {
            ItemManager.instance.OnItemApplied -= ShowNotification;
        }
    }

    private void ShowNotification(ItemData item)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("NotificationUI is not set up correctly. Assign the panel and text in the Inspector.");
            return;
        }

        // If a notification is already showing, stop the old hiding coroutine
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        Color color = Color.white;
        switch(item.grade)
        {
            case ItemGrade.Legendary:
                color = Color.red;
                break;

            case ItemGrade.Epic:
                color = Color.yellow;
                break;

            case ItemGrade.Rare:
                color = Color.green;
                break;

            case ItemGrade.Common:
                color = Color.white;
                break;
        }

        textGrade.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}><{item.grade.ToString()}></color>";
        notificationText.text = $"{item.description}";


        // Set the text based on the item data
        // You can customize this format
        //notificationText.text = $"<color=yellow>{item.itemName}</color>\n<size=80%>{item.description}</size>";

        // Show the panel and start a coroutine to hide it after a delay
        notificationPanel.SetActive(true);
        _hideCoroutine = StartCoroutine(HideAfterDelay(5f)); // Hides after 5 seconds
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPanel.SetActive(false);
        _hideCoroutine = null;
    }
}
