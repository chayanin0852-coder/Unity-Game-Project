
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    private static InventorySlot draggedSlot;
    private static InventorySlotDragHandler activeHandler;

    private Canvas parentCanvas;
    private GameObject dragIconObject;
    private RectTransform dragIconRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventorySlot slot = GetComponent<InventorySlot>();

        if (slot == null || slot.IsEmpty)
            return;

        if (slot.CurrentItem.icon == null)
            return;

        draggedSlot = slot;
        activeHandler = this;

        parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
        {
            CreateDragIcon(slot.CurrentItem.icon);
            UpdateDragPosition(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeHandler != this)
            return;

        UpdateDragPosition(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null)
            return;

        InventorySlot targetSlot = GetComponent<InventorySlot>();

        if (targetSlot == null)
            return;

        if (targetSlot == draggedSlot)
            return;

        bool moved = draggedSlot.TryMoveTo(targetSlot);

        if (moved)
        {
            Debug.Log(
                "ย้ายไอเทม " +
                targetSlot.CurrentItem.itemName +
                " ไปยัง " + targetSlot.SlotType
            );
        }
        else
        {
            Debug.Log(
                "ไม่สามารถวางไอเทมในช่องนี้ได้"
            );
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (activeHandler != this)
            return;

        draggedSlot = null;
        activeHandler = null;

        if (dragIconObject != null)
        {
            Destroy(dragIconObject);
        }

        dragIconObject = null;
        dragIconRect = null;
    }

    private void CreateDragIcon(Sprite sprite)
    {
        dragIconObject = new GameObject(
            "InventoryDragIcon",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        dragIconObject.transform.SetParent(
            parentCanvas.transform,
            false
        );

        Image image = dragIconObject.GetComponent<Image>();

        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        dragIconRect =
            dragIconObject.GetComponent<RectTransform>();

        RectTransform sourceRect =
            GetComponent<RectTransform>();

        if (sourceRect != null)
        {
            dragIconRect.sizeDelta = sourceRect.rect.size;
        }
        else
        {
            dragIconRect.sizeDelta = new Vector2(64f, 64f);
        }

        dragIconObject.transform.SetAsLastSibling();
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        if (dragIconRect == null || parentCanvas == null)
            return;

        RectTransform canvasRect =
            parentCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            dragIconRect.localPosition = localPoint;
        }
    }
}
