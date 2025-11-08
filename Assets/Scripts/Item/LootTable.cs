using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<ItemData> items;

    // Get a random item from the list based on weights
    public ItemData GetRandomItem()
    {
        if (items == null || items.Count == 0)
        {
            return null;
        }

        float totalWeight = 0;
        foreach (var item in items)
        {
            totalWeight += item.probabilityWeight;
        }

        float randomPoint = Random.Range(0, totalWeight);

        foreach (var item in items)
        {
            if (randomPoint < item.probabilityWeight)
            {
                return item;
            }
            else
            {
                randomPoint -= item.probabilityWeight;
            }
        }
        // Should not happen if weights are set up correctly, but as a fallback
        return items[items.Count - 1];
    }
}
