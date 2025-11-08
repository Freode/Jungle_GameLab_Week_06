using UnityEngine;

public class ItemBox : BaseInteract, IInteract
{
    public GameObject useIcon;

    [SerializeField]
    private LootTable lootTable;

    [SerializeField]
    private bool destroyOnUse = true; // Option to destroy the box after interaction

    private bool _isUsed = false;

    // BaseInteract already implements IInteract.
    // We provide a new implementation for OnClick for this specific class.
    void IInteract.OnClick()
    {
        if (_canInteract == false) return;

        if (_isUsed) return;

        if (lootTable == null)
        {
            Debug.LogError("LootTable is not assigned to this ItemBox.", gameObject);
            return;
        }

        Debug.Log("Item Use");

        ItemData chosenItem = lootTable.GetRandomItem();

        if (chosenItem != null && ItemManager.instance != null)
        {
            ItemManager.instance.ApplyItemEffect(chosenItem);
        }

        if (destroyOnUse)
        {
            // To prevent further interaction and make it visually disappear
            _isUsed = true;
            Destroy(gameObject);
        }
        else
        {
            // Or handle cooldown if it's not destroyed
            useIcon.SetActive(true);
            _isUsed = true;
            // You could add cooldown logic here, e.g., disabling the collider for a while.
        }
    }
}
