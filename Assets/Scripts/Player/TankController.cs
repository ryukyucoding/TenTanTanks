using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed = 2.5f;  // 基礎速度，會被 TankStats 動態設置
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;      
    [SerializeField] private Transform turret;        
    [SerializeField] private Transform firePoint;     

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;     
    // 嚙踝蕭J嚙豌潘蕭
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Vector2 lookInput;

    // 嚙調伐蕭犍嚙?
    private Rigidbody rb;
    private PlayerInput playerInput;

    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        Debug.Log($"[TankController.Awake] 物件: {gameObject.name}");
        Debug.Log($"  - Rigidbody: {(rb != null ? "找到" : "缺失")}");
        Debug.Log($"  - PlayerInput: {(playerInput != null ? "找到" : "缺失")}");

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        else
        {
            Debug.LogError($"? Rigidbody 缺失！物件: {gameObject.name}");
        }
    }

    /// <summary>
    /// 設置移動速度（由 TankStats 呼叫）
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        float oldSpeed = moveSpeed;
        moveSpeed = speed;
        Debug.Log($"? TankController.SetMoveSpeed 被調用！");
        Debug.Log($"   物件: {gameObject.name}");
        Debug.Log($"   舊速度: {oldSpeed:F2}");
        Debug.Log($"   新速度: {speed:F2}");
        Debug.Log($"   當前 moveSpeed 值: {moveSpeed:F2}");
    }

    /// <summary>
    /// 獲取當前移動速度（用於調試）
    /// </summary>
    public float GetCurrentMoveSpeed()
    {
        return moveSpeed;
    }

    void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError($"? PlayerInput 是 null，無法啟用輸入！物件: {gameObject.name}");
            return;
        }

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Attack"];

        moveAction?.Enable();
        lookAction?.Enable();
        shootAction?.Enable();
        
        Debug.Log($"[TankController.OnEnable] 輸入系統已啟用 (物件: {gameObject.name})");
    }

    void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        shootAction?.Disable();
    }

    void Update()
    {
        // 測試用：按 K 鍵強制設置速度為 10（使用 Input System）
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            moveSpeed = 7f;
            Debug.Log($"!!! 強制設置速度為 7.0 (物件: {gameObject.name})");
        }
        
        // 測試用：按 L 鍵顯示當前速度（使用 Input System）
        // if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        // {
        //     Debug.Log($"=== 當前移動速度 = {moveSpeed:F2} (物件: {gameObject.name}) ===");
        // }
        
        HandleInput();

        HandleTurretRotation();
    }

    void FixedUpdate()
    {
        // 嚙畿嚙緲嚙踝蕭嚙褊（嚙踝蕭嚙緲嚙踝蕭s嚙稷
        // 
        HandleMovement();
    }

    private void HandleInput()
    {
        // 檢查輸入系統是否已初始化
        if (moveAction == null || lookAction == null || playerInput == null)
        {
            Debug.LogWarning("Input系統尚未初始化！");
            return;
        }
        
        // 嚙踝蕭嚙踝蕭嚙踝蕭尪嚙皚
        moveInput = moveAction.ReadValue<Vector2>();
        // 嚙踝蕭嚙踝蕭侇嚙?
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        // 嚙踝蕭嚙踝蕭?對蕭嚙踝蕭m
        mousePosition = lookAction.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        if (rb == null)
        {
            Debug.LogError("? Rigidbody 是 null，無法移動！");
            return;
        }
        
        // 每隔一段時間顯示當前速度值（調試用）
        if (Time.frameCount % 60 == 0 && moveInput.magnitude > 0.1f)
        {
            Debug.Log($"[HandleMovement] 當前 moveSpeed = {moveSpeed:F2}, moveInput = {moveInput}");
        }
        
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);

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
            Debug.LogWarning($"??桃恣???頧?憭望??: turret={turret != null}, camera={playerCamera != null}");
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePos = ray.GetPoint(distance);
            Vector3 direction = (worldMousePos - turret.position).normalized;
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
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

        // 備用方案：如果 firePoint 和 turret 都不存在，返回坦克本體位置
        return transform.position;
    }

    public Vector3 GetFireDirection()
    {
        return turret != null ? turret.forward : transform.forward;
    }

    public bool IsShootPressed()
    {
        // 判斷射擊按鈕是否被按下
        return shootAction.IsPressed();
    }

    // 處理繪製Gizmos
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
