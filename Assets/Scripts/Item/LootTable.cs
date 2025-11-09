using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    // Wrapper class to hold ItemData and its probability weight
    [System.Serializable]
    public class LootEntry
    {
        public ItemData item;
        public float probabilityWeight = 1f; // Default weight
    }

    public List<LootEntry> lootEntries; // Changed from 'items' to 'lootEntries'

    // Get a random item from the list based on weights
    public ItemData GetRandomItem()
    {
        if (lootEntries == null || lootEntries.Count == 0)
        {
            return null;
        }

        float totalWeight = 0;
        foreach (var entry in lootEntries)
        {
            totalWeight += entry.probabilityWeight;
        }

        float randomPoint = Random.Range(0, totalWeight);

        foreach (var entry in lootEntries)
        {
            if (randomPoint < entry.probabilityWeight)
            {
                return entry.item; // Return the actual ItemData
            }
            else
            {
                randomPoint -= entry.probabilityWeight;
            }
        }
        // Should not happen if weights are set up correctly, but as a fallback
        return lootEntries[lootEntries.Count - 1].item;
    }
}