using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemDataEditor
{
    // Path where the generated assets will be stored
    private const string AssetPath = "Assets/Datas/Items";
    // Default duration for buffs (30 seconds)
    private const float DefaultBuffDuration = 30f;

    [MenuItem("My Game/Create Item Assets")]
    public static void CreateItemAssets()
    {
        if (!Directory.Exists(AssetPath))
        {
            Directory.CreateDirectory(AssetPath);
        }

        ClearExistingAssets();

        // --- Item Data Definition ---

        // Common Items
        CreateItem("Common_None", "Nothing happens...", ItemRarity.Common, ItemEffectType.Nothing, DurationType.Permanent, 0);
        CreateItem("Common_MoveSpeed_Buff", "Speed Up +1 (30s)", ItemRarity.Common, ItemEffectType.MoveSpeed, DurationType.Buff, 1, DefaultBuffDuration);
        CreateItem("Common_ViewAngle_Buff", "Sight Angle Up +10 (30s)", ItemRarity.Common, ItemEffectType.ViewAngle, DurationType.Buff, 10, DefaultBuffDuration);
        CreateItem("Common_AttackDamage_Buff", "Attack Up +1 (30s)", ItemRarity.Common, ItemEffectType.AttackDamage, DurationType.Buff, 1, DefaultBuffDuration);
        CreateItem("Common_MaxHealth_Buff", "Max Health Up +10 (30s)", ItemRarity.Common, ItemEffectType.MaxHealth, DurationType.Buff, 10, DefaultBuffDuration);
        CreateItem("Common_Energy_Permanent", "Energy +10", ItemRarity.Common, ItemEffectType.Energy, DurationType.Permanent, 10);
        CreateItem("Common_Water_Permanent", "Water +10", ItemRarity.Common, ItemEffectType.Water, DurationType.Permanent, 10);
        CreateItem("Common_Health_Permanent", "Health +10", ItemRarity.Common, ItemEffectType.Health, DurationType.Permanent, 10);
        CreateItem("Common_StatSave_Permanent", "Save Stats 0.5%", ItemRarity.Common, ItemEffectType.StatSave, DurationType.Permanent, 0.5f);

        // Rare Items
        CreateItem("Rare_MoveSpeed_Buff", "Speed Up +2 (30s)", ItemRarity.Rare, ItemEffectType.MoveSpeed, DurationType.Buff, 2, DefaultBuffDuration);
        CreateItem("Rare_ViewAngle_Buff", "Sight Angle Up +20 (30s)", ItemRarity.Rare, ItemEffectType.ViewAngle, DurationType.Buff, 20, DefaultBuffDuration);
        CreateItem("Rare_AttackDamage_Buff", "Attack Up +2 (30s)", ItemRarity.Rare, ItemEffectType.AttackDamage, DurationType.Buff, 2, DefaultBuffDuration);
        CreateItem("Rare_MaxHealth_Buff", "Max Health Up +25 (30s)", ItemRarity.Rare, ItemEffectType.MaxHealth, DurationType.Buff, 25, DefaultBuffDuration);
        CreateItem("Rare_Energy_Permanent", "Energy +25", ItemRarity.Rare, ItemEffectType.Energy, DurationType.Permanent, 25);
        CreateItem("Rare_Water_Permanent", "Water +25", ItemRarity.Rare, ItemEffectType.Water, DurationType.Permanent, 25);
        CreateItem("Rare_Health_Permanent", "Health +25", ItemRarity.Rare, ItemEffectType.Health, DurationType.Permanent, 25);
        CreateItem("Rare_StatSave_Permanent", "Save Stats 1.5%", ItemRarity.Rare, ItemEffectType.StatSave, DurationType.Permanent, 1.5f);
        CreateItem("Rare_KeyMarker_Permanent", "Mark Key on Stage 2", ItemRarity.Rare, ItemEffectType.KeyMarker, DurationType.Permanent, 0, 0, "Stage_2");

        // Epic Items
        CreateItem("Epic_MoveSpeed_Buff", "Speed Up +4 (30s)", ItemRarity.Epic, ItemEffectType.MoveSpeed, DurationType.Buff, 4, DefaultBuffDuration);
        CreateItem("Epic_ViewAngle_Buff", "Sight Angle Up +45 (30s)", ItemRarity.Epic, ItemEffectType.ViewAngle, DurationType.Buff, 45, DefaultBuffDuration);
        CreateItem("Epic_AttackDamage_Buff", "Attack Up +5 (30s)", ItemRarity.Epic, ItemEffectType.AttackDamage, DurationType.Buff, 5, DefaultBuffDuration);
        CreateItem("Epic_MaxHealth_Buff", "Max Health Up +60 (30s)", ItemRarity.Epic, ItemEffectType.MaxHealth, DurationType.Buff, 60, DefaultBuffDuration);
        CreateItem("Epic_Energy_Permanent", "Energy +80", ItemRarity.Epic, ItemEffectType.Energy, DurationType.Permanent, 80);
        CreateItem("Epic_Water_Permanent", "Water +80", ItemRarity.Epic, ItemEffectType.Water, DurationType.Permanent, 80);
        CreateItem("Epic_Health_Permanent", "Health +80", ItemRarity.Epic, ItemEffectType.Health, DurationType.Permanent, 80);
        CreateItem("Epic_StatSave_Permanent", "Save Stats 4%", ItemRarity.Epic, ItemEffectType.StatSave, DurationType.Permanent, 4);
        CreateItem("Epic_KeyMarker_Permanent", "Mark Key on Stage 3", ItemRarity.Epic, ItemEffectType.KeyMarker, DurationType.Permanent, 0, 0, "Stage_3");
        CreateItem("Epic_Shelter_Permanent", "Grants 1 Temporary Shelter", ItemRarity.Epic, ItemEffectType.Shelter, DurationType.Permanent, 1);

        // Legendary Items
        CreateItem("Legendary_MoveSpeed_Buff", "Speed Up +8 (30s)", ItemRarity.Legendary, ItemEffectType.MoveSpeed, DurationType.Buff, 8, DefaultBuffDuration);
        CreateItem("Legendary_FullView_Buff", "Sight Angle Up +60 (180s)", ItemRarity.Legendary, ItemEffectType.FullView, DurationType.Buff, 60, 180f);
        CreateItem("Legendary_AttackDamage_Buff", "Attack Up +12 (30s)", ItemRarity.Legendary, ItemEffectType.AttackDamage, DurationType.Buff, 12, DefaultBuffDuration);
        CreateItem("Legendary_MaxHealth_Buff", "Max Health Up +150 (30s)", ItemRarity.Legendary, ItemEffectType.MaxHealth, DurationType.Buff, 150, DefaultBuffDuration);
        CreateItem("Legendary_StatSave_Permanent", "Save Stats 10%", ItemRarity.Legendary, ItemEffectType.StatSave, DurationType.Permanent, 10);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Item assets re-balanced and created successfully! Total: " + (Directory.GetFiles(AssetPath, "*.asset").Length) + " assets in " + AssetPath);
    }

    private static void ClearExistingAssets()
    {
        if (!Directory.Exists(AssetPath)) return;
        string[] oldAssets = Directory.GetFiles(AssetPath, "*.asset");
        if (oldAssets.Length > 0)
        {
            Debug.Log("Clearing " + oldAssets.Length + " existing item assets...");
            foreach (string asset in oldAssets)
            {
                AssetDatabase.DeleteAsset(asset);
            }
        }
    }

    private static void CreateItem(string assetName, string desc, ItemRarity rarity, ItemEffectType effectType, DurationType durationType, float val, float dur = 0, string stringVal = "")
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();

        item.itemName = assetName.Replace("_", " ");
        item.description = desc;
        item.rarity = rarity;
        item.effectType = effectType;
        item.durationType = durationType;
        item.value = val;
        item.duration = dur;
        item.stringValue = stringVal;

        string path = Path.Combine(AssetPath, $"{assetName}.asset");
        AssetDatabase.CreateAsset(item, path);
    }
}