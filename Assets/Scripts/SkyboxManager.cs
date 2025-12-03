using UnityEngine;

/// <summary>
/// Skybox 管理器
/// 可以在場景中動態設置和切換天空盒
/// </summary>
public class SkyboxManager : MonoBehaviour
{
    [Header("Skybox Material")]
    [Tooltip("要應用的 Skybox 材質（從 SkySeries Freebie 中選擇）")]
    [SerializeField] private Material skyboxMaterial;

    [Header("Settings")]
    [Tooltip("在遊戲開始時自動套用 Skybox")]
    [SerializeField] private bool applyOnStart = true;

    [Tooltip("Skybox 旋轉速度（度/秒），設為 0 則不旋轉")]
    [SerializeField] private float rotationSpeed = 0f;

    [Tooltip("Skybox 曝光度調整（1 為正常）")]
    [SerializeField, Range(0f, 3f)] private float exposure = 1f;

    [Header("Lighting")]
    [Tooltip("同時調整環境光照強度")]
    [SerializeField, Range(0f, 2f)] private float ambientIntensity = 1f;

    private float currentRotation = 0f;

    void Start()
    {
        if (applyOnStart && skyboxMaterial != null)
        {
            ApplySkybox();
        }
    }

    void Update()
    {
        // 旋轉 Skybox（如果速度不為 0）
        if (rotationSpeed != 0f)
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
        }
    }

    /// <summary>
    /// 應用 Skybox 材質到場景
    /// </summary>
    public void ApplySkybox()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogWarning("[SkyboxManager] Skybox 材質未指定！");
            return;
        }

        // 設置 Skybox
        RenderSettings.skybox = skyboxMaterial;

        // 設置曝光度
        RenderSettings.skybox.SetFloat("_Exposure", exposure);

        // 設置環境光照
        RenderSettings.ambientIntensity = ambientIntensity;

        // 重新生成環境光照
        DynamicGI.UpdateEnvironment();

        Debug.Log($"[SkyboxManager] 已套用 Skybox: {skyboxMaterial.name}");
    }

    /// <summary>
    /// 動態切換 Skybox 材質
    /// </summary>
    public void ChangeSkybox(Material newSkybox)
    {
        if (newSkybox == null)
        {
            Debug.LogWarning("[SkyboxManager] 新的 Skybox 材質為 null！");
            return;
        }

        skyboxMaterial = newSkybox;
        ApplySkybox();
    }

    /// <summary>
    /// 設置 Skybox 旋轉速度
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    /// <summary>
    /// 設置 Skybox 曝光度
    /// </summary>
    public void SetExposure(float exp)
    {
        exposure = Mathf.Clamp(exp, 0f, 3f);
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetFloat("_Exposure", exposure);
        }
    }

    /// <summary>
    /// 設置環境光照強度
    /// </summary>
    public void SetAmbientIntensity(float intensity)
    {
        ambientIntensity = Mathf.Clamp(intensity, 0f, 2f);
        RenderSettings.ambientIntensity = ambientIntensity;
    }

#if UNITY_EDITOR
    // Editor 專用：在 Inspector 中預覽 Skybox
    void OnValidate()
    {
        if (Application.isPlaying && skyboxMaterial != null)
        {
            ApplySkybox();
        }
    }
#endif
}
