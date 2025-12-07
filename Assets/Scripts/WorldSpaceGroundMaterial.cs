using UnityEngine;

/// <summary>
/// 自動設置地面材質使用世界坐標平鋪
/// 這樣無論 Plane 多大，紋理都不會被拉伸
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WorldSpaceGroundMaterial : MonoBehaviour
{
    [Header("紋理設置")]
    [Tooltip("每多少世界單位平鋪一次紋理")]
    [SerializeField] private float tilingScale = 1f;

    [Tooltip("紋理偏移")]
    [SerializeField] private Vector2 textureOffset = Vector2.zero;

    private Renderer meshRenderer;
    private Material groundMaterial;

    void Start()
    {
        SetupMaterial();
    }

    void SetupMaterial()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("[WorldSpaceGroundMaterial] 找不到 Renderer 組件！");
            return;
        }

        // 獲取材質的實例（避免修改原始材質）
        groundMaterial = meshRenderer.material;

        // 計算基於世界空間的平鋪
        UpdateTextureTiling();

        Debug.Log("[WorldSpaceGroundMaterial] 世界空間平鋪已設置");
    }

    void UpdateTextureTiling()
    {
        if (groundMaterial == null) return;

        // 獲取 Plane 的世界空間尺寸
        // Unity Plane 預設是 10x10，所以實際大小 = scale * 10
        Vector3 scale = transform.lossyScale;
        float worldSizeX = scale.x * 10f;
        float worldSizeZ = scale.z * 10f;

        // 計算平鋪次數
        Vector2 tiling = new Vector2(
            worldSizeX / tilingScale,
            worldSizeZ / tilingScale
        );

        // 應用到材質
        groundMaterial.mainTextureScale = tiling;
        groundMaterial.mainTextureOffset = textureOffset;

        Debug.Log($"[WorldSpaceGroundMaterial] 平鋪設置為: {tiling}");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && groundMaterial != null)
        {
            UpdateTextureTiling();
        }
    }
#endif
}
