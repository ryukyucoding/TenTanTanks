using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrimaryDividerLines : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private Color lineColor = Color.black;
    [SerializeField] private float lineWidth = 3f;
    [SerializeField] private float outerRadius = 480f; // Full wheel radius

    [Header("Animation Settings")]
    [SerializeField] private float delayBeforeLines = 0.5f;
    [SerializeField] private float lineFadeInDuration = 0.3f;
    [SerializeField] private bool animateLines = true;

    private Transform lineContainer;

    void Start()
    {
        if (animateLines)
        {
            StartCoroutine(CreateDividerLinesDelayed());
        }
        else
        {
            CreateDividerLines();
        }
    }

    private IEnumerator CreateDividerLinesDelayed()
    {
        yield return new WaitForSeconds(delayBeforeLines);
        yield return StartCoroutine(CreateDividerLinesAnimated());
    }

    private IEnumerator CreateDividerLinesAnimated()
    {
        CreateDividerLines();

        Image[] lineImages = lineContainer.GetComponentsInChildren<Image>();

        // Set all lines to transparent initially
        foreach (Image lineImage in lineImages)
        {
            Color color = lineImage.color;
            color.a = 0f;
            lineImage.color = color;
        }

        // Fade in all lines
        float elapsed = 0f;
        while (elapsed < lineFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / lineFadeInDuration;

            foreach (Image lineImage in lineImages)
            {
                Color color = lineColor;
                color.a = alpha;
                lineImage.color = color;
            }

            yield return null;
        }

        foreach (Image lineImage in lineImages)
        {
            lineImage.color = lineColor;
        }
    }

    [ContextMenu("Create Primary Divider Lines")]
    public void CreateDividerLines()
    {
        ClearExistingLines();

        if (lineContainer == null)
        {
            GameObject container = new GameObject("PrimaryDividerLines");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            lineContainer = container.transform;
        }

        // Create main division lines: 0¢X, 120¢X, 240¢X (from center to edge)
        // These go behind everything except Basic center
        float[] angles = { -60f, 60f, 180f }; // Adjusted for your rotation

        foreach (float angle in angles)
        {
            CreateRadialLine(angle, 0f, outerRadius, "PrimaryDivision");
        }
    }

    private void CreateRadialLine(float angle, float startRadius, float endRadius, string name)
    {
        GameObject lineObj = CreateLineObject(name);

        float radian = angle * Mathf.Deg2Rad;
        Vector3 startPos = new Vector3(
            Mathf.Cos(radian) * startRadius,
            Mathf.Sin(radian) * startRadius,
            0f
        );
        Vector3 endPos = new Vector3(
            Mathf.Cos(radian) * endRadius,
            Mathf.Sin(radian) * endRadius,
            0f
        );

        Vector3 centerPos = (startPos + endPos) / 2f;
        float lineLength = Vector3.Distance(startPos, endPos);

        centerPos.z = 2f; // Behind tier 1 and tier 2 buttons
        lineObj.transform.localPosition = centerPos;
        lineObj.transform.localRotation = Quaternion.Euler(0, 0, angle - 90f);

        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(lineWidth, lineLength);
    }

    private GameObject CreateLineObject(string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(lineContainer);
        lineObj.transform.localScale = Vector3.one;

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = lineColor;

        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector3.zero;

        return lineObj;
    }

    private void ClearExistingLines()
    {
        if (lineContainer != null)
        {
            for (int i = lineContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(lineContainer.GetChild(i).gameObject);
            }
        }
    }

    public void CreateLinesWithDelay()
    {
        StartCoroutine(CreateDividerLinesDelayed());
    }
}
