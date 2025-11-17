using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 場景轉場管理器
/// 用於在關卡之間使用 Transition 場景進行轉場
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;
    
    // 存儲下一個要加載的場景名稱
    private static string nextSceneName;
    
    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SceneTransitionManager>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 設置下一個要加載的場景，然後加載 Transition 場景
    /// </summary>
    public static void LoadSceneWithTransition(string sceneName)
    {
        nextSceneName = sceneName;
        Debug.Log($"[SceneTransitionManager] 設置下一個場景: {sceneName}");
        SceneManager.LoadScene("Transition");
    }
    
    /// <summary>
    /// 獲取下一個要加載的場景名稱（由 TransitionMover 調用）
    /// </summary>
    public static string GetNextSceneName()
    {
        return nextSceneName;
    }
    
    /// <summary>
    /// 清除下一個場景名稱（可選，用於清理）
    /// </summary>
    public static void ClearNextSceneName()
    {
        nextSceneName = null;
    }
}

