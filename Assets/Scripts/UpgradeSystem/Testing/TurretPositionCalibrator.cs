using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 砲塔位置校正工具 - 运行时调整砲塔位置
/// 使用方向键和数字键调整位置和旋转
/// </summary>
public class TurretPositionCalibrator : MonoBehaviour
{
    [Header("调整速度")]
    [SerializeField] private float positionStep = 0.1f;
    [SerializeField] private float rotationStep = 5f;

    [Header("当前偏移值")]
    public Vector3 currentPositionOffset = Vector3.zero;
    public Vector3 currentRotationOffset = Vector3.zero;

    private TankTransformationManager transformManager;
    private Transform currentTurret;
    private bool isCalibrating = false;

    void Start()
    {
        GameObject player = GameManager.GetPlayerTank();
        if (player != null)
        {
            transformManager = player.GetComponent<TankTransformationManager>();
            FindCurrentTurret(player);
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // F12: 开启/关闭校正模式
        if (keyboard.f12Key.wasPressedThisFrame)
        {
            isCalibrating = !isCalibrating;
            Debug.Log($"[校正模式] {(isCalibrating ? "开启" : "关闭")}");
            if (isCalibrating)
            {
                GameObject player = GameManager.GetPlayerTank();
                if (player != null) FindCurrentTurret(player);
            }
        }

        if (!isCalibrating || currentTurret == null) return;

        bool changed = false;

        // === 位置调整 ===
        // 方向键: 前后左右
        if (keyboard.upArrowKey.isPressed)
        {
            currentPositionOffset.z += positionStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.downArrowKey.isPressed)
        {
            currentPositionOffset.z -= positionStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.leftArrowKey.isPressed)
        {
            currentPositionOffset.x -= positionStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.rightArrowKey.isPressed)
        {
            currentPositionOffset.x += positionStep * Time.deltaTime * 10;
            changed = true;
        }

        // PageUp/PageDown: 上下
        if (keyboard.pageUpKey.isPressed)
        {
            currentPositionOffset.y += positionStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.pageDownKey.isPressed)
        {
            currentPositionOffset.y -= positionStep * Time.deltaTime * 10;
            changed = true;
        }

        // === 旋转调整 ===
        // Numpad 4/6: Y轴旋转（左右转）
        if (keyboard.numpad4Key.isPressed)
        {
            currentRotationOffset.y -= rotationStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.numpad6Key.isPressed)
        {
            currentRotationOffset.y += rotationStep * Time.deltaTime * 10;
            changed = true;
        }

        // Numpad 8/2: X轴旋转（俯仰）
        if (keyboard.numpad8Key.isPressed)
        {
            currentRotationOffset.x -= rotationStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.numpad2Key.isPressed)
        {
            currentRotationOffset.x += rotationStep * Time.deltaTime * 10;
            changed = true;
        }

        // Numpad 7/9: Z轴旋转（翻滚）
        if (keyboard.numpad7Key.isPressed)
        {
            currentRotationOffset.z -= rotationStep * Time.deltaTime * 10;
            changed = true;
        }
        if (keyboard.numpad9Key.isPressed)
        {
            currentRotationOffset.z += rotationStep * Time.deltaTime * 10;
            changed = true;
        }

        // R: 重置
        if (keyboard.rKey.wasPressedThisFrame)
        {
            currentPositionOffset = Vector3.zero;
            currentRotationOffset = Vector3.zero;
            changed = true;
            Debug.Log("[校正] 重置偏移值");
        }

        // P: 打印当前值
        if (keyboard.pKey.wasPressedThisFrame)
        {
            PrintCurrentValues();
        }

        // 应用偏移
        if (changed)
        {
            ApplyOffset();
        }
    }

    void FindCurrentTurret(GameObject player)
    {
        // 查找名为 "Turret" 的子物件
        for (int i = 0; i < player.transform.childCount; i++)
        {
            Transform child = player.transform.GetChild(i);
            if (child.name == "Turret" || child.name.Contains("Turret"))
            {
                currentTurret = child;
                Debug.Log($"[校正] 找到砲塔: {currentTurret.name}");
                Debug.Log($"[校正] 当前位置: {currentTurret.localPosition}");
                Debug.Log($"[校正] 当前旋转: {currentTurret.localRotation.eulerAngles}");
                return;
            }
        }
        Debug.LogWarning("[校正] 找不到砲塔！");
    }

    void ApplyOffset()
    {
        if (currentTurret == null) return;

        // 获取基础位置（假设是 Vector3.zero 或原始位置）
        Vector3 basePosition = Vector3.zero; // 你可以改为 originalTurretPosition
        Quaternion baseRotation = Quaternion.identity;

        currentTurret.localPosition = basePosition + currentPositionOffset;
        currentTurret.localRotation = baseRotation * Quaternion.Euler(currentRotationOffset);
    }

    void PrintCurrentValues()
    {
        Debug.Log("========== 当前校正值 ==========");
        Debug.Log($"位置偏移: {currentPositionOffset}");
        Debug.Log($"旋转偏移: {currentRotationOffset}");
        Debug.Log("\n复制以下代码到 Inspector:");
        Debug.Log($"Turret Position Offset: ({currentPositionOffset.x:F3}, {currentPositionOffset.y:F3}, {currentPositionOffset.z:F3})");
        Debug.Log($"Turret Rotation Offset: ({currentRotationOffset.x:F3}, {currentRotationOffset.y:F3}, {currentRotationOffset.z:F3})");
        Debug.Log("===================================");
    }

    void OnGUI()
    {
        if (!isCalibrating) return;

        GUILayout.BeginArea(new Rect(Screen.width - 450, 10, 440, 400));
        GUILayout.Label("=== 砲塔位置校正工具 ===");
        GUILayout.Label($"校正模式: {(isCalibrating ? "开启 (F12关闭)" : "关闭 (F12开启)")}");
        GUILayout.Label("");
        
        GUILayout.Label("位置调整:");
        GUILayout.Label("  ↑↓←→: 前后左右移动");
        GUILayout.Label("  PageUp/Down: 上下移动");
        GUILayout.Label("");
        
        GUILayout.Label("旋转调整:");
        GUILayout.Label("  Numpad 4/6: 左右旋转 (Y轴)");
        GUILayout.Label("  Numpad 8/2: 俯仰 (X轴)");
        GUILayout.Label("  Numpad 7/9: 翻滚 (Z轴)");
        GUILayout.Label("");
        
        GUILayout.Label("其他:");
        GUILayout.Label("  R: 重置所有偏移");
        GUILayout.Label("  P: 打印当前值到Console");
        GUILayout.Label("");
        
        GUILayout.Label($"当前位置偏移: ({currentPositionOffset.x:F3}, {currentPositionOffset.y:F3}, {currentPositionOffset.z:F3})");
        GUILayout.Label($"当前旋转偏移: ({currentRotationOffset.x:F1}°, {currentRotationOffset.y:F1}°, {currentRotationOffset.z:F1}°)");
        
        GUILayout.EndArea();
    }
}
