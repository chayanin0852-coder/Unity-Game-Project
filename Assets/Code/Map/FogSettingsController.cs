
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FogSettingsController : MonoBehaviour
{
    [Header("Fog Settings")]
    [SerializeField] private Slider fogSlider;
    [SerializeField] private TMP_Text fogValueText;

    private const string FogKey = "FogDensity";

    private void Start()
    {
        if (fogSlider == null)
        {
            Debug.LogError("Fog Slider is not assigned!");
            return;
        }

        fogSlider.minValue = 0f;
        fogSlider.maxValue = 1f;

        float savedValue = PlayerPrefs.GetFloat(FogKey, 0.6667f);

        fogSlider.SetValueWithoutNotify(savedValue);

        UpdateFogText(savedValue);

        fogSlider.onValueChanged.AddListener(SetFog);
    }

    public void SetFog(float value)
    {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(FogKey, value);
        PlayerPrefs.Save();

        UpdateFogText(value);
    }

    private void UpdateFogText(float value)
    {
        if (fogValueText != null)
        {
            fogValueText.text =
                Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void OnDestroy()
    {
        if (fogSlider != null)
            fogSlider.onValueChanged.RemoveListener(SetFog);
    }
}
