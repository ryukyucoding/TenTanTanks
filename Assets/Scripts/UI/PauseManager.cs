using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    bool isPaused;

    void Awake()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;          // 確保恢復流程
        SceneManager.LoadScene("Menu");
    }
}