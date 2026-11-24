
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioVolumeSlider : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;
    public TMP_Text percentText;

    [Header("Audio Sources")]
    public AudioSource[] audioSources;

    [Header("Save Settings")]
    public string preferenceKey = "MusicVolume";

    private float[] originalVolumes;

    void Start()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponent<Slider>();

        if (volumeSlider == null)
        {
            Debug.LogError("Volume Slider is missing!");
            return;
        }

        // Remember original volumes
        originalVolumes = new float[audioSources.Length];

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
                originalVolumes[i] = audioSources[i].volume;
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        float savedVolume = PlayerPrefs.GetFloat(
            preferenceKey, 1f
        );

        volumeSlider.SetValueWithoutNotify(savedVolume);

        ApplyVolume(savedVolume);

        volumeSlider.onValueChanged.AddListener(ApplyVolume);
    }

    public void ApplyVolume(float value)
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                audioSources[i].volume =
                    originalVolumes[i] * value;
            }
        }

        if (percentText != null)
        {
            percentText.text =
                Mathf.RoundToInt(value * 100f) + "%";
        }

        PlayerPrefs.SetFloat(preferenceKey, value);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
    }
}
