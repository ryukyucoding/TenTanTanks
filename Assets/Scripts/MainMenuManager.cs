using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstLevelScene = "Level1";  // 第一個關卡場景名稱
    
    public void StartGame()
    {
        // 使用 SceneTransitionManager 設置第一個關卡，然後加載 Transition 場景
        SceneTransitionManager.LoadSceneWithTransition(firstLevelScene);
    }

    public void QuitGame()
    {
        Application.Quit();
        // 若在 Unity Editor 測試，可加上 #if UNITY_EDITOR 讓它停止播放
    }
}