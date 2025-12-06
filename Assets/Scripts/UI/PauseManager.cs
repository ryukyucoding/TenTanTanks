using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    bool isPaused;
    
    // 升級 UI 引用
    private UpgradeUI upgradeUI;

    void Awake()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // 尋找升級 UI
        upgradeUI = FindFirstObjectByType<UpgradeUI>();
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
        
        // 暫停時顯示升級 UI
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradeUI();
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
        
        // 恢復時隱藏升級 UI（如果不是手動開啟的）
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradeUI();
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;          // 確保恢復流程
        SceneManager.LoadScene("Menu");
    }
}