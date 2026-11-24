
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int keyCount = 0;
    public int maxKeys = 3;

    public Door door;

    [Header("Key Counter UI")]
    public Text keyCountText;

    private void Awake()
    {
        instance = this;
        UpdateKeyCountUI();
    }

    public void AddKey()
    {
        keyCount++;

        UpdateKeyCountUI();

        Debug.Log("‡°Á∫°ÿ≠·®: " + keyCount);

        if (keyCount >= maxKeys)
        {
            Debug.Log("‡ª‘¥ª√–µŸ!");

            if (door != null)
            {
                door.OpenDoor();
            }
            else
            {
                Debug.LogError("‰¡Ë‰¥È°”Àπ¥ Door „π GameManager!");
            }
        }
    }

    private void UpdateKeyCountUI()
    {
        if (keyCountText == null)
            return;

        int displayCount = Mathf.Min(keyCount, maxKeys);

        keyCountText.text = "°ÿ≠·® " + displayCount + "/" + maxKeys;
    }
}
