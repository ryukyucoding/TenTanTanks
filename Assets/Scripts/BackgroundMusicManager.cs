using UnityEngine;

/// <summary>
/// 背景音樂管理器
/// 管理場景的背景音樂播放，支持循環和非循環模式
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("音樂設置")]
    [Tooltip("背景音樂音訊片段")]
    [SerializeField] private AudioClip backgroundMusic;

    [Tooltip("是否循環播放")]
    [SerializeField] private bool loopMusic = true;

    [Tooltip("音樂音量 (0-1)")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    [Tooltip("淡入時間（秒）")]
    [SerializeField] private float fadeInDuration = 1f;

    private AudioSource audioSource;
    private static BackgroundMusicManager instance;

    void Awake()
    {
        // 檢查是否已有實例
        if (instance != null && instance != this)
        {
            // 如果場景切換後有新的音樂管理器，停止舊的
            if (instance.audioSource != null && instance.audioSource.isPlaying)
            {
                instance.audioSource.Stop();
            }
            Destroy(instance.gameObject);
        }

        instance = this;

        // 設置 AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = loopMusic;
        audioSource.volume = 0f; // 從 0 開始，用於淡入效果
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic();
        }
        else
        {
            Debug.LogWarning($"[BackgroundMusicManager] 場景 {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} 未設置背景音樂");
        }
    }

    private void PlayMusic()
    {
        audioSource.clip = backgroundMusic;
        audioSource.Play();

        // 淡入效果
        if (fadeInDuration > 0)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            audioSource.volume = volume;
        }

        Debug.Log($"[BackgroundMusicManager] 播放音樂: {backgroundMusic.name} (循環: {loopMusic})");
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeInDuration);
            yield return null;
        }

        audioSource.volume = volume;
    }

    /// <summary>
    /// 停止音樂（帶淡出效果）
    /// </summary>
    public void StopMusic(float fadeOutDuration = 1f)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAndStop(fadeOutDuration));
        }
    }

    private System.Collections.IEnumerator FadeOutAndStop(float fadeOutDuration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    /// <summary>
    /// 設置音樂音量
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
