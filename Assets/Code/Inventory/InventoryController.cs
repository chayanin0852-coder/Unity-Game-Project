
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Storage Slots")]
    [SerializeField] private InventorySlot[] storageSlots = new InventorySlot[9];

    [Header("Mouse Look")]
    [SerializeField] private MonoBehaviour mouseLookBehaviour;

    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Start()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("ยังไม่ได้กำหนด InventoryPanel!");
            return;
        }

        inventoryPanel.SetActive(false);
        isOpen = false;

        UpdateCursorState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        UpdateCursorState();
    }

    private void UpdateCursorState()
    {
        Cursor.visible = isOpen;

        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        if (mouseLookBehaviour != null)
        {
            mouseLookBehaviour.enabled = !isOpen;
        }
    }

    public bool TryAddItem(ItemData item)
    {
        if (item == null)
            return false;

        foreach (InventorySlot slot in storageSlots)
        {
            if (slot == null)
                continue;

            if (slot.SlotType != InventorySlotType.Storage)
                continue;

            if (!slot.IsEmpty)
                continue;

            if (slot.TrySetItem(item))
            {
                Debug.Log("เก็บไอเท็มสำเร็จ: " + item.itemName);
                return true;
            }
        }

        Debug.LogWarning("ช่องเก็บของเต็ม!");
        return false;
    }
}
