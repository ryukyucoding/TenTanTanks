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
        
        if (pausePanel == null)
        {
            Debug.LogError("[PauseManager] pausePanel 未設置！請在 Inspector 中賦值。");
            return;
        }
        
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        isPaused = true;
        
        // 暫停時顯示升級 UI
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradeUI();
            Debug.Log("[PauseManager] 遊戲暫停，顯示升級 UI");
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        
        if (pausePanel == null)
        {
            Debug.LogError("[PauseManager] pausePanel 未設置！請在 Inspector 中賦值。");
            return;
        }
        
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
        
        // 恢復時隱藏升級 UI（如果不是手動開啟的）
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradeUI();
            Debug.Log("[PauseManager] 遊戲恢復");
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;          // 確保恢復流程

        // 重置玩家數據（生命值和升級數據）
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetData();
            Debug.Log("[PauseManager] 返回主選單，已重置玩家數據");
        }

        SceneManager.LoadScene("Menu-new");
    }
}