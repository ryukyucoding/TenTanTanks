using UnityEngine;

public class AudioListenerManager : MonoBehaviour
{
    [Header("音頻監聽器管理")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            FixAudioListeners();
        }
    }
    
    [ContextMenu("修復音頻監聽器")]
    public void FixAudioListeners()
    {
        // 找到所有 Audio Listener 組件
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
        
        if (allListeners.Length == 0)
        {
            Debug.LogWarning("場景中沒有找到 Audio Listener！");
            return;
        }
        
        if (allListeners.Length == 1)
        {
            Debug.Log("場景中只有一個 Audio Listener，這是正確的。");
            return;
        }
        
        Debug.LogWarning($"發現 {allListeners.Length} 個 Audio Listener，需要修復！");
        
        // 保留第一個（通常是主相機），禁用其他的
        for (int i = 1; i < allListeners.Length; i++)
        {
            if (allListeners[i] != null)
            {
                Debug.Log($"禁用 Audio Listener: {allListeners[i].gameObject.name}");
                allListeners[i].enabled = false;
            }
        }
        
        Debug.Log($"音頻監聽器修復完成！保留了 {allListeners[0].gameObject.name} 的 Audio Listener。");
    }
    
    [ContextMenu("檢查音頻監聽器狀態")]
    public void CheckAudioListeners()
    {
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
        
        Debug.Log($"=== 音頻監聽器狀態檢查 ===");
        Debug.Log($"總共找到 {allListeners.Length} 個 Audio Listener:");
        
        for (int i = 0; i < allListeners.Length; i++)
        {
            if (allListeners[i] != null)
            {
                Debug.Log($"  {i + 1}. {allListeners[i].gameObject.name} - 啟用: {allListeners[i].enabled}");
            }
        }
        
        if (allListeners.Length > 1)
        {
            Debug.LogWarning("⚠️ 場景中有多個 Audio Listener，這會導致音頻問題！");
        }
        else if (allListeners.Length == 1)
        {
            Debug.Log("✅ 音頻監聽器設置正確！");
        }
        else
        {
            Debug.LogError("❌ 場景中沒有 Audio Listener！");
        }
    }
    
    private void OnValidate()
    {
        // 在編輯器中自動檢查
        if (Application.isPlaying)
        {
            CheckAudioListeners();
        }
    }
}
