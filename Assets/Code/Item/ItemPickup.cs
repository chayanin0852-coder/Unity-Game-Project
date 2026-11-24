
using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Information")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private InventoryController inventory;

    [Header("Pickup Settings")]
    [SerializeField] private float holdDuration = 2f;

    [Header("Pickup UI")]
    [SerializeField] private GameObject itemPrompt;
    [SerializeField] private Slider itemProgress;

    [Header("Pickup Sound")]
    [SerializeField] private AudioClip pickupSound;

    [Range(0f, 1f)]
    [SerializeField] private float pickupSoundVolume = 0.8f;

    private bool playerInRange;
    private bool isCollected;
    private float holdTimer;

    private void Start()
    {
        if (itemPrompt != null)
            itemPrompt.SetActive(false);

        if (itemProgress != null)
        {
            itemProgress.minValue = 0f;
            itemProgress.maxValue = 1f;
            itemProgress.value = 0f;
            itemProgress.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isCollected || !playerInRange)
            return;

        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            if (itemProgress != null)
            {
                float safeDuration = Mathf.Max(holdDuration, 0.01f);

                itemProgress.value = Mathf.Clamp01(
                    holdTimer / safeDuration
                );
            }

            if (holdTimer >= holdDuration)
                TryCollectItem();
        }
        else
        {
            ResetProgress();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        ShowPickupUI(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        ResetProgress();
        ShowPickupUI(false);
    }

    private void TryCollectItem()
    {
        if (inventory == null || itemData == null)
        {
            Debug.LogError(
                "ยังไม่ได้กำหนด Inventory หรือ ItemData!",
                this
            );

            ResetProgress();
            return;
        }

        if (inventory.TryAddItem(itemData))
        {
            isCollected = true;

            // Play sound before destroying the item.
            // PlayClipAtPoint creates its own temporary AudioSource,
            // so the sound continues after this GameObject is destroyed.
            if (pickupSound != null && pickupSoundVolume > 0f)
            {
                AudioSource.PlayClipAtPoint(
                    pickupSound,
                    transform.position,
                    pickupSoundVolume
                );
            }

            ShowPickupUI(false);

            Debug.Log(
                "เก็บไอเทมสำเร็จ: " + itemData.itemName,
                this
            );

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning(
                "Inventory เต็ม ไม่สามารถเก็บไอเทมได้!",
                this
            );

            ResetProgress();
        }
    }

    private void ResetProgress()
    {
        holdTimer = 0f;

        if (itemProgress != null)
            itemProgress.value = 0f;
    }

    private void ShowPickupUI(bool show)
    {
        if (itemPrompt != null)
            itemPrompt.SetActive(show);

        if (itemProgress != null)
            itemProgress.gameObject.SetActive(show);
    }
}
