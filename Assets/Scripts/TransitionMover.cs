using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionMover : MonoBehaviour
{
    [SerializeField] float speed =  12;       // 移動速度（單位/秒）
    [SerializeField] float targetX = 12;     // 目標 X 座標
    [SerializeField] string nextScene = "Level1";  // 預設場景（如果沒有通過 SceneTransitionManager 設置）

    void Start()
    {
        // 優先使用 SceneTransitionManager 設置的場景
        string transitionScene = SceneTransitionManager.GetNextSceneName();
        if (!string.IsNullOrEmpty(transitionScene))
        {
            nextScene = transitionScene;
            Debug.Log($"[TransitionMover] 使用 SceneTransitionManager 設置的場景: {nextScene}");
        }
        else
        {
            Debug.Log($"[TransitionMover] 使用預設場景: {nextScene}");
        }
    }

    void Update()
    {
        Vector3 pos = transform.position;

        if (pos.x < targetX)
        {
            // 朝 targetX 推進；用 MoveTowards 可避免超過
            float step = speed * Time.deltaTime;
            pos.x = Mathf.MoveTowards(pos.x, targetX, step);
            transform.position = pos;
        }
        else
        {
            LoadNext();
        }
    }

    void LoadNext()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        
        // 清除 SceneTransitionManager 中的場景名稱
        SceneTransitionManager.ClearNextSceneName();
        
        Debug.Log($"[TransitionMover] 加載場景: {nextScene}");
        SceneManager.LoadScene(nextScene);
        enabled = false; // 防止重複呼叫
    }
}