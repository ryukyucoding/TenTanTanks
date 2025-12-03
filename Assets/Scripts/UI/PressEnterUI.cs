using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Press Enter to Start 閃爍提示 UI
/// 在攝影機轉完之後顯示，按下 Enter 後進入第一關
/// </summary>
public class PressEnterUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("顯示 'Press Enter to Start...' 的文字組件")]
    [SerializeField] private TextMeshProUGUI pressEnterText;

    [Header("Text Style Settings")]
    [Tooltip("文字內容")]
    [SerializeField] private string displayText = "Press Enter to Start...";

    [Tooltip("文字顏色")]
    [SerializeField] private Color textColor = Color.white;

    [Tooltip("字體大小")]
    [SerializeField] private float fontSize = 48f;

    [Tooltip("文字對齊方式")]
    [SerializeField] private TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.Center;

    [Header("Blinking Settings")]
    [Tooltip("閃爍速度（秒）")]
    [SerializeField] private float blinkSpeed = 0.8f;

    [Tooltip("延遲顯示時間（秒）- 讓標題先出現")]
    [SerializeField] private float delayBeforeShow = 2f;

    [Header("Scene Settings")]
    [Tooltip("按下 Enter 後要載入的場景")]
    [SerializeField] private string firstLevelScene = "Level1";

    [Header("Audio (Optional)")]
    [Tooltip("按下 Enter 時的音效")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterSound;

    private bool isActive = false;
    private bool hasStarted = false;
    private Coroutine blinkCoroutine;

    void Start()
    {
        // 初始化文字
        if (pressEnterText != null)
        {
            ApplyTextStyle();

            // 一開始隱藏，等攝影機轉完再顯示
            SetTextVisible(false);
        }
        else
        {
            Debug.LogError("[PressEnterUI] pressEnterText 未指定！");
        }
    }

    /// <summary>
    /// 應用文字樣式
    /// </summary>
    private void ApplyTextStyle()
    {
        if (pressEnterText == null) return;

        pressEnterText.text = displayText;
        pressEnterText.color = textColor;
        pressEnterText.fontSize = fontSize;
        pressEnterText.alignment = alignment;

        // 確保文字不會被裁切
        pressEnterText.enableWordWrapping = false;
        pressEnterText.overflowMode = TMPro.TextOverflowModes.Overflow;
    }

    void Update()
    {
        // 只有在啟用狀態且尚未開始時，才監聽 Enter 鍵
        if (isActive && !hasStarted)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartGame();
            }
        }
    }

    /// <summary>
    /// 啟用 Press Enter 提示（由 CinematicIntroCamera 的 OnIntroFinished 事件調用）
    /// </summary>
    public void ShowPressEnter()
    {
        if (pressEnterText == null)
        {
            Debug.LogError("[PressEnterUI] pressEnterText 未指定，無法顯示！");
            return;
        }

        // 延遲顯示，讓標題先出現
        StartCoroutine(ShowPressEnterDelayed());
    }

    /// <summary>
    /// 延遲顯示 Press Enter 提示
    /// </summary>
    private IEnumerator ShowPressEnterDelayed()
    {
        Debug.Log($"[PressEnterUI] 將在 {delayBeforeShow} 秒後顯示 Press Enter 提示");

        yield return new WaitForSeconds(delayBeforeShow);

        Debug.Log("[PressEnterUI] 顯示 Press Enter 提示");

        isActive = true;
        SetTextVisible(true);

        // 開始閃爍動畫
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkText());
    }

    /// <summary>
    /// 隱藏 Press Enter 提示
    /// </summary>
    public void HidePressEnter()
    {
        isActive = false;
        SetTextVisible(false);

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    /// <summary>
    /// 設置文字可見性
    /// </summary>
    private void SetTextVisible(bool visible)
    {
        if (pressEnterText != null)
        {
            Color color = pressEnterText.color;
            color.a = visible ? 1f : 0f;
            pressEnterText.color = color;
        }
    }

    /// <summary>
    /// 閃爍文字動畫
    /// </summary>
    private IEnumerator BlinkText()
    {
        while (isActive)
        {
            // 淡出
            yield return StartCoroutine(FadeText(1f, 0.2f, blinkSpeed / 2f));

            // 淡入
            yield return StartCoroutine(FadeText(0.2f, 1f, blinkSpeed / 2f));
        }
    }

    /// <summary>
    /// 文字淡入淡出
    /// </summary>
    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        if (pressEnterText == null) yield break;

        Color color = pressEnterText.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            pressEnterText.color = color;
            yield return null;
        }

        // 確保最終透明度正確
        color.a = endAlpha;
        pressEnterText.color = color;
    }

    /// <summary>
    /// 開始遊戲（進入第一關）
    /// </summary>
    private void StartGame()
    {
        if (hasStarted) return;

        hasStarted = true;

        Debug.Log($"[PressEnterUI] 按下 Enter，載入場景: {firstLevelScene}");

        // 播放音效
        if (audioSource != null && enterSound != null)
        {
            audioSource.PlayOneShot(enterSound);
        }

        // 停止閃爍
        HidePressEnter();

        // 使用 SceneTransitionManager 載入第一關（會經過 Transition 場景）
        SceneTransitionManager.LoadSceneWithTransition(firstLevelScene);
    }

    /// <summary>
    /// 提供給外部直接呼叫的開始遊戲方法（可選）
    /// </summary>
    public void TriggerStartGame()
    {
        StartGame();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor 預覽功能：在 Inspector 中調整參數時即時更新文字樣式
    /// </summary>
    void OnValidate()
    {
        if (pressEnterText != null && !Application.isPlaying)
        {
            ApplyTextStyle();
            SetTextVisible(true); // 編輯模式下顯示文字以便預覽
        }
    }
#endif
}
