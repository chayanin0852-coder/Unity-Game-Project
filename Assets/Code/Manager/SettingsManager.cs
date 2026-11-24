
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;

    [Header("Mouse Look")]
    public MouseLook mouseLookBehaviour;

    private bool isOpen = false;

    void Start()
    {
        OpenSettings(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenSettings(!isOpen);
        }
    }

    public void OpenSettings(bool open)
    {
        isOpen = open;

        if (settingsPanel != null)
            settingsPanel.SetActive(open);

        if (mouseLookBehaviour != null)
            mouseLookBehaviour.enabled = !open;

        Cursor.visible = open;

        Cursor.lockState = open
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Time.timeScale = open ? 0f : 1f;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
