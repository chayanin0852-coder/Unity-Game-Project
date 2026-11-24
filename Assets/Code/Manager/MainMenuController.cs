
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuGroup;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Game Scene")]
    [SerializeField] private string gameplaySceneName = "Level1";

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (mainMenuGroup != null)
        {
            mainMenuGroup.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuGroup != null)
        {
            mainMenuGroup.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void BackToMenu()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (mainMenuGroup != null)
        {
            mainMenuGroup.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("ออกจากเกม");

        Application.Quit();
    }
}
