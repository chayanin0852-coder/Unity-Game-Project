
using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public CharacterController controller;
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float gravity = -9.8f;
    private float yVelocity = 0f;
    public Animator animator;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float stamina;
    public float staminaDrain = 1f;
    public float staminaRegen = 0.5f;

    private float activeStaminaDrainMultiplier = 1f;
    private Coroutine staminaEffectCoroutine;

    [Header("Equipment Slots")]
    [SerializeField] private InventorySlot gogglesSlot;
    [SerializeField] private InventorySlot bootsSlot;

    [Header("Goggles Light")]
    [SerializeField] private Light gogglesLight;
    [SerializeField] private float gogglesBaseIntensity = 2f;

    [Header("Goggles Fog Vision")]
    [SerializeField, Range(0f, 1f)]
    private float fogAmount = 0.6667f;

    [SerializeField, Min(1f)]
    private float gogglesFogDistanceMultiplier = 2f;

    private float originalFogStartDistance;
    private float originalFogEndDistance;
    private bool originalFogEnabled;
    private bool fogSettingsCaptured;

    [Header("Footstep Sound")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip footstepClip;

    [SerializeField] private float walkStepInterval = 0.55f;
    [SerializeField] private float runStepInterval = 0.32f;

    [Range(0f, 1f)]
    [SerializeField] private float walkFootstepVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float runFootstepVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float bootsVolumeMultiplier = 0.35f;

    [Header("Ghost Hearing")]
    [SerializeField] private float walkNoiseRadius = 5f;
    [SerializeField] private float runNoiseRadius = 12f;

    [Range(0f, 1f)]
    [SerializeField] private float bootsNoiseMultiplier = 0.4f;

    public float CurrentNoiseRadius { get; private set; }

    private float footstepTimer;

    [Header("Cloak Effect")]
    private Renderer[] cloakRenderers;
    private bool[] cloakOriginalRendererStates;
    private Coroutine cloakEffectCoroutine;

    private bool isCloaked;

    public bool IsCloaked => isCloaked;

    private void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        stamina = maxStamina;

        CaptureFogSettings();

        if (gogglesLight != null)
            gogglesLight.enabled = false;
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        bool isMoving = move.magnitude > 0.01f;
        bool isRunning = isMoving &&
                         Input.GetKey(KeyCode.LeftShift) &&
                         stamina > 0f;

        ItemData boots = GetEquippedBoots();
        ItemData goggles = GetEquippedGoggles();

        float bootsSpeedMultiplier = boots != null
            ? Mathf.Max(1f, boots.speedMultiplier)
            : 1f;

        float speed = walkSpeed;
        float animSpeed = 0f;

        if (isRunning)
        {
            speed = runSpeed * bootsSpeedMultiplier;
            animSpeed = 1f;

            stamina -= staminaDrain *
                       activeStaminaDrainMultiplier *
                       Time.deltaTime;
        }
        else if (isMoving)
        {
            animSpeed = 0.3f;
        }

        if (!isRunning)
            stamina += staminaRegen * Time.deltaTime;

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        // Gravity and movement.
        if (controller != null)
        {
            if (controller.isGrounded && yVelocity < 0f)
                yVelocity = -2f;

            yVelocity += gravity * Time.deltaTime;
            move.y = yVelocity;

            controller.Move(move * speed * Time.deltaTime);
        }

        if (animator != null)
            animator.SetFloat("Speed", animSpeed);

        UpdateGoggles(goggles);
        UpdateFootsteps(isMoving, isRunning, boots);
    }

    private ItemData GetEquippedBoots()
    {
        if (bootsSlot == null)
            return null;

        ItemData item = bootsSlot.CurrentItem;

        if (item == null || item.itemType != InventoryItemType.Boots)
            return null;

        return item;
    }

    private ItemData GetEquippedGoggles()
    {
        if (gogglesSlot == null)
            return null;

        ItemData item = gogglesSlot.CurrentItem;

        if (item == null || item.itemType != InventoryItemType.Goggles)
            return null;

        return item;
    }

    // Goggles: enable the light and reduce fog while equipped.
    private void UpdateGoggles(ItemData goggles)
    {
        bool equipped = goggles != null;

        if (gogglesLight != null)
        {
            gogglesLight.enabled = equipped;

            if (equipped)
            {
                gogglesLight.intensity =
                    gogglesBaseIntensity *
                    Mathf.Max(0.1f, goggles.brightnessMultiplier);
            }
        }

        ApplyFogSettings(equipped);
    }

    private void CaptureFogSettings()
    {
        originalFogEnabled = RenderSettings.fog;
        originalFogStartDistance = RenderSettings.fogStartDistance;
        originalFogEndDistance = RenderSettings.fogEndDistance;

        fogSettingsCaptured = true;

        if (!originalFogEnabled)
        {
            Debug.LogWarning(
                "Fog is disabled in Lighting Settings. " +
                "Enable Fog and use Linear mode."
            );
        }
    }

    // Called by the future Settings menu Slider.
    public void SetFogAmount(float amount)
    {
        fogAmount = Mathf.Clamp01(amount);

        ApplyFogSettings(GetEquippedGoggles() != null);
    }

    public float FogAmount => fogAmount;

    private void ApplyFogSettings(bool gogglesEquipped)
    {
        if (!fogSettingsCaptured)
            return;

        // 0% means no fog.
        if (fogAmount <= 0.001f)
        {
            RenderSettings.fog = false;
            return;
        }

        // Restore the scene's original fog enabled state.
        RenderSettings.fog = originalFogEnabled;

        if (!originalFogEnabled)
            return;

        // Lower slider value = lighter fog.
        // Higher slider value = denser fog.
        // Around 66.67% preserves the current scene fog.
        float distanceMultiplier = Mathf.Lerp(
            2f,
            0.5f,
            fogAmount
        );

        // Goggles increase fog distances, improving visibility.
        if (gogglesEquipped)
        {
            distanceMultiplier *=
                Mathf.Max(1f, gogglesFogDistanceMultiplier);
        }

        RenderSettings.fogStartDistance =
            originalFogStartDistance * distanceMultiplier;

        RenderSettings.fogEndDistance = Mathf.Max(
            RenderSettings.fogStartDistance + 0.1f,
            originalFogEndDistance * distanceMultiplier
        );
    }

    // Boots: quieter footsteps and smaller hearing radius.
    private void UpdateFootsteps(
        bool isMoving,
        bool isRunning,
        ItemData boots)
    {
        if (!isMoving || controller == null || !controller.isGrounded)
        {
            CurrentNoiseRadius = 0f;
            footstepTimer = 0f;
            return;
        }

        bool hasBoots = boots != null;

        float noiseRadius = isRunning
            ? runNoiseRadius
            : walkNoiseRadius;

        // Calculate the distance that ghosts can hear.
        CurrentNoiseRadius = isCloaked
            ? 0f
            : (hasBoots
                ? noiseRadius * bootsNoiseMultiplier
                : noiseRadius);

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            // Play the actual footstep audio if configured.
            if (footstepAudioSource != null && footstepClip != null)
            {
                float volume = isRunning
                    ? runFootstepVolume
                    : walkFootstepVolume;

                if (hasBoots)
                    volume *= bootsVolumeMultiplier;

                footstepAudioSource.PlayOneShot(
                    footstepClip,
                    volume
                );
            }

            // Send the footstep position to the ghost AI.
            // This works even before actual audio clips are assigned.
            if (!isCloaked && CurrentNoiseRadius > 0f)
            {
                GhostNoiseSystem.EmitNoise(
                    transform.position,
                    CurrentNoiseRadius
                );
            }

            // Set the next footstep interval.
            footstepTimer = isRunning
                ? runStepInterval
                : walkStepInterval;
        }
    }

    // Water effect.
    public bool ApplyStaminaDrainEffect(float multiplier, float duration)
    {
        if (duration <= 0f)
            return false;

        if (staminaEffectCoroutine != null)
            StopCoroutine(staminaEffectCoroutine);

        staminaEffectCoroutine = StartCoroutine(
            StaminaDrainEffectRoutine(
                Mathf.Max(0.01f, multiplier),
                duration
            )
        );

        return true;
    }

    private IEnumerator StaminaDrainEffectRoutine(
        float multiplier,
        float duration)
    {
        activeStaminaDrainMultiplier = multiplier;

        yield return new WaitForSeconds(duration);

        activeStaminaDrainMultiplier = 1f;
        staminaEffectCoroutine = null;

        Debug.Log("Stamina item effect ended.");
    }

    // Cloak effect.
    public bool ApplyCloakEffect(float duration)
    {
        if (duration <= 0f)
            return false;

        if (cloakEffectCoroutine != null)
        {
            StopCoroutine(cloakEffectCoroutine);
            RestoreCloakRenderers();
        }

        cloakRenderers = GetComponentsInChildren<Renderer>(true);

        if (cloakRenderers.Length == 0)
        {
            cloakRenderers = null;
            return false;
        }

        cloakOriginalRendererStates =
            new bool[cloakRenderers.Length];

        // Activate camouflage for ghost AI.
        isCloaked = true;
        CurrentNoiseRadius = 0f;

        for (int i = 0; i < cloakRenderers.Length; i++)
        {
            if (cloakRenderers[i] == null)
                continue;

            cloakOriginalRendererStates[i] =
                cloakRenderers[i].enabled;

            cloakRenderers[i].enabled = false;
        }

        cloakEffectCoroutine =
            StartCoroutine(CloakEffectRoutine(duration));

        return true;
    }

    private IEnumerator CloakEffectRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        RestoreCloakRenderers();
        cloakEffectCoroutine = null;
    }

    private void RestoreCloakRenderers()
    {
        if (cloakRenderers != null &&
            cloakOriginalRendererStates != null)
        {
            int count = Mathf.Min(
                cloakRenderers.Length,
                cloakOriginalRendererStates.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (cloakRenderers[i] != null)
                {
                    cloakRenderers[i].enabled =
                        cloakOriginalRendererStates[i];
                }
            }
        }

        cloakRenderers = null;
        cloakOriginalRendererStates = null;

        // Camouflage has ended.
        isCloaked = false;
        CurrentNoiseRadius = 0f;
    }

    private void OnDisable()
    {
        if (cloakEffectCoroutine != null)
        {
            StopCoroutine(cloakEffectCoroutine);
            cloakEffectCoroutine = null;
        }

        if (staminaEffectCoroutine != null)
        {
            StopCoroutine(staminaEffectCoroutine);
            staminaEffectCoroutine = null;
        }

        if (gogglesLight != null)
            gogglesLight.enabled = false;

        // Restore original scene fog when this component is disabled.
        if (fogSettingsCaptured)
        {
            RenderSettings.fog = originalFogEnabled;
            RenderSettings.fogStartDistance = originalFogStartDistance;
            RenderSettings.fogEndDistance = originalFogEndDistance;
        }

        RestoreCloakRenderers();
    }
}
