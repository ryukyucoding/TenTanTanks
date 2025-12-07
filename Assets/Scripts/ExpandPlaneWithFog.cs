using UnityEngine;

/// <summary>
/// 自動擴大現有的 Plane 並設置霧效
/// 掛載到你的 Plane GameObject 上即可
/// </summary>
public class ExpandPlaneWithFog : MonoBehaviour
{
    [Header("Plane 設置")]
    [Tooltip("Plane 的縮放大小（越大越接近地平線）")]
    [SerializeField] private float planeScale = 50f;

    [Tooltip("是否在 Start 時自動調整 Plane")]
    [SerializeField] private bool autoAdjustOnStart = true;

    [Header("霧效設置")]
    [Tooltip("啟用霧效")]
    [SerializeField] private bool enableFog = true;

    [Tooltip("霧的顏色（建議與 Skybox 協調）")]
    [SerializeField] private Color fogColor = new Color(0.75f, 0.8f, 0.9f); // 淺藍色

    [Tooltip("霧的起始距離")]
    [SerializeField] private float fogStart = 100f;

    [Tooltip("霧的結束距離")]
    [SerializeField] private float fogEnd = 300f;

    [Header("材質設置（可選）")]
    [Tooltip("是否調整紋理平鋪")]
    [SerializeField] private bool adjustTextureTiling = true;

    [Tooltip("紋理平鋪次數")]
    [SerializeField] private Vector2 textureTiling = new Vector2(50f, 50f);

    void Start()
    {
        if (autoAdjustOnStart)
        {
            ExpandPlane();
            SetupFog();
            AdjustTexture();
        }
    }

    /// <summary>
    /// 擴大 Plane 的尺寸
    /// </summary>
    public void ExpandPlane()
    {
        // 設置 Scale
        transform.localScale = new Vector3(planeScale, 1f, planeScale);

        Debug.Log($"[ExpandPlaneWithFog] Plane 已擴大至 Scale: {planeScale}");
    }

    /// <summary>
    /// 設置霧效
    /// </summary>
    public void SetupFog()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;

            Debug.Log($"[ExpandPlaneWithFog] 霧效已啟用，顏色: {fogColor}");
        }
        else
        {
            RenderSettings.fog = false;
            Debug.Log("[ExpandPlaneWithFog] 霧效已禁用");
        }
    }

    /// <summary>
    /// 調整紋理平鋪
    /// </summary>
    public void AdjustTexture()
    {
        if (!adjustTextureTiling) return;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.mainTextureScale = textureTiling;
            Debug.Log($"[ExpandPlaneWithFog] 紋理平鋪已調整為: {textureTiling}");
        }
    }

    /// <summary>
    /// 匹配霧顏色到 Skybox
    /// </summary>
    public void MatchFogToSkybox()
    {
        if (RenderSettings.skybox != null)
        {
            // 嘗試從 Skybox 提取主色調
            if (RenderSettings.skybox.HasProperty("_Tint"))
            {
                Color skyTint = RenderSettings.skybox.GetColor("_Tint");
                fogColor = skyTint;
                RenderSettings.fogColor = fogColor;
                Debug.Log($"[ExpandPlaneWithFog] 霧顏色已匹配 Skybox: {fogColor}");
            }
        }
    }

#if UNITY_EDITOR
    // 在 Inspector 中調整參數時即時預覽
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ExpandPlane();
            SetupFog();
            AdjustTexture();
        }
    }
#endif
}
