
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    [Header("Ending Panels")]
    [SerializeField] private GameObject diePanel;
    [SerializeField] private GameObject winPanel;

    [Header("Gameplay Scripts To Disable")]
    [SerializeField] private Behaviour[] scriptsToDisable;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool hasEnded = false;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (diePanel != null)
            diePanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void PlayerDied()
    {
        EndGame(diePanel);
    }

    public void PlayerWon()
    {
        EndGame(winPanel);
    }

    private void EndGame(GameObject panel)
    {
        if (hasEnded)
            return;

        if (panel == null)
        {
            Debug.LogError(
                "GameEndManager: ไม่ได้กำหนด Ending Panel",
                this
            );
            return;
        }

        hasEnded = true;

        if (diePanel != null)
            diePanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        // Show the correct ending screen.
        panel.SetActive(true);

        // Stop player, hotbar, inventory, ghost and other gameplay scripts.
        if (scriptsToDisable != null)
        {
            foreach (Behaviour script in scriptsToDisable)
            {
                if (script != null && script != this)
                {
                    script.enabled = false;
                }
            }
        }

        // Pause physics and gameplay time.
        Time.timeScale = 0f;

        // Show cursor for the MENU button.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Game End: " + panel.name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
