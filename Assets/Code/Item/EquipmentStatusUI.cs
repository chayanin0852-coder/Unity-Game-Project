
using UnityEngine;
using UnityEngine.UI;

public class EquipmentStatusUI : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private InventorySlot gogglesSlot;
    [SerializeField] private InventorySlot bootsSlot;

    [Header("Goggles Display")]
    [SerializeField] private Image gogglesIcon;
    [SerializeField] private Text gogglesTimerText;

    [Header("Boots Display")]
    [SerializeField] private Image bootsIcon;
    [SerializeField] private Text bootsTimerText;

    private void Start()
    {
        HideDisplay(gogglesIcon, gogglesTimerText);
        HideDisplay(bootsIcon, bootsTimerText);
    }

    private void Update()
    {
        UpdateEquipment(
            gogglesSlot,
            gogglesIcon,
            gogglesTimerText
        );

        UpdateEquipment(
            bootsSlot,
            bootsIcon,
            bootsTimerText
        );
    }

    private void UpdateEquipment(
        InventorySlot slot,
        Image itemIcon,
        Text timerText)
    {
        if (slot == null)
            return;

        ItemData item = slot.CurrentItem;

        if (item == null)
        {
            HideDisplay(itemIcon, timerText);
            return;
        }

        float duration = item.effectDuration > 0f
            ? item.effectDuration
            : 120f;

        float remaining = slot.GetOrStartEquipmentTimer(duration);

        // Countdown only while equipped.
        remaining -= Time.deltaTime;

        slot.SetRemainingEquipmentTime(remaining);

        if (remaining <= 0f)
        {
            Debug.Log(item.itemName + " หมดเวลาแล้ว");

            slot.RemoveItem();

            HideDisplay(itemIcon, timerText);
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = item.icon != null;
            itemIcon.preserveAspect = true;
        }

        if (timerText != null)
        {
            timerText.enabled = true;
            timerText.text = FormatTime(remaining);
        }
    }

    private string FormatTime(float time)
    {
        int totalSeconds = Mathf.CeilToInt(time);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return minutes.ToString("00") + ":" +
               seconds.ToString("00");
    }

    private void HideDisplay(Image icon, Text timer)
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (timer != null)
        {
            timer.text = "";
            timer.enabled = false;
        }
    }
}
