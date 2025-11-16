using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WheelDividerLines : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform lineContainer;
    [SerializeField] private Color lineColor = Color.black;
    [SerializeField] private float lineWidth = 3f;
    [SerializeField] private float outerRadius = 480f;
    [SerializeField] private float innerRadius = 160f; // Center area radius

    [Header("Animation Settings")]
    [SerializeField] private float delayBeforeLines = 0.5f; // Wait for wheel animation
    [SerializeField] private float lineFadeInDuration = 0.3f; // Line fade-in time
    [SerializeField] private bool animateLines = true;

    [Header("Tier Divisions")]
    [SerializeField] private bool showTier1Divisions = true;
    [SerializeField] private bool showTier2Divisions = true;
    [SerializeField] private bool showInnerDivisions = true;

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
        // Wait for wheel animation to complete
        yield return new WaitForSeconds(delayBeforeLines);

        // Create lines with fade-in animation
        yield return StartCoroutine(CreateDividerLinesAnimated());
    }

    private IEnumerator CreateDividerLinesAnimated()
    {
        // Create all lines first (invisible)
        CreateDividerLines();

        // Get all line images for animation
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

        // Ensure final alpha is correct
        foreach (Image lineImage in lineImages)
        {
            lineImage.color = lineColor;
        }
    }

    [ContextMenu("Create Divider Lines")]
    public void CreateDividerLines()
    {
        // Clear existing lines
        ClearExistingLines();

        if (lineContainer == null)
        {
            // Create line container if it doesn't exist
            GameObject container = new GameObject("DividerLines");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            lineContainer = container.transform;
        }

        // Create Tier 1 division lines (120¢X intervals)
        if (showTier1Divisions)
        {
            CreateTier1Lines();
        }

        // Create Tier 2 division lines (60¢X intervals)  
        if (showTier2Divisions)
        {
            CreateTier2Lines();
        }

        // Create inner circle division (between center and tier 1)
        if (showInnerDivisions)
        {
            CreateInnerCircleDivisions();
        }
    }

    public void CreateLinesWithDelay()
    {
        StartCoroutine(CreateDividerLinesDelayed());
    }

    private void CreateTier1Lines()
    {
        // 3 lines at 120¢X intervals starting from -60¢X (adjusted for rotation)
        float[] angles = { -60f, 60f, 180f };

        foreach (float angle in angles)
        {
            CreateRadialLine(angle, innerRadius, outerRadius, "Tier1Division");
        }
    }

    private void CreateTier2Lines()
    {
        // Additional lines to divide tier 2 into 60¢X segments
        float[] angles = { -30f, 30f, 90f, 150f, 210f, 270f };

        foreach (float angle in angles)
        {
            CreateRadialLine(angle, (innerRadius + outerRadius) / 2, outerRadius, "Tier2Division");
        }
    }

    private void CreateInnerCircleDivisions()
    {
        // Lines from center to tier 1 boundary
        float[] angles = { -60f, 60f, 180f };

        foreach (float angle in angles)
        {
            CreateRadialLine(angle, 0f, innerRadius, "InnerDivision");
        }
    }

    private void CreateRadialLine(float angle, float startRadius, float endRadius, string name)
    {
        GameObject lineObj = CreateLineObject(name);

        // Calculate start and end positions
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

        // Position and rotate the line
        Vector3 centerPos = (startPos + endPos) / 2f;
        float lineLength = Vector3.Distance(startPos, endPos);

        lineObj.transform.localPosition = centerPos;
        lineObj.transform.localRotation = Quaternion.Euler(0, 0, angle - 90f); // -90 because UI images point right by default

        // Set line dimensions
        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(lineWidth, lineLength);
    }

    private GameObject CreateLineObject(string name)
    {
        GameObject lineObj;

        if (linePrefab != null)
        {
            lineObj = Instantiate(linePrefab, lineContainer);
        }
        else
        {
            // Create a basic line using UI Image
            lineObj = new GameObject(name);
            lineObj.transform.SetParent(lineContainer);
            lineObj.transform.localScale = Vector3.one;

            // Add Image component
            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = lineColor;

            // Add RectTransform
            RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector3.zero;
        }

        return lineObj;
    }

    private void ClearExistingLines()
    {
        if (lineContainer != null)
        {
            // Destroy all children
            for (int i = lineContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(lineContainer.GetChild(i).gameObject);
            }
        }
    }

    [ContextMenu("Clear All Lines")]
    public void ClearAllLines()
    {
        ClearExistingLines();
    }
}