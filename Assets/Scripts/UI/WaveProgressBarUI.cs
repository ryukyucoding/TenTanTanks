using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Wave Progress Bar UI Controller
/// 控制下一波敵人襲來的時間進度條
/// </summary>
public class WaveProgressBarUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("時間條 Image（使用 Filled 模式）")]
    [SerializeField] private Image timeBar;

    [Tooltip("火焰圖示（自動跟隨 timeBar 右側）")]
    [SerializeField] private Image fireIcon;

    [Tooltip("Progress Bar 背景 RectTransform（用於計算位置）")]
    [SerializeField] private RectTransform progressBarBackground;

    [Tooltip("Tank 圖示容器")]
    [SerializeField] private RectTransform tankIconsContainer;

    [Tooltip("Wave Mark 容器")]
    [SerializeField] private RectTransform waveMarksContainer;

    [Header("Prefab References")]
    [Tooltip("Tank 圖示 Prefab（如果留空則不生成）")]
    [SerializeField] private GameObject tankIconPrefab;

    [Tooltip("Wave Mark Prefab（如果留空則不生成）")]
    [SerializeField] private GameObject waveMarkPrefab;

    [Header("Settings")]
    [Tooltip("是否自動生成坦克圖標標記（false = 手動在 Editor 中放置）")]
    [SerializeField] private bool autoGenerateTankIcons = false;

    [Tooltip("是否自動生成波次標記（false = 手動在 Editor 中放置）")]
    [SerializeField] private bool autoGenerateWaveMarks = false;

    [Tooltip("是否在等待下一波時顯示進度條")]
    [SerializeField] private bool showDuringWaveDelay = true;

    [Tooltip("是否在波次進行中隱藏")]
    [SerializeField] private bool hideDuringWave = false;

    // 私有變數
    private float totalLevelTime = 0f;      // 關卡總時間
    private float currentLevelTime = 0f;    // 當前已經過的時間
    private bool isActive = false;          // 是否正在計時

    private List<float> waveSpawnTimes = new List<float>();  // 每波出現的時間點
    private List<GameObject> spawnedTankIcons = new List<GameObject>();
    private List<GameObject> spawnedWaveMarks = new List<GameObject>();

    // 時間結束回調
    public System.Action OnTimeUp;  // 當時間到時觸發

    private void Update()
    {
        if (isActive && totalLevelTime > 0)
        {
            // 更新關卡時間（累加）
            currentLevelTime += Time.deltaTime;

            if (currentLevelTime >= totalLevelTime)
            {
                currentLevelTime = totalLevelTime;
                isActive = false;
                Debug.Log("[WaveProgressBar] Level time finished!");

                // 觸發時間結束回調
                OnTimeUp?.Invoke();
            }

            // 計算剩餘時間的比例（從 1 縮小到 0）
            float remainingRatio = 1f - (currentLevelTime / totalLevelTime);

            // 更新 timeBar 的 Fill Amount
            if (timeBar != null)
            {
                timeBar.fillAmount = Mathf.Clamp01(remainingRatio);
            }

            // 更新 FireIcon 位置，讓它跟隨 timeBar 的右側
            UpdateFireIconPosition(remainingRatio);
        }
    }

    /// <summary>
    /// 更新火焰圖示位置，讓它跟隨時間條的右側
    /// </summary>
    private void UpdateFireIconPosition(float fillAmount)
    {
        if (fireIcon == null || timeBar == null) return;

        RectTransform fireRect = fireIcon.rectTransform;
        RectTransform timeBarRect = timeBar.rectTransform;

        if (fireRect != null && timeBarRect != null)
        {
            // 計算 timeBar 的實際寬度
            float barWidth = timeBarRect.rect.width;

            // 根據 fillAmount 計算火焰的位置
            // fillAmount = 1 時在最右側，= 0 時在最左側
            float xPosition = -barWidth / 2f + (barWidth * fillAmount);

            // 保存當前的 Y 位置
            float currentY = fireRect.anchoredPosition.y;

            // 更新位置
            fireRect.anchoredPosition = new Vector2(xPosition, currentY);

            // Debug 日誌（用於除錯）
            if (fillAmount == 1f || fillAmount < 0.5f) // 只在開始和中途顯示
            {
                Debug.Log($"[WaveProgressBar] FireIcon - Fill: {fillAmount:F2}, X: {xPosition:F1}, BarWidth: {barWidth:F1}");
            }
        }
    }

    /// <summary>
    /// 初始化整個關卡的時間軸
    /// </summary>
    /// <param name="levelConfig">關卡配置</param>
    public void InitializeLevelTimeline(LevelDataConfig levelConfig)
    {
        if (levelConfig == null || levelConfig.waves == null)
        {
            Debug.LogError("[WaveProgressBar] Invalid level config!");
            return;
        }

        // 決定總時間：優先使用 survivalTime，否則使用 waveDelay 總和
        if (levelConfig.survivalTime > 0)
        {
            // 使用設定的存活時間
            totalLevelTime = levelConfig.survivalTime;
            Debug.Log($"[WaveProgressBar] Using survivalTime: {totalLevelTime}s");
        }
        else
        {
            // 計算所有 waveDelay 的總和
            totalLevelTime = 0f;
            foreach (var wave in levelConfig.waves)
            {
                totalLevelTime += wave.waveDelay;
            }
            Debug.Log($"[WaveProgressBar] Using sum of waveDelays: {totalLevelTime}s");
        }

        // 計算每波出現的時間點
        waveSpawnTimes.Clear();
        float accumulatedTime = 0f;

        for (int i = 0; i < levelConfig.waves.Length; i++)
        {
            WaveConfig wave = levelConfig.waves[i];

            // 優先使用 spawnTime（新系統），否則使用累積的 waveDelay（舊系統）
            if (wave.spawnTime > 0 || i == 0)
            {
                // 使用絕對時間
                waveSpawnTimes.Add(wave.spawnTime);
            }
            else
            {
                // 使用相對時間（累積）
                waveSpawnTimes.Add(accumulatedTime);
                accumulatedTime += wave.waveDelay;
            }
        }

        Debug.Log($"[WaveProgressBar] Level timeline initialized. Total time: {totalLevelTime}s, Waves: {levelConfig.waves.Length}");

        // 重置計時
        currentLevelTime = 0f;
        isActive = true;

        // 重置 timeBar
        if (timeBar != null)
        {
            timeBar.fillAmount = 1f;
        }

        // 初始化 FireIcon 位置
        UpdateFireIconPosition(1f);

        // 清除舊標記
        ClearAllMarkers();

        // 生成波次標記在時間軸上
        GenerateWaveMarkers(levelConfig);

        // 顯示進度條
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 舊方法（保留兼容性，但現在會警告）
    /// </summary>
    [System.Obsolete("Use InitializeLevelTimeline instead")]
    public void StartWaveCountdown(float delay, WaveConfig waveConfig = null)
    {
        Debug.LogWarning("[WaveProgressBar] StartWaveCountdown is deprecated. Use InitializeLevelTimeline instead.");
    }

    /// <summary>
    /// 停止倒數計時（波次開始時調用）- 新版本不需要此方法
    /// </summary>
    public void StopCountdown()
    {
        // 新設計不需要停止，時間軸持續進行
        // 保留此方法以兼容舊代碼
    }

    /// <summary>
    /// 生成波次標記在時間軸上
    /// </summary>
    private void GenerateWaveMarkers(LevelDataConfig levelConfig)
    {
        if (progressBarBackground == null)
        {
            Debug.LogWarning("[WaveProgressBar] ProgressBarBackground 未設定");
            return;
        }

        float barWidth = progressBarBackground.rect.width;

        // 為每一波生成標記
        for (int i = 0; i < levelConfig.waves.Length; i++)
        {
            WaveConfig wave = levelConfig.waves[i];

            // 直接使用 LevelDatabase 中的 spawnTime
            float waveSpawnTime = wave.spawnTime;

            // 計算這一波在時間軸上的比例（0 = 開始，1 = 結束）
            float timeRatio = waveSpawnTime / totalLevelTime;

            // 因為 timeBar 是從右往左縮短（剩餘時間），所以標記也要從右側開始計算
            // remainingRatio = 1 - timeRatio，代表這一波出現時，timeBar 還剩多少
            float remainingRatio = 1f - timeRatio;

            // 計算標記在進度條上的位置（使用 localPosition 相對於容器）
            // progressBarBackground 的左側 = -barWidth/2，右側 = +barWidth/2
            float xPosition = -barWidth / 2f + (barWidth * remainingRatio);

            Debug.Log($"[WaveProgressBar] Wave {i}: spawnTime={waveSpawnTime}s, timeRatio={timeRatio:F2}, remainingRatio={remainingRatio:F2}, xPos={xPosition:F1}, barWidth={barWidth:F1}");

            // 只在啟用自動生成時才生成 wave mark
            if (autoGenerateWaveMarks && waveMarkPrefab != null && waveMarksContainer != null)
            {
                GameObject waveMark = Instantiate(waveMarkPrefab, waveMarksContainer);
                RectTransform markRect = waveMark.GetComponent<RectTransform>();

                if (markRect != null)
                {
                    // 設定 anchor 為中心點，然後設定位置
                    markRect.anchorMin = new Vector2(0.5f, 0.5f);
                    markRect.anchorMax = new Vector2(0.5f, 0.5f);
                    markRect.pivot = new Vector2(0.5f, 0.5f);
                    markRect.anchoredPosition = new Vector2(xPosition, 0);
                }

                spawnedWaveMarks.Add(waveMark);
            }

            // 只在啟用自動生成時才生成 tank icons
            if (autoGenerateTankIcons && tankIconPrefab != null && tankIconsContainer != null)
            {
                foreach (var enemy in wave.enemies)
                {
                    GameObject tankIcon = Instantiate(tankIconPrefab, tankIconsContainer);
                    RectTransform iconRect = tankIcon.GetComponent<RectTransform>();

                    if (iconRect != null)
                    {
                        // 設定 anchor 為中心點，然後設定位置
                        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                        iconRect.pivot = new Vector2(0.5f, 0.5f);
                        iconRect.anchoredPosition = new Vector2(xPosition, 0);
                    }

                    spawnedTankIcons.Add(tankIcon);
                }
            }
        }

        Debug.Log($"[WaveProgressBar] Generated {spawnedWaveMarks.Count} wave marks and {spawnedTankIcons.Count} tank icons");
    }

    /// <summary>
    /// 根據波次配置生成 Tank 圖示（舊方法，已被 GenerateWaveMarkers 取代）
    /// </summary>
    [System.Obsolete("This method is deprecated. GenerateWaveMarkers handles tank icon generation now.")]
    private void GenerateTankIcons(WaveConfig waveConfig)
    {
        // 這個方法已經不再使用，所有標記生成都在 GenerateWaveMarkers 中處理
        Debug.LogWarning("[WaveProgressBarUI] GenerateTankIcons is deprecated and should not be called.");
    }

    /// <summary>
    /// 設定 Wave Mark（波次標記）
    /// 在 Unity Editor 中手動放置，或通過程式生成
    /// </summary>
    public void SetWaveMarks(int totalWaves, int currentWave)
    {
        // 這個方法可以用於更新波次標記
        // 你可以手動在 Unity 中放置 wave_mark，或者用這個方法動態生成

        if (waveMarkPrefab == null || waveMarksContainer == null)
        {
            return;
        }

        ClearWaveMarks();

        float barWidth = progressBarBackground.rect.width;

        for (int i = 0; i < totalWaves; i++)
        {
            GameObject waveMark = Instantiate(waveMarkPrefab, waveMarksContainer);
            RectTransform markRect = waveMark.GetComponent<RectTransform>();

            if (markRect != null)
            {
                // 平均分配波次標記位置
                float positionRatio = (float)i / Mathf.Max(1, totalWaves - 1);
                float xPosition = -barWidth / 2f + (barWidth * positionRatio);
                markRect.anchoredPosition = new Vector2(xPosition, 0);

                // 可以高亮當前波次
                if (i == currentWave)
                {
                    Image markImage = waveMark.GetComponent<Image>();
                    if (markImage != null)
                    {
                        markImage.color = Color.yellow; // 高亮當前波
                    }
                }
            }

            spawnedWaveMarks.Add(waveMark);
        }
    }

    /// <summary>
    /// 清除所有標記
    /// </summary>
    private void ClearAllMarkers()
    {
        ClearTankIcons();
        // Wave Marks 通常不需要每次清除，除非你想動態更新
    }

    /// <summary>
    /// 清除所有 Tank 圖示
    /// </summary>
    private void ClearTankIcons()
    {
        foreach (var icon in spawnedTankIcons)
        {
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        spawnedTankIcons.Clear();
    }

    /// <summary>
    /// 清除所有 Wave Marks
    /// </summary>
    private void ClearWaveMarks()
    {
        foreach (var mark in spawnedWaveMarks)
        {
            if (mark != null)
            {
                Destroy(mark);
            }
        }
        spawnedWaveMarks.Clear();
    }

    /// <summary>
    /// 顯示進度條
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隱藏進度條
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 手動設定進度（0-1）
    /// </summary>
    public void SetProgress(float progress)
    {
        if (timeBar != null)
        {
            timeBar.fillAmount = Mathf.Clamp01(progress);
        }
    }

    private void OnValidate()
    {
        // Editor 中驗證設定
        if (timeBar != null && timeBar.type != Image.Type.Filled)
        {
            Debug.LogWarning("[WaveProgressBarUI] TimeBar 應該設定為 Filled 類型！");
        }
    }
}
