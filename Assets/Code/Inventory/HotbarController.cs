
using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [Header("Hotbar Slots")]
    [SerializeField] private GameObject[] hotbarSlots = new GameObject[3];

    [Header("Player Reference")]
    [SerializeField] private PlayerMove playerMove;

    [Header("Equipment Slots")]
    [SerializeField] private InventorySlot gogglesEquipmentSlot;
    [SerializeField] private InventorySlot bootsEquipmentSlot;

    private int selectedIndex = 0;

    public int SelectedIndex => selectedIndex;

    private void Start()
    {
        if (playerMove == null)
        {
            playerMove = FindFirstObjectByType<PlayerMove>();
        }

        UpdateSelection();
    }

    private void Update()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            SelectNext();
        }
        else if (scroll < 0f)
        {
            SelectPrevious();
        }

        if (Cursor.lockState == CursorLockMode.Locked &&
            !Cursor.visible &&
            Input.GetMouseButtonDown(1))
        {
            UseSelectedItem();
        }
    }

    private void SelectNext()
    {
        selectedIndex++;

        if (selectedIndex >= hotbarSlots.Length)
            selectedIndex = 0;

        UpdateSelection();
    }

    private void SelectPrevious()
    {
        selectedIndex--;

        if (selectedIndex < 0)
            selectedIndex = hotbarSlots.Length - 1;

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (hotbarSlots == null)
            return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
                continue;

            Outline outline = hotbarSlots[i].GetComponent<Outline>();

            if (outline != null)
                outline.enabled = (i == selectedIndex);
        }
    }

    private void UseSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= hotbarSlots.Length)
            return;

        GameObject selectedObject = hotbarSlots[selectedIndex];

        if (selectedObject == null)
            return;

        InventorySlot selectedSlot =
            selectedObject.GetComponent<InventorySlot>();

        if (selectedSlot == null || selectedSlot.IsEmpty)
            return;

        ItemData item = selectedSlot.CurrentItem;

        if (item == null)
            return;

        switch (item.itemType)
        {
            case InventoryItemType.Consumable:
                UseWater(selectedSlot, item);
                break;

            case InventoryItemType.Cloak:
                UseCloak(selectedSlot, item);
                break;

            case InventoryItemType.Goggles:
                EquipItem(
                    selectedSlot,
                    gogglesEquipmentSlot,
                    "แว่นตา"
                );
                break;

            case InventoryItemType.Boots:
                EquipItem(
                    selectedSlot,
                    bootsEquipmentSlot,
                    "รองเท้า"
                );
                break;
        }
    }

    private void UseWater(InventorySlot slot, ItemData item)
    {
        if (playerMove == null)
            playerMove = FindFirstObjectByType<PlayerMove>();

        if (playerMove == null)
        {
            Debug.LogError("ไม่พบ PlayerMove!");
            return;
        }

        playerMove.ApplyStaminaDrainEffect(
            item.staminaDrainMultiplier,
            item.effectDuration
        );

        slot.RemoveItem();

        Debug.Log("ใช้ไอเทมน้ำแล้ว");
    }

    private void UseCloak(InventorySlot slot, ItemData item)
    {
        if (playerMove == null)
            playerMove = FindFirstObjectByType<PlayerMove>();

        if (playerMove == null)
        {
            Debug.LogError("ไม่พบ PlayerMove!");
            return;
        }

        playerMove.ApplyCloakEffect(item.effectDuration);

        slot.RemoveItem();

        Debug.Log("ใช้ผ้าคลุมแล้ว");
    }

    private void EquipItem(
        InventorySlot sourceSlot,
        InventorySlot equipmentSlot,
        string itemName)
    {
        if (equipmentSlot == null)
        {
            Debug.LogError("ยังไม่ได้กำหนดช่อง Equipment: " + itemName);
            return;
        }

        if (sourceSlot.TryMoveTo(equipmentSlot))
        {
            Debug.Log("สวมใส่" + itemName + "เรียบร้อยแล้ว");
        }
        else
        {
            Debug.LogWarning("ไม่สามารถสวมใส่" + itemName + "ได้");
        }
    }
}
