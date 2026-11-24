using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public PlayerMove player;
    public Slider slider;
    public Image fill;

    void Update()
    {
        float value = player.stamina / player.maxStamina;
        slider.value = value;

        // เปลี่ยนสีตามพลังงาน
        if (value > 0.5f)
        {
            fill.color = Color.green;
        }
        else if (value > 0.2f)
        {
            fill.color = Color.yellow;
        }
        else
        {
            fill.color = Color.red;
        }
    }
}