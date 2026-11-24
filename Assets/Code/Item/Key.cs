
using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    [Header("Key Collection")]
    [SerializeField] private float holdDuration = 5f;
    [SerializeField] private Slider keyProgressBar;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("Pickup Sound")]
    [SerializeField] private AudioClip pickupSound;

    [Range(0f, 1f)]
    [SerializeField] private float pickupSoundVolume = 0.8f;

    private float holdTimer = 0f;
    private bool playerInRange = false;
    private bool isCollected = false;

    private static Key activeKey;

    private void Start()
    {
        holdDuration = Mathf.Max(0.01f, holdDuration);

        if (keyProgressBar != null)
        {
            keyProgressBar.minValue = 0f;
            keyProgressBar.maxValue = 1f;
            keyProgressBar.value = 0f;
            keyProgressBar.gameObject.SetActive(false);
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange || isCollected)
            return;

        if (activeKey == null && Input.GetKeyDown(KeyCode.E))
        {
            activeKey = this;
        }

        if (activeKey != this)
            return;

        if (!Input.GetKey(KeyCode.E))
        {
            CancelCollection();
            return;
        }

        holdTimer += Time.deltaTime;

        if (keyProgressBar != null)
        {
            keyProgressBar.gameObject.SetActive(true);
            keyProgressBar.value =
                Mathf.Clamp01(holdTimer / holdDuration);
        }

        if (holdTimer >= holdDuration)
        {
            CollectKey();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (activeKey == this)
            {
                CancelCollection();
            }
        }
    }

    private void CancelCollection()
    {
        holdTimer = 0f;

        if (activeKey == this)
        {
            activeKey = null;
        }

        if (keyProgressBar != null)
        {
            keyProgressBar.value = 0f;
            keyProgressBar.gameObject.SetActive(false);
        }
    }

    private void CollectKey()
    {
        if (isCollected)
            return;

        if (GameManager.instance == null)
        {
            Debug.LogError("ไม่พบ GameManager!");
            CancelCollection();
            return;
        }

        isCollected = true;
        activeKey = null;

        if (keyProgressBar != null)
        {
            keyProgressBar.value = 1f;
            keyProgressBar.gameObject.SetActive(false);
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Add key to the game manager.
        GameManager.instance.AddKey();

        // Play pickup sound before destroying the key.
        if (pickupSound != null && pickupSoundVolume > 0f)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position,
                pickupSoundVolume
            );
        }

        Debug.Log("เก็บกุญแจสำเร็จ!");

        Destroy(gameObject);
    }
}
