using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [Header("Tooltip Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private LayoutElement layoutElement;

    [Header("Tooltip Settings")]
    [SerializeField] private float maxWidth = 300f;
    [SerializeField] private float padding = 20f;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();

        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;

        if (layoutElement == null)
        {
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.preferredWidth = maxWidth;
    }

    public void SetContent(string title, string description, string stats = "")
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
        }

        if (statsText != null)
        {
            statsText.text = stats;
            statsText.gameObject.SetActive(!string.IsNullOrEmpty(stats));
        }

        // 強制重新計算布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        else
            gameObject.SetActive(false);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        // 確保工具提示不會超出螢幕邊界
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (rectTransform == null) return;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        // 檢查右邊界
        if (corners[2].x > canvasCorners[2].x)
        {
            float offset = corners[2].x - canvasCorners[2].x;
            transform.position += Vector3.left * offset;
        }

        // 檢查左邊界
        if (corners[0].x < canvasCorners[0].x)
        {
            float offset = canvasCorners[0].x - corners[0].x;
            transform.position += Vector3.right * offset;
        }

        // 檢查上邊界
        if (corners[2].y > canvasCorners[2].y)
        {
            float offset = corners[2].y - canvasCorners[2].y;
            transform.position += Vector3.down * offset;
        }

        // 檢查下邊界
        if (corners[0].y < canvasCorners[0].y)
        {
            float offset = canvasCorners[0].y - corners[0].y;
            transform.position += Vector3.up * offset;
        }
    }
}