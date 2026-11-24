
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Level1AudioSettings : MonoBehaviour
{
    [Header("Ghost Volume")]
    [SerializeField] private Slider ghostSlider;
    [SerializeField] private TMP_Text ghostValueText;

    [Header("Background Volume")]
    [SerializeField] private Slider backgroundSlider;
    [SerializeField] private TMP_Text backgroundValueText;

    [Header("SFX Volume")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValueText;

    private const string GhostKey = "GhostVolume";
    private const string BackgroundKey = "BackgroundVolume";
    private const string SFXKey = "SFXVolume";

    private float ghostVolume = 1f;
    private float backgroundVolume = 1f;
    private float sfxVolume = 1f;

    private float nextScanTime = 0f;

    private readonly Dictionary<AudioSource, float> originalVolumes =
        new Dictionary<AudioSource, float>();

    private void Start()
    {
        // Load saved settings from Main Menu.
        ghostVolume = PlayerPrefs.GetFloat(GhostKey, 1f);
        backgroundVolume = PlayerPrefs.GetFloat(BackgroundKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFXKey, 1f);

        // Connect UI sliders.
        SetupSlider(ghostSlider, ghostVolume, ghostValueText);
        SetupSlider(backgroundSlider, backgroundVolume, backgroundValueText);
        SetupSlider(sfxSlider, sfxVolume, sfxValueText);

        // Apply saved ghost volume.
        ApplyGhostVolume();

        // Find and control all audio sources.
        RefreshAudioSources();

        // Listen for slider changes.
        if (ghostSlider != null)
            ghostSlider.onValueChanged.AddListener(SetGhostVolume);

        if (backgroundSlider != null)
            backgroundSlider.onValueChanged.AddListener(SetBackgroundVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void Update()
    {
        // Periodically detect newly created audio sources,
        // including temporary One Shot Audio sources.
        if (Time.unscaledTime >= nextScanTime)
        {
            nextScanTime = Time.unscaledTime + 0.2f;
            RefreshAudioSources();
        }
    }

    private void SetupSlider(
        Slider slider,
        float savedValue,
        TMP_Text valueText)
    {
        if (slider == null)
        {
            Debug.LogError(
                "Level1AudioSettings: Missing audio slider.",
                this
            );
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;

        savedValue = Mathf.Clamp01(savedValue);

        slider.SetValueWithoutNotify(savedValue);

        UpdateValueText(valueText, savedValue);
    }

    public void SetGhostVolume(float value)
    {
        ghostVolume = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(GhostKey, ghostVolume);
        PlayerPrefs.Save();

        UpdateValueText(ghostValueText, ghostVolume);

        ApplyGhostVolume();
    }

    public void SetBackgroundVolume(float value)
    {
        backgroundVolume = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(BackgroundKey, backgroundVolume);
        PlayerPrefs.Save();

        UpdateValueText(backgroundValueText, backgroundVolume);

        RefreshAudioSources();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(SFXKey, sfxVolume);
        PlayerPrefs.Save();

        UpdateValueText(sfxValueText, sfxVolume);

        RefreshAudioSources();
    }

    private void ApplyGhostVolume()
    {
        GhostAudioController[] ghosts =
            FindObjectsByType<GhostAudioController>(
                FindObjectsSortMode.None
            );

        foreach (GhostAudioController ghost in ghosts)
        {
            if (ghost != null)
                ghost.SetGhostVolume(ghostVolume);
        }
    }

    private void RefreshAudioSources()
    {
        AudioSource[] sources =
            FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;

            // Ghost sounds use GhostAudioController separately.
            if (IsGhostSource(source))
                continue;

            // Remember the original volume only once.
            if (!originalVolumes.ContainsKey(source))
            {
                originalVolumes.Add(source, source.volume);
            }

            float originalVolume = originalVolumes[source];

            // Background and all other sounds have separate controls.
            if (IsBackgroundSource(source))
            {
                source.volume = originalVolume * backgroundVolume;
            }
            else
            {
                source.volume = originalVolume * sfxVolume;
            }
        }

        // Remove destroyed audio sources from the dictionary.
        List<AudioSource> removedSources =
            new List<AudioSource>();

        foreach (KeyValuePair<AudioSource, float> pair in originalVolumes)
        {
            if (pair.Key == null)
                removedSources.Add(pair.Key);
        }

        foreach (AudioSource source in removedSources)
        {
            originalVolumes.Remove(source);
        }
    }

    private bool IsGhostSource(AudioSource source)
    {
        if (source.GetComponentInParent<GhostAudioController>() != null)
            return true;

        Transform current = source.transform;

        while (current != null)
        {
            if (current.name.ToLowerInvariant().Contains("ghost"))
                return true;

            current = current.parent;
        }

        return false;
    }

    private bool IsBackgroundSource(AudioSource source)
    {
        Transform current = source.transform;

        while (current != null)
        {
            string objectName = current.name.ToLowerInvariant();

            if (objectName.Contains("ambient") ||
                objectName.Contains("background") ||
                objectName.Contains("music"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void UpdateValueText(TMP_Text valueText, float value)
    {
        if (valueText != null)
        {
            valueText.text =
                Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void OnDestroy()
    {
        if (ghostSlider != null)
            ghostSlider.onValueChanged.RemoveListener(SetGhostVolume);

        if (backgroundSlider != null)
            backgroundSlider.onValueChanged.RemoveListener(SetBackgroundVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}
