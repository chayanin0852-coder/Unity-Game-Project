
using UnityEngine;
using UnityEngine.UI;

public enum InventorySlotType
{
    Storage,
    Hotbar,
    GogglesEquipment,
    BootsEquipment
}

public class InventorySlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private InventorySlotType slotType;

    [Header("UI References")]
    [SerializeField] private Image itemIcon;

    [Header("Item Information")]
    [SerializeField] private ItemData currentItem;

    [Header("Equipment Timer")]
    [SerializeField, HideInInspector] private float remainingEquipmentTime;
    [SerializeField, HideInInspector] private bool equipmentTimerInitialized;

    public InventorySlotType SlotType => slotType;
    public ItemData CurrentItem => currentItem;
    public bool IsEmpty => currentItem == null;

    public float RemainingEquipmentTime => remainingEquipmentTime;

    private void Awake()
    {
        UpdateUI();
    }

    public bool CanAcceptItem(ItemData item)
    {
        if (item == null)
            return false;

        switch (slotType)
        {
            case InventorySlotType.Storage:
                return true;

            case InventorySlotType.Hotbar:
                return item.itemType == InventoryItemType.Consumable
                    || item.itemType == InventoryItemType.Cloak
                    || item.itemType == InventoryItemType.Goggles
                    || item.itemType == InventoryItemType.Boots;

            case InventorySlotType.GogglesEquipment:
                return item.itemType == InventoryItemType.Goggles;

            case InventorySlotType.BootsEquipment:
                return item.itemType == InventoryItemType.Boots;

            default:
                return false;
        }
    }

    public bool TrySetItem(ItemData item)
    {
        if (!IsEmpty || !CanAcceptItem(item))
            return false;

        currentItem = item;

        remainingEquipmentTime = 0f;
        equipmentTimerInitialized = false;

        UpdateUI();

        return true;
    }

    public ItemData RemoveItem()
    {
        if (IsEmpty)
            return null;

        ItemData removedItem = currentItem;

        currentItem = null;
        remainingEquipmentTime = 0f;
        equipmentTimerInitialized = false;

        UpdateUI();

        return removedItem;
    }

    public bool TryMoveTo(InventorySlot target)
    {
        if (target == null || target == this || IsEmpty)
            return false;

        if (!target.CanAcceptItem(currentItem))
            return false;

        if (!target.IsEmpty &&
            !CanAcceptItem(target.currentItem))
            return false;

        ItemData tempItem = currentItem;
        float tempTime = remainingEquipmentTime;
        bool tempTimerInitialized = equipmentTimerInitialized;

        currentItem = target.currentItem;
        remainingEquipmentTime = target.remainingEquipmentTime;
        equipmentTimerInitialized = target.equipmentTimerInitialized;

        target.currentItem = tempItem;
        target.remainingEquipmentTime = tempTime;
        target.equipmentTimerInitialized = tempTimerInitialized;

        UpdateUI();
        target.UpdateUI();

        return true;
    }

    public float GetOrStartEquipmentTimer(float duration)
    {
        if (currentItem == null)
            return 0f;

        if (!equipmentTimerInitialized)
        {
            remainingEquipmentTime = Mathf.Max(0f, duration);
            equipmentTimerInitialized = true;
        }

        return remainingEquipmentTime;
    }

    public void SetRemainingEquipmentTime(float time)
    {
        if (currentItem == null)
            return;

        remainingEquipmentTime = Mathf.Max(0f, time);
        equipmentTimerInitialized = true;
    }

    private void UpdateUI()
    {
        if (itemIcon == null)
            return;

        if (currentItem == null || currentItem.icon == null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
        else
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.enabled = true;
            itemIcon.preserveAspect = true;
        }
    }
}
