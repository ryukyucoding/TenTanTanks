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

    [Header("坦克參考")]
    [SerializeField] private Transform tankTransform;         // 坦克物件的 Transform
    [SerializeField] private Vector3 levelTextOffset = new Vector3(0, 2f, 0);  // 關卡文字相對坦克的偏移

    private int currentLevel;
    private int nextLevel;
    private int currentHealth;
    private int previousHealth;
    private bool shouldShowHealthGain;

    void Start()
    {
        Debug.Log("[TransitionUI] 初始化開始");

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

    void Update()
    {
        // 更新左右兩側的關卡文字位置（跟隨坦克）
        UpdateLevelTextPositions();
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
            leftLevelText.text = $"關卡 {currentLevel}";
            leftLevelText.gameObject.SetActive(false);  // 初始隱藏，等坦克到達左側時顯示
        }

        if (rightLevelText != null)
        {
            rightLevelText.text = $"關卡 {nextLevel}";
            rightLevelText.gameObject.SetActive(false);  // 初始隱藏，等坦克接近右側時顯示
        }
    }

    /// <summary>
    /// 更新關卡文字位置（跟隨坦克）
    /// </summary>
    private void UpdateLevelTextPositions()
    {
        if (tankTransform == null) return;

        Vector3 tankPos = tankTransform.position;

        // 左側文字：坦克在左半邊時顯示
        if (leftLevelText != null)
        {
            if (tankPos.x < 0 && !leftLevelText.gameObject.activeSelf)
            {
                leftLevelText.gameObject.SetActive(true);
            }

            if (leftLevelText.gameObject.activeSelf)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(tankPos + levelTextOffset);
                leftLevelText.transform.position = screenPos;
            }
        }

        // 右側文字：坦克在右半邊時顯示
        if (rightLevelText != null)
        {
            if (tankPos.x > 0 && !rightLevelText.gameObject.activeSelf)
            {
                rightLevelText.gameObject.SetActive(true);
            }

            if (rightLevelText.gameObject.activeSelf)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(tankPos + levelTextOffset);
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
