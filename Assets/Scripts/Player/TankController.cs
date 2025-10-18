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
    private Vector2 lookInput;

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
        // 獲取瞄準
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
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
        if (turret == null) return;

        // 使用InputSystem的Look delta
        float rotationAmount = lookInput.x * rotationSpeed * Time.deltaTime;
        turret.Rotate(0, rotationAmount, 0);

        // Debug看數值
        Debug.Log($"Look Input: {lookInput}, Rotation Amount: {rotationAmount}");
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
