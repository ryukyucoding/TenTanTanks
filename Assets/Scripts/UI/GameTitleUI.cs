using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 遊戲標題 UI
/// 在攝影機動畫結束後淡入顯示，然後在 Press Enter 出現前保持顯示
/// </summary>
public class GameTitleUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("顯示遊戲標題的文字組件")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Title Settings")]
    [Tooltip("標題文字內容")]
    [SerializeField] private string titleContent = "Ten Tan Tanks!!!";

    [Tooltip("標題文字顏色")]
    [SerializeField] private Color titleColor = Color.white;

    [Tooltip("字體大小")]
    [SerializeField] private float fontSize = 120f;

    [Tooltip("字體 Asset")]
    [SerializeField] private TMP_FontAsset fontAsset;

    [Tooltip("文字對齊方式")]
    [SerializeField] private TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.Center;

    [Header("Animation Settings")]
    [Tooltip("淡入動畫持續時間（秒）")]
    [SerializeField] private float fadeInDuration = 1.5f;

    [Tooltip("標題淡入後是否保持顯示（不淡出）")]
    [SerializeField] private bool stayVisible = true;

    [Header("Effects (Optional)")]
    [Tooltip("啟用打字機效果")]
    [SerializeField] private bool enableTypewriterEffect = false;

    [Tooltip("打字機效果速度（字符/秒）")]
    [SerializeField] private float typewriterSpeed = 20f;

    [Tooltip("啟用縮放動畫")]
    [SerializeField] private bool enableScaleAnimation = false;

    [Tooltip("縮放動畫起始大小")]
    [SerializeField] private float scaleFrom = 0.5f;

    [Tooltip("縮放動畫結束大小")]
    [SerializeField] private float scaleTo = 1f;

    private bool isAnimating = false;
    private Coroutine animationCoroutine;

    void Start()
    {
        // 初始化標題文字
        if (titleText != null)
        {
            ApplyTitleStyle();
            SetTitleVisible(false);
        }
        else
        {
            Debug.LogError("[GameTitleUI] titleText 未指定！");
        }
    }

    /// <summary>
    /// 應用標題樣式
    /// </summary>
    private void ApplyTitleStyle()
    {
        if (titleText == null) return;

        titleText.text = titleContent;
        titleText.color = titleColor;
        titleText.fontSize = fontSize;
        titleText.alignment = alignment;

        // 設置字體
        if (fontAsset != null)
        {
            titleText.font = fontAsset;
        }

        // 確保文字不會被裁切
        titleText.enableWordWrapping = false;
        titleText.overflowMode = TMPro.TextOverflowModes.Overflow;

        Debug.Log($"[GameTitleUI] 已套用標題樣式，字體: {(fontAsset != null ? fontAsset.name : "預設")}");
    }

    /// <summary>
    /// 設置標題可見性
    /// </summary>
    private void SetTitleVisible(bool visible)
    {
        if (titleText == null) return;

        Color color = titleText.color;
        color.a = visible ? 1f : 0f;
        titleText.color = color;
    }

    /// <summary>
    /// 顯示標題動畫（由 CinematicIntroCamera 的 OnIntroFinished 調用）
    /// </summary>
    public void ShowTitle()
    {
        if (isAnimating)
        {
            Debug.LogWarning("[GameTitleUI] 標題動畫正在播放中");
            return;
        }

        if (titleText == null)
        {
            Debug.LogError("[GameTitleUI] titleText 未指定，無法顯示標題！");
            return;
        }

        Debug.Log("[GameTitleUI] 開始顯示標題動畫");

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(TitleAnimationSequence());
    }

    /// <summary>
    /// 標題動畫序列
    /// </summary>
    private IEnumerator TitleAnimationSequence()
    {
        isAnimating = true;

        // 重置狀態
        SetTitleVisible(false);
        if (enableScaleAnimation)
        {
            titleText.transform.localScale = Vector3.one * scaleFrom;
        }

        // 如果啟用打字機效果
        if (enableTypewriterEffect)
        {
            yield return StartCoroutine(TypewriterEffect());
        }
        else
        {
            // 淡入動畫
            yield return StartCoroutine(FadeTitle(0f, 1f, fadeInDuration));
        }

        // 如果啟用縮放動畫
        if (enableScaleAnimation)
        {
            StartCoroutine(ScaleAnimation());
        }

        // 如果設定為保持顯示，就不淡出
        if (stayVisible)
        {
            isAnimating = false;
            Debug.Log("[GameTitleUI] 標題淡入完成，保持顯示");
        }
        else
        {
            // 停留顯示（已移除，因為不需要淡出）
            // 可以保留這段如果想要淡出前有停留時間
            // yield return new WaitForSeconds(displayDuration);

            // 淡出動畫
            yield return StartCoroutine(FadeTitle(1f, 0f, 0.5f));
            isAnimating = false;
            Debug.Log("[GameTitleUI] 標題動畫完成（已淡出）");
        }
    }

    /// <summary>
    /// 淡入淡出動畫
    /// </summary>
    private IEnumerator FadeTitle(float startAlpha, float endAlpha, float duration)
    {
        if (titleText == null) yield break;

        Color color = titleText.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 使用 Smooth Step 讓動畫更流暢
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            color.a = Mathf.Lerp(startAlpha, endAlpha, smoothT);
            titleText.color = color;

            yield return null;
        }

        // 確保最終透明度正確
        color.a = endAlpha;
        titleText.color = color;
    }

    /// <summary>
    /// 打字機效果
    /// </summary>
    private IEnumerator TypewriterEffect()
    {
        if (titleText == null) yield break;

        SetTitleVisible(true);

        string fullText = titleContent;
        titleText.text = "";

        float charDelay = 1f / typewriterSpeed;

        for (int i = 0; i <= fullText.Length; i++)
        {
            titleText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(charDelay);
        }
    }

    /// <summary>
    /// 縮放動畫
    /// </summary>
    private IEnumerator ScaleAnimation()
    {
        if (titleText == null) yield break;

        Transform textTransform = titleText.transform;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);

            // 使用彈性曲線
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float scale = Mathf.Lerp(scaleFrom, scaleTo, smoothT);
            textTransform.localScale = Vector3.one * scale;

            yield return null;
        }

        textTransform.localScale = Vector3.one * scaleTo;
    }

    /// <summary>
    /// 立即隱藏標題
    /// </summary>
    public void HideTitle()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        SetTitleVisible(false);
        isAnimating = false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor 預覽功能
    /// </summary>
    void OnValidate()
    {
        if (titleText != null && !Application.isPlaying)
        {
            ApplyTitleStyle();
            SetTitleVisible(true); // 編輯模式下顯示以便預覽
        }
    }
#endif
}
