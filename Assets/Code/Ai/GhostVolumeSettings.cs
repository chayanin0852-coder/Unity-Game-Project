
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GhostVolumeSettings : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text volumeText;

    [Header("Ghost Audio")]
    [SerializeField] private GhostAudioController ghostAudioController;

    private Slider volumeSlider;

    private const string preferenceKey = "GhostVolume";

    private void Start()
    {
        volumeSlider = GetComponent<Slider>();

        if (volumeSlider == null)
        {
            Debug.LogError("GhostVolumeSettings: ‰¡Ëæ∫ Slider");
            return;
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        float savedVolume = PlayerPrefs.GetFloat(preferenceKey, 1f);

        volumeSlider.SetValueWithoutNotify(savedVolume);

        SetVolume(savedVolume);

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        if (ghostAudioController != null)
        {
            ghostAudioController.SetGhostVolume(value);
        }

        if (volumeText != null)
        {
            volumeText.text =
                Mathf.RoundToInt(value * 100f) + "%";
        }

        PlayerPrefs.SetFloat(preferenceKey, value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
        }
    }
}
