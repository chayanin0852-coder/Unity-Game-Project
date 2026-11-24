
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuAudioSettings : MonoBehaviour
{
    [Header("Ghost Volume")]
    [SerializeField] private Slider ghostVolumeSlider;
    [SerializeField] private TMP_Text ghostVolumeValue;

    [Header("Background Volume")]
    [SerializeField] private Slider backgroundVolumeSlider;
    [SerializeField] private TMP_Text backgroundVolumeValue;

    [Header("SFX Volume")]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeValue;

    private const string GhostKey = "GhostVolume";
    private const string BackgroundKey = "BackgroundVolume";
    private const string SFXKey = "SFXVolume";

    private void Start()
    {
        SetupSlider(
            ghostVolumeSlider,
            ghostVolumeValue,
            GhostKey
        );

        SetupSlider(
            backgroundVolumeSlider,
            backgroundVolumeValue,
            BackgroundKey
        );

        SetupSlider(
            sfxVolumeSlider,
            sfxVolumeValue,
            SFXKey
        );

        ghostVolumeSlider?.onValueChanged.AddListener(
            SetGhostVolume
        );

        backgroundVolumeSlider?.onValueChanged.AddListener(
            SetBackgroundVolume
        );

        sfxVolumeSlider?.onValueChanged.AddListener(
            SetSFXVolume
        );
    }

    private void SetupSlider(
        Slider slider,
        TMP_Text valueText,
        string preferenceKey
    )
    {
        if (slider == null)
        {
            Debug.LogError(
                "MainMenuAudioSettings: ไม่ได้เชื่อม Slider " +
                preferenceKey,
                this
            );
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;

        float savedValue = PlayerPrefs.GetFloat(
            preferenceKey,
            1f
        );

        savedValue = Mathf.Clamp01(savedValue);

        slider.SetValueWithoutNotify(savedValue);

        UpdateValueText(valueText, savedValue);
    }

    private void SetGhostVolume(float value)
    {
        SaveVolume(GhostKey, value, ghostVolumeValue);
    }

    private void SetBackgroundVolume(float value)
    {
        SaveVolume(
            BackgroundKey,
            value,
            backgroundVolumeValue
        );
    }

    private void SetSFXVolume(float value)
    {
        SaveVolume(SFXKey, value, sfxVolumeValue);
    }

    private void SaveVolume(
        string key,
        float value,
        TMP_Text valueText
    )
    {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();

        UpdateValueText(valueText, value);
    }

    private void UpdateValueText(
        TMP_Text valueText,
        float value
    )
    {
        if (valueText != null)
        {
            valueText.text =
                Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void OnDestroy()
    {
        if (ghostVolumeSlider != null)
        {
            ghostVolumeSlider.onValueChanged.RemoveListener(
                SetGhostVolume
            );
        }

        if (backgroundVolumeSlider != null)
        {
            backgroundVolumeSlider.onValueChanged.RemoveListener(
                SetBackgroundVolume
            );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(
                SetSFXVolume
            );
        }
    }
}
