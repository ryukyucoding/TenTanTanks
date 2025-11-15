using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Components")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.green;

    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private Vector3 tooltipOffset = new Vector3(0, 100, 0);

    private UpgradeOption upgradeOption;
    private System.Action onClickCallback;
    private bool isSelected = false;
    private bool isHovered = false;
    private Vector3 originalScale;
    private Coroutine currentAnimation;
    private GameObject currentTooltip;

    private void Awake()
    {
        originalScale = transform.localScale;

        // 確保有Button組件
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            button = gameObject.AddComponent<Button>();

        // 確保有背景圖片
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage == null)
        {
            backgroundImage = gameObject.AddComponent<Image>();
            // 可以在這裡設置默認的按鈕背景
        }
    }

    public void Setup(UpgradeOption option, System.Action onClick)
    {
        upgradeOption = option;
        onClickCallback = onClick;

        UpdateVisuals();
        SetSelected(false);
    }

    private void UpdateVisuals()
    {
        if (upgradeOption == null) return;

        // 設置圖標
        if (iconImage != null && upgradeOption.icon != null)
            iconImage.sprite = upgradeOption.icon;

        // 設置名稱
        if (nameText != null)
            nameText.text = upgradeOption.upgradeName;

        // 設置描述
        if (descriptionText != null)
            descriptionText.text = upgradeOption.description;

        // 設置顏色
        UpdateColor();
    }

    private void UpdateColor()
    {
        Color targetColor = normalColor;

        if (isSelected)
            targetColor = selectedColor;
        else if (isHovered)
            targetColor = hoverColor;

        if (backgroundImage != null)
            backgroundImage.color = targetColor;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateColor();

        if (selected)
        {
            // 選中時的特殊效果
            AnimateScale(hoverScale);
        }
        else if (!isHovered)
        {
            AnimateScale(1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateColor();

        if (!isSelected)
            AnimateScale(hoverScale);

        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateColor();

        if (!isSelected)
            AnimateScale(1f);

        HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (upgradeOption != null)
        {
            // 點擊動畫
            StartCoroutine(ClickAnimation());

            // 執行回調
            onClickCallback?.Invoke();

            Debug.Log($"Clicked upgrade: {upgradeOption.upgradeName}");
        }
    }

    private IEnumerator ClickAnimation()
    {
        // 按下效果
        yield return AnimateScale(pressScale);

        // 恢復
        float targetScale = isSelected ? hoverScale : (isHovered ? hoverScale : 1f);
        yield return AnimateScale(targetScale);

        // 顏色閃爍效果
        if (backgroundImage != null)
        {
            Color originalColor = backgroundImage.color;
            backgroundImage.color = pressedColor;

            yield return new WaitForSeconds(0.1f);

            backgroundImage.color = originalColor;
        }
    }

    private Coroutine AnimateScale(float targetScale)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(ScaleAnimation(targetScale));
        return currentAnimation;
    }

    private IEnumerator ScaleAnimation(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * targetScale;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float curveValue = scaleCurve.Evaluate(progress);

            transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            yield return null;
        }

        transform.localScale = endScale;
        currentAnimation = null;
    }

    private void ShowTooltip()
    {
        if (tooltipPrefab == null || upgradeOption == null) return;

        // 創建工具提示
        currentTooltip = Instantiate(tooltipPrefab, transform.parent);
        currentTooltip.transform.position = transform.position + tooltipOffset;

        // 設置工具提示內容
        var tooltipText = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltipText != null)
        {
            string tooltipContent = $"<b>{upgradeOption.upgradeName}</b>\n\n{upgradeOption.description}\n\n";
            tooltipContent += $"傷害: {upgradeOption.stats.damage}\n";
            tooltipContent += $"射速: {upgradeOption.stats.fireRate}/秒\n";
            tooltipContent += $"子彈大小: {upgradeOption.stats.bulletSize}\n";
            tooltipContent += $"移動速度: {upgradeOption.stats.moveSpeed}";

            tooltipText.text = tooltipContent;
        }

        // 工具提示淡入動畫
        var canvasGroup = currentTooltip.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = currentTooltip.AddComponent<CanvasGroup>();

        StartCoroutine(FadeTooltip(canvasGroup, 0f, 1f, 0.2f));
    }

    private void HideTooltip()
    {
        if (currentTooltip != null)
        {
            var canvasGroup = currentTooltip.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeTooltip(canvasGroup, 1f, 0f, 0.1f, () => {
                    Destroy(currentTooltip);
                    currentTooltip = null;
                }));
            }
            else
            {
                Destroy(currentTooltip);
                currentTooltip = null;
            }
        }
    }

    private IEnumerator FadeTooltip(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        canvasGroup.alpha = startAlpha;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }

    public UpgradeOption GetUpgradeOption()
    {
        return upgradeOption;
    }

    private void OnDestroy()
    {
        // 清理工具提示
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
        }

        // 停止動畫
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
    }

    // 公開方法供外部調用
    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;

        // 可以添加視覺反饋，比如變灰
        var color = backgroundImage.color;
        color.a = interactable ? 1f : 0.5f;
        backgroundImage.color = color;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void PlaySelectAnimation()
    {
        StartCoroutine(SelectPulseAnimation());
    }

    private IEnumerator SelectPulseAnimation()
    {
        float pulseScale = hoverScale * 1.2f;

        yield return AnimateScale(pulseScale);
        yield return new WaitForSeconds(0.1f);
        yield return AnimateScale(hoverScale);
    }
}