using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Transition 場景 UI 管理器
/// 顯示關卡資訊、生命值和動畫效果
/// </summary>
public class TransitionUI : MonoBehaviour
{
    [Header("關卡顯示")]
    [SerializeField] private TextMeshProUGUI leftLevelText;   // 坦克左側的關卡數字（剛破完的關卡）
    [SerializeField] private TextMeshProUGUI rightLevelText;  // 坦克右側的關卡數字（即將前往的關卡）
    [SerializeField] private TMP_FontAsset fontAsset;         // 字體資源（可選，不設定則使用預設字體）
    [SerializeField] private float fontSize = 36f;            // 字體大小
    [SerializeField] private Color textColor = Color.white;   // 文字顏色

    [Header("生命值顯示")]
    [SerializeField] private Transform heartsContainer;       // 愛心容器
    [SerializeField] private Sprite heartSprite;              // 愛心圖片 Sprite
    [SerializeField] private Vector2 heartSize = new Vector2(40f, 40f);  // 愛心大小
    [SerializeField] private float heartSpacing = 50f;        // 愛心之間的間距
    [SerializeField] private float heartZOffset = 0f;         // 愛心 Z 軸偏移

    [Header("愛心增加動畫")]
    [SerializeField] private float animationDelay = 1f;       // 動畫延遲時間（秒）
    [SerializeField] private float scaleAnimationDuration = 0.5f;  // 縮放動畫時長
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 縮放曲線

    [Header("關卡文字位置設定")]
    [SerializeField] private bool followTank = false;         // 是否跟隨坦克移動
    [SerializeField] private Transform tankTransform;         // 坦克物件的 Transform（只在 followTank = true 時需要）
    [SerializeField] private Vector3 leftLevelTextOffset = new Vector3(0, 2f, 0);   // 左側關卡文字相對坦克的偏移
    [SerializeField] private Vector3 rightLevelTextOffset = new Vector3(0, 2f, 0);  // 右側關卡文字相對坦克的偏移

    [Header("固定位置設定（不跟隨坦克時使用）")]
    [SerializeField] private Vector2 leftTextScreenPosition = new Vector2(200, 400);   // 左側文字螢幕座標
    [SerializeField] private Vector2 rightTextScreenPosition = new Vector2(600, 400);  // 右側文字螢幕座標
    [SerializeField] private float displayDuration = 2f;      // 每個文字顯示的時間（秒）

    private int currentLevel;
    private int nextLevel;
    private int currentHealth;
    private int previousHealth;
    private bool shouldShowHealthGain;

    void Start()
    {
        Debug.Log("[TransitionUI] 初始化開始");

        // 設置愛心容器位置
        SetupHeartsContainerPosition();

        // 獲取關卡資訊
        GetLevelInfo();

        // 獲取生命值資訊
        GetHealthInfo();

        // 更新 UI
        UpdateLevelDisplay();
        UpdateHealthDisplay();

        // 如果需要顯示加命動畫，延遲播放
        if (shouldShowHealthGain)
        {
            StartCoroutine(PlayHealthGainAnimation());
        }
    }

    /// <summary>
    /// 設置愛心容器位置（畫面下方 2/3）
    /// </summary>
    private void SetupHeartsContainerPosition()
    {
        Debug.Log($"[TransitionUI] SetupHeartsContainerPosition 被調用");

        // 如果容器為 null 或不是 RectTransform，自動創建
        RectTransform containerRect = null;

        if (heartsContainer != null)
        {
            containerRect = heartsContainer.GetComponent<RectTransform>();
        }

        if (containerRect == null)
        {
            Debug.LogWarning("[TransitionUI] heartsContainer 不是有效的 UI 元素，自動創建新的容器");

            // 找到 Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TransitionUI] 場景中沒有 Canvas，無法創建愛心容器");
                return;
            }

            // 創建新的 UI 容器
            GameObject containerObj = new GameObject("HeartsContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            containerRect = containerObj.AddComponent<RectTransform>();
            heartsContainer = containerObj.transform;

            Debug.Log("[TransitionUI] 已自動創建 HeartsContainer");
        }

        // 設置錨點為畫面下方中心
        containerRect.anchorMin = new Vector2(0.5f, 0f);
        containerRect.anchorMax = new Vector2(0.5f, 0f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);

        // 設置位置：從底部往上 1/3 的位置
        // 假設畫面高度為 Screen.height，2/3 處就是 Screen.height * (1/3)
        float yPosition = Screen.height * (1f / 3f);
        containerRect.anchoredPosition = new Vector2(0, yPosition);

        Debug.Log($"[TransitionUI] 愛心容器位置設定為畫面下方 2/3，Y: {yPosition}");
    }

    void Update()
    {
        // 更新關卡文字位置
        if (followTank)
        {
            UpdateLevelTextPositions();  // 跟隨坦克
        }
        else
        {
            UpdateFixedTextPositions();  // 固定位置
        }
    }

    /// <summary>
    /// 獲取關卡資訊
    /// </summary>
    private void GetLevelInfo()
    {
        string nextSceneName = SceneTransitionManager.GetNextSceneName();

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[TransitionUI] 無法獲取下一個場景名稱");
            currentLevel = 1;
            nextLevel = 1;
            return;
        }

        // 從場景名稱中提取關卡數字（例如 "Level3" -> 3）
        nextLevel = ExtractLevelNumber(nextSceneName);
        currentLevel = Mathf.Max(1, nextLevel - 1);

        Debug.Log($"[TransitionUI] 當前關卡: {currentLevel}, 下一關: {nextLevel}");
    }

    /// <summary>
    /// 獲取生命值資訊
    /// </summary>
    private void GetHealthInfo()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("[TransitionUI] PlayerDataManager.Instance 為 null");
            currentHealth = 3;
            previousHealth = 3;
            shouldShowHealthGain = false;
            return;
        }

        // 在 TransitionMover.Start() 中已經處理過加命邏輯
        // 這裡我們需要知道是否剛剛加過命
        currentHealth = PlayerDataManager.Instance.GetCurrentHealth();

        // 檢查是否是會加命的關卡轉換（2->3 或 4->5）
        shouldShowHealthGain = (nextLevel == 3 || nextLevel == 5);

        if (shouldShowHealthGain)
        {
            previousHealth = currentHealth - 1;  // 加命前的生命值
            Debug.Log($"[TransitionUI] 偵測到加命關卡，前: {previousHealth}, 後: {currentHealth}");
        }
        else
        {
            previousHealth = currentHealth;
        }
    }

    /// <summary>
    /// 更新關卡顯示
    /// </summary>
    private void UpdateLevelDisplay()
    {
        if (leftLevelText != null)
        {
            leftLevelText.text = $"Level {currentLevel}";
            ApplyTextStyle(leftLevelText);
            // 如果不跟隨坦克，開始時顯示左側文字
            if (!followTank)
            {
                leftLevelText.gameObject.SetActive(true);
                StartCoroutine(ShowTextSequence());
            }
            else
            {
                leftLevelText.gameObject.SetActive(tankTransform == null);
            }
        }

        if (rightLevelText != null)
        {
            rightLevelText.text = $"Level {nextLevel}";
            ApplyTextStyle(rightLevelText);
            // 如果不跟隨坦克，初始隱藏右側文字
            if (!followTank)
            {
                rightLevelText.gameObject.SetActive(false);
            }
            else
            {
                rightLevelText.gameObject.SetActive(tankTransform == null);
            }
        }
    }

    /// <summary>
    /// 顯示文字序列（固定位置模式）
    /// </summary>
    private IEnumerator ShowTextSequence()
    {
        // 先顯示左側文字
        yield return new WaitForSeconds(displayDuration);

        // 切換到右側文字
        if (leftLevelText != null) leftLevelText.gameObject.SetActive(false);
        if (rightLevelText != null) rightLevelText.gameObject.SetActive(true);

        Debug.Log("[TransitionUI] 切換到右側關卡文字");
    }

    /// <summary>
    /// 應用文字樣式（字體、大小、顏色）
    /// </summary>
    private void ApplyTextStyle(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        // 設定字體（如果有指定）
        if (fontAsset != null)
        {
            textComponent.font = fontAsset;
        }

        // 設定字體大小
        textComponent.fontSize = fontSize;

        // 設定文字顏色
        textComponent.color = textColor;

        // 設定文字框大小，避免文字被裁切
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 設置足夠大的文字框，避免換行或裁切
            rectTransform.sizeDelta = new Vector2(500, 200);  // 寬度500，高度200
        }

        // 禁用自動換行
        textComponent.enableWordWrapping = false;

        // 設定溢出模式為 Overflow（讓文字可以超出邊界）
        textComponent.overflowMode = TMPro.TextOverflowModes.Overflow;

        // 設定對齊方式為中心
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
    }

    /// <summary>
    /// 更新固定位置的文字
    /// </summary>
    private void UpdateFixedTextPositions()
    {
        if (leftLevelText != null && leftLevelText.gameObject.activeSelf)
        {
            leftLevelText.transform.position = leftTextScreenPosition;
        }

        if (rightLevelText != null && rightLevelText.gameObject.activeSelf)
        {
            rightLevelText.transform.position = rightTextScreenPosition;
        }
    }

    /// <summary>
    /// 更新關卡文字位置（跟隨坦克）
    /// </summary>
    private void UpdateLevelTextPositions()
    {
        if (tankTransform == null) return;

        Vector3 tankPos = tankTransform.position;

        // 每秒記錄一次坦克位置
        if (Time.frameCount % 60 == 0)  // 假設 60 FPS
        {
            Debug.Log($"[TransitionUI] 坦克當前座標: X={tankPos.x:F2}, Y={tankPos.y:F2}, Z={tankPos.z:F2}");
        }

        // 左側文字：坦克在螢幕左半邊時顯示（X < 6，假設中點是 6）
        if (leftLevelText != null)
        {
            // 坦克 X 座標 < 6 時顯示左側文字
            if (tankPos.x < 0 && !leftLevelText.gameObject.activeSelf)
            {
                leftLevelText.gameObject.SetActive(true);
                Debug.Log($"[TransitionUI] 顯示左側關卡文字，坦克位置: {tankPos.x}");
            }

            // 當坦克移動到右半邊時，隱藏左側文字
            if (tankPos.x >= 0 && leftLevelText.gameObject.activeSelf)
            {
                leftLevelText.gameObject.SetActive(false);
                Debug.Log($"[TransitionUI] 隱藏左側關卡文字，坦克位置: {tankPos.x}");
            }

            if (leftLevelText.gameObject.activeSelf)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(tankPos + leftLevelTextOffset);
                leftLevelText.transform.position = screenPos;
            }
        }

        // 右側文字：坦克在螢幕右半邊時顯示（X >= 6）
        if (rightLevelText != null)
        {
            if (tankPos.x >= 0 && !rightLevelText.gameObject.activeSelf)
            {
                rightLevelText.gameObject.SetActive(true);
                Debug.Log($"[TransitionUI] 顯示右側關卡文字，坦克位置: {tankPos.x}");
            }

            // 當坦克在左半邊時，隱藏右側文字
            if (tankPos.x < 0 && rightLevelText.gameObject.activeSelf)
            {
                rightLevelText.gameObject.SetActive(false);
                Debug.Log($"[TransitionUI] 隱藏右側關卡文字，坦克位置: {tankPos.x}");
            }

            if (rightLevelText.gameObject.activeSelf)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(tankPos + rightLevelTextOffset);
                rightLevelText.transform.position = screenPos;
            }
        }
    }

    /// <summary>
    /// 更新生命值顯示
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (heartsContainer == null || heartSprite == null)
        {
            Debug.LogWarning("[TransitionUI] 缺少愛心容器或愛心圖片");
            return;
        }

        // 清空現有的愛心
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        // 顯示初始生命值（如果會加命，先顯示加命前的數量）
        int displayHealth = shouldShowHealthGain ? previousHealth : currentHealth;

        for (int i = 0; i < displayHealth; i++)
        {
            CreateHeart(i, displayHealth);
        }
    }

    /// <summary>
    /// 創建一個愛心
    /// </summary>
    private GameObject CreateHeart(int index, int totalHearts)
    {
        // 創建一個新的 GameObject 作為愛心
        GameObject heart = new GameObject($"Heart_{index}");
        heart.transform.SetParent(heartsContainer, false);

        // 添加 Image 組件
        Image heartImage = heart.AddComponent<Image>();
        heartImage.sprite = heartSprite;
        heartImage.preserveAspect = true;

        // 設置 RectTransform
        RectTransform rectTransform = heart.GetComponent<RectTransform>();

        // 設置錨點為中心點
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 設置大小
        rectTransform.sizeDelta = heartSize;

        // 計算居中位置
        // 總寬度 = (愛心數量 - 1) * (愛心寬度 + 間距) + 愛心寬度
        float totalWidth = (totalHearts - 1) * (heartSize.x + heartSpacing) + heartSize.x;
        // 起始位置（最左邊的愛心中心點）
        float startX = -totalWidth / 2f + heartSize.x / 2f;
        // 當前愛心的位置
        float xPos = startX + index * (heartSize.x + heartSpacing);

        rectTransform.anchoredPosition = new Vector2(xPos, 0);

        // 設置 Z 軸偏移
        Vector3 localPos = rectTransform.localPosition;
        localPos.z = heartZOffset;
        rectTransform.localPosition = localPos;

        return heart;
    }

    /// <summary>
    /// 播放愛心增加動畫
    /// </summary>
    private IEnumerator PlayHealthGainAnimation()
    {
        // 等待一段時間後播放動畫
        yield return new WaitForSeconds(animationDelay);

        Debug.Log($"[TransitionUI] 播放加命動畫：{previousHealth} -> {currentHealth}");

        // 需要重新排列所有愛心以保持居中
        // 先清空所有現有愛心
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        // 重新創建所有愛心（包括新的），使用新的總數量來計算居中位置
        for (int i = 0; i < currentHealth; i++)
        {
            GameObject heart = CreateHeart(i, currentHealth);

            // 如果是新增的愛心，播放縮放動畫
            if (i == previousHealth)
            {
                StartCoroutine(ScaleHeartAnimation(heart));
            }
        }
    }

    /// <summary>
    /// 愛心縮放動畫
    /// </summary>
    private IEnumerator ScaleHeartAnimation(GameObject heart)
    {
        if (heart == null) yield break;

        Transform heartTransform = heart.transform;
        Vector3 targetScale = heartTransform.localScale;

        // 從 0 開始縮放
        heartTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleAnimationDuration);
            float curveValue = scaleCurve.Evaluate(t);

            heartTransform.localScale = targetScale * curveValue;

            yield return null;
        }

        // 確保最終縮放正確
        heartTransform.localScale = targetScale;
    }

    /// <summary>
    /// 從場景名稱中提取關卡數字
    /// </summary>
    private int ExtractLevelNumber(string sceneName)
    {
        // 移除 "Level" 前綴，取得數字
        string numberStr = sceneName.Replace("Level", "");

        if (int.TryParse(numberStr, out int levelNumber))
        {
            return levelNumber;
        }

        Debug.LogWarning($"[TransitionUI] 無法從場景名稱 '{sceneName}' 中提取關卡數字");
        return 1;
    }
}
