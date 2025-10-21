using UnityEngine;
using UnityEngine.InputSystem;

public class WebGLTankController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePoint;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    // 輸入緩存
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Vector2 lookInput;

    // 組件
    private Rigidbody rb;
    private PlayerInput playerInput;

    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;

    // WebGL 兼容性
    private bool isWebGL = false;
    private bool mouseEnabled = false;

    void Awake()
    {
        // 檢測 WebGL 平台
        #if UNITY_WEBGL && !UNITY_EDITOR
        isWebGL = true;
        #endif

        // 獲取組件
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        // 如果沒有設定相機，使用主相機
        if (playerCamera == null)
            playerCamera = Camera.main;

        // 設定 Rigidbody 約束
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        // 設定輸入動作
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Attack"];

        moveAction.Enable();
        lookAction.Enable();
        shootAction.Enable();

        // WebGL 特殊處理
        if (isWebGL)
        {
            StartCoroutine(WebGLInitialization());
        }
    }

    void OnDisable()
    {
        // 禁用輸入動作
        moveAction?.Disable();
        lookAction?.Disable();
        shootAction?.Disable();
    }

    private System.Collections.IEnumerator WebGLInitialization()
    {
        // 等待 WebGL 完全初始化
        yield return new WaitForSeconds(1f);
        
        // 啟用鼠標輸入
        mouseEnabled = true;
        Debug.Log("WebGL 坦克控制器初始化完成");
    }

    void Update()
    {
        // 處理輸入
        HandleInput();

        // 處理炮管旋轉
        HandleTurretRotation();
    }

    void FixedUpdate()
    {
        // 處理移動
        HandleMovement();
    }

    private void HandleInput()
    {
        // 移動輸入
        moveInput = moveAction.ReadValue<Vector2>();
        
        // 視角輸入
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        
        // 鼠標位置
        if (mouseEnabled)
        {
            mousePosition = lookAction.ReadValue<Vector2>();
        }
    }

    private void HandleMovement()
    {
        // 計算移動方向
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // 移動坦克
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);

        // 如果移動，旋轉坦克車身
        if (moveDirection.magnitude > 0.1f && tankBody != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleTurretRotation()
    {
        if (turret == null || playerCamera == null) 
        {
            if (isWebGL)
            {
                Debug.LogWarning($"WebGL 炮管旋轉失敗: turret={turret != null}, camera={playerCamera != null}");
            }
            return;
        }

        // WebGL 兼容性檢查
        if (isWebGL && !mouseEnabled)
        {
            return;
        }

        try
        {
            // 獲取鼠標位置
            Vector2 mousePos = mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldMousePos = ray.GetPoint(distance);
                Vector3 direction = (worldMousePos - turret.position).normalized;
                direction.y = 0; // 保持水平

                if (direction.magnitude > 0.1f)
                {
                    // 計算目標旋轉角度
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // 平滑旋轉炮管
                    turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    
                    if (isWebGL)
                    {
                        Debug.Log($"WebGL 炮管旋轉: 方向={direction}, 目標角度={targetRotation.eulerAngles}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WebGL 炮管旋轉錯誤: {e.Message}");
        }
    }

    public Vector3 GetFirePointPosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        if (turret != null)
        {
            return turret.position;
        }

        return transform.position;
    }

    public Vector3 GetFireDirection()
    {
        return turret != null ? turret.forward : transform.forward;
    }

    public bool IsShootPressed()
    {
        return shootAction.WasPressedThisFrame();
    }

    // 繪製 Gizmos
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
            Gizmos.DrawRay(firePoint.position, GetFireDirection() * 2f);
        }
    }
}

