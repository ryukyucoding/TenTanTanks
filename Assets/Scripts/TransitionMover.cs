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

        // 檢查 PlayerDataManager 是否存在
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("[TransitionMover] PlayerDataManager.Instance 為 null！請確保場景中有 PlayerDataManager 物件。");
        }
        else
        {
            Debug.Log($"[TransitionMover] PlayerDataManager 存在，當前生命值: {PlayerDataManager.Instance.GetCurrentHealth()}");
        }

        // 在特定關卡轉換時增加生命值
        ApplyHealthBonus(nextScene);
    }

    /// <summary>
    /// 在特定關卡轉換時增加生命值
    /// 2→3關 和 4→5關時加一命
    /// </summary>
    private void ApplyHealthBonus(string targetScene)
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("[TransitionMover] PlayerDataManager.Instance 為 null，無法加命！");
            return;
        }

        Debug.Log($"[TransitionMover] 檢查目標場景: {targetScene}");

        // 檢查目標場景是否為 Level3 或 Level5
        if (targetScene == "Level3")
        {
            int beforeHealth = PlayerDataManager.Instance.GetCurrentHealth();
            PlayerDataManager.Instance.AddHealth(1);
            int afterHealth = PlayerDataManager.Instance.GetCurrentHealth();
            Debug.Log($"[TransitionMover] ★★★ 進入 Level3，獲得額外生命 +1 (前: {beforeHealth}, 後: {afterHealth})");
        }
        else if (targetScene == "Level5")
        {
            int beforeHealth = PlayerDataManager.Instance.GetCurrentHealth();
            PlayerDataManager.Instance.AddHealth(1);
            int afterHealth = PlayerDataManager.Instance.GetCurrentHealth();
            Debug.Log($"[TransitionMover] ★★★ 進入 Level5，獲得額外生命 +1 (前: {beforeHealth}, 後: {afterHealth})");
        }
        else
        {
            Debug.Log($"[TransitionMover] 目標場景 '{targetScene}' 不需要加命");
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