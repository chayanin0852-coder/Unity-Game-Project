
using UnityEngine;

public enum InventoryItemType
{
    Consumable,
    Goggles,
    Boots,
    Cloak
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Backrooms/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;

    [Header("Item Type")]
    public InventoryItemType itemType;

    [Min(1)]
    public int maxStack = 1;

    [Header("Item Effects")]
    [Range(0.1f, 2f)]
    public float staminaDrainMultiplier = 1f;

    [Min(1f)]
    public float speedMultiplier = 1f;

    [Min(1f)]
    public float brightnessMultiplier = 1f;

    [Min(0f)]
    public float effectDuration = 0f;
}
