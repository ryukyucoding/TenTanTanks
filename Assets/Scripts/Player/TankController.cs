using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;      // 坦克車身（移動時會旋轉）
    [SerializeField] private Transform turret;        // 砲塔（跟隨滑鼠旋轉）
    [SerializeField] private Transform firePoint;     // 砲彈發射點

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;     // 玩家攝像機

    // 輸入變數
    private Vector2 moveInput;
    private Vector2 mousePosition;

    // 組件引用
    private Rigidbody rb;
    private PlayerInput playerInput;

    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;

    void Awake()
    {
        // 獲取組件
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        // 如果沒有指定攝像機，嘗試找到主攝像機
        if (playerCamera == null)
            playerCamera = Camera.main;

        // 設置Rigidbody（防止翻滾）
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        // 綁定輸入事件
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Attack"];

        moveAction.Enable();
        lookAction.Enable();
        shootAction.Enable();
    }

    void OnDisable()
    {
        // 解除綁定
        moveAction?.Disable();
        lookAction?.Disable();
        shootAction?.Disable();
    }

    void Update()
    {
        // 讀取輸入
        HandleInput();

        // 處理砲塔旋轉（每幀更新）
        HandleTurretRotation();
    }

    void FixedUpdate()
    {
        // 處理移動（物理更新）
        HandleMovement();
    }

    private void HandleInput()
    {
        // 獲取移動輸入
        moveInput = moveAction.ReadValue<Vector2>();

        // 獲取滑鼠位置
        mousePosition = lookAction.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        // 計算移動方向
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // 應用移動
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);

        // 如果有移動輸入，旋轉車身朝向移動方向
        if (moveDirection.magnitude > 0.1f && tankBody != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleTurretRotation()
    {
        if (turret == null || playerCamera == null) return;

        // 將滑鼠螢幕座標轉換為世界座標
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        // 創建一個在坦克高度的平面
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        // 檢查射線與平面的交點
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePos = ray.GetPoint(distance);

            // 計算從砲塔到滑鼠位置的方向
            Vector3 direction = (worldMousePos - turret.position).normalized;
            direction.y = 0; // 保持在水平面上

            // 旋轉砲塔朝向滑鼠位置
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        // 讓FirePoint也跟著砲塔旋轉
        if (firePoint != null && turret != null)
        {
            firePoint.rotation = turret.rotation;
        }
    }

    // 提供給射擊腳本使用的方法
    public Vector3 GetFirePointPosition()
    {
        return firePoint != null ? firePoint.position : turret.position;
    }

    public Vector3 GetFireDirection()
    {
        return turret != null ? turret.forward : transform.forward;
    }

    public bool IsShootPressed()
    {
        return shootAction.WasPressedThisFrame();
    }

    // 除錯用的Gizmos
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
