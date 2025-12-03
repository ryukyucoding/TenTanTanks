using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerHealth playerHealthReference; // 手動指定玩家（可選）
    [SerializeField] private bool autoFindPlayer = true; // 自動尋找玩家
    [SerializeField] private float retryDelay = 0.5f; // 如果找不到玩家，延遲重試

    [Header("UI Settings")]
    [SerializeField] private GameObject heartPrefab;  // 愛心圖片的預製物（Image）
    [SerializeField] private Transform heartsContainer; // 存放愛心的容器
    [SerializeField] private float heartSpacing = 10f; // 愛心之間的間距

    [Header("Heart Sprite")]
    [SerializeField] private Sprite fullHeartSprite; // 完整的愛心圖片
    [SerializeField] private Sprite emptyHeartSprite; // 空的愛心圖片（可選）

    [Header("Heart Settings")]
    [SerializeField] private Vector2 heartSize = new Vector2(40f, 40f); // 愛心大小

    private List<Image> heartImages = new List<Image>();
    private PlayerHealth playerHealth;
    private bool isInitialized = false;

    void Start()
    {
        // 如果手動指定了玩家，直接使用
        if (playerHealthReference != null)
        {
            SetPlayer(playerHealthReference);
        }
        else if (autoFindPlayer)
        {
            // 嘗試尋找玩家
            StartCoroutine(FindPlayerWithRetry());
        }
    }

    private IEnumerator FindPlayerWithRetry()
    {
        int maxRetries = 10;
        int retries = 0;

        while (retries < maxRetries && playerHealth == null)
        {
            FindPlayerHealth();

            if (playerHealth != null)
            {
                // 找到玩家了，初始化UI
                playerHealth.OnHealthChanged += UpdateHealthDisplay;
                UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.CurrentHealth);
                isInitialized = true;
                yield break;
            }

            retries++;
            yield return new WaitForSeconds(retryDelay);
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("HealthUI: 找不到 PlayerHealth 元件！請確認場景中有標籤為 'Player' 的物件，或手動指定 Player Health Reference。");
        }
    }

    void OnDestroy()
    {
        // 取消訂閱事件
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
    }

    private void FindPlayerHealth()
    {
        // 尋找場景中的玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void UpdateHealthDisplay(int currentHealth, int unusedParameter)
    {
        // 確保有足夠的愛心圖示（動態調整）
        while (heartImages.Count < currentHealth)
        {
            // 需要更多愛心，創建新的
            CreateHeart(heartImages.Count);
        }

        // 更新每個愛心的顯示狀態
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
            {
                if (i < currentHealth)
                {
                    // 顯示完整的愛心
                    if (fullHeartSprite != null)
                        heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].enabled = true;
                }
                else
                {
                    // 超過當前生命的愛心隱藏
                    heartImages[i].enabled = false;
                }
            }
        }
    }

    private void CreateHeart(int index)
    {
        GameObject heartObj;

        if (heartPrefab != null)
        {
            // 使用預製物
            heartObj = Instantiate(heartPrefab, heartsContainer);
        }
        else
        {
            // 動態創建 Image
            heartObj = new GameObject($"Heart_{index}");
            heartObj.transform.SetParent(heartsContainer);
            heartObj.AddComponent<Image>();
        }

        // 設置 RectTransform
        RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = heartSize;
        rectTransform.anchoredPosition = new Vector2(index * (heartSize.x + heartSpacing), 0);

        // 設置 Image
        Image heartImage = heartObj.GetComponent<Image>();
        if (fullHeartSprite != null)
        {
            heartImage.sprite = fullHeartSprite;
        }

        heartImages.Add(heartImage);
    }

    // 手動設置玩家（如果需要）
    public void SetPlayer(PlayerHealth health)
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }

        playerHealth = health;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.CurrentHealth);
        }
    }
}
