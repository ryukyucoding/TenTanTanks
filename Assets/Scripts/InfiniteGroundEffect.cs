using UnityEngine;

/// <summary>
/// 無限地面效果
/// 創造地面延伸到天際線的視覺效果
/// </summary>
public class InfiniteGroundEffect : MonoBehaviour
{
    [Header("Ground Settings")]
    [Tooltip("地面延伸的大小（半徑）")]
    [SerializeField] private float groundRadius = 500f;

    [Tooltip("地面材質")]
    [SerializeField] private Material groundMaterial;

    [Tooltip("地面高度（Y 軸位置）")]
    [SerializeField] private float groundHeight = 0f;

    [Header("Fog Settings")]
    [Tooltip("啟用霧效來隱藏地平線邊緣")]
    [SerializeField] private bool enableFog = true;

    [Tooltip("霧的顏色（建議與 Skybox 協調）")]
    [SerializeField] private Color fogColor = new Color(0.5f, 0.7f, 0.9f);

    [Tooltip("霧的起始距離")]
    [SerializeField] private float fogStart = 100f;

    [Tooltip("霧的結束距離")]
    [SerializeField] private float fogEnd = 300f;

    [Header("Ground Texture Settings")]
    [Tooltip("地面紋理平鋪次數")]
    [SerializeField] private Vector2 textureTiling = new Vector2(50f, 50f);

    void Start()
    {
        CreateExpandedGround();
        SetupFog();
    }

    /// <summary>
    /// 創建擴展的地面
    /// </summary>
    void CreateExpandedGround()
    {
        // 檢查是否已經有地面
        GameObject existingGround = GameObject.Find("ExpandedGround");
        if (existingGround != null)
        {
            Debug.Log("[InfiniteGroundEffect] 擴展地面已存在");
            return;
        }

        // 創建新的地面平面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "ExpandedGround";
        ground.transform.position = new Vector3(0, groundHeight, 0);

        // Unity 的 Plane 預設大小是 10x10，我們需要縮放
        // 要達到 groundRadius，需要縮放 groundRadius / 5
        float scale = groundRadius / 5f;
        ground.transform.localScale = new Vector3(scale, 1f, scale);

        // 應用材質
        if (groundMaterial != null)
        {
            Renderer renderer = ground.GetComponent<Renderer>();
            renderer.material = groundMaterial;

            // 設置紋理平鋪
            renderer.material.mainTextureScale = textureTiling;
        }

        // 設置 Layer（可選，用於物理碰撞）
        ground.layer = LayerMask.NameToLayer("Default");

        Debug.Log($"[InfiniteGroundEffect] 已創建擴展地面，半徑: {groundRadius}");
    }

    /// <summary>
    /// 設置霧效
    /// </summary>
    void SetupFog()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;

            Debug.Log("[InfiniteGroundEffect] 霧效已啟用");
        }
        else
        {
            RenderSettings.fog = false;
        }
    }

    /// <summary>
    /// 動態調整霧的顏色以匹配 Skybox
    /// </summary>
    public void MatchFogToSkybox()
    {
        if (RenderSettings.skybox != null)
        {
            // 嘗試從 Skybox 材質中提取主要顏色
            // 這是一個簡化版本，實際可能需要更複雜的顏色提取
            Color skyColor = RenderSettings.skybox.HasProperty("_Tint")
                ? RenderSettings.skybox.GetColor("_Tint")
                : fogColor;

            RenderSettings.fogColor = skyColor;
            Debug.Log($"[InfiniteGroundEffect] 霧顏色已匹配 Skybox: {skyColor}");
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetupFog();
        }
    }

    // 在 Scene 視圖中繪製地面範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(0, groundHeight, 0),
            new Vector3(groundRadius * 2, 0.1f, groundRadius * 2)
        );
    }
#endif
}
