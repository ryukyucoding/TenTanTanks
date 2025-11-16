using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;      // ï¿½Zï¿½Jï¿½ï¿½ï¿½ï¿½ï¿½]ï¿½ï¿½ï¿½Ê®É·|ï¿½ï¿½ï¿½ï¿½^
    [SerializeField] private Transform turret;        // ï¿½ï¿½ï¿½ï¿½]ï¿½ï¿½ï¿½Hï¿½Æ¹ï¿½ï¿½ï¿½ï¿½ï¿½^
    [SerializeField] private Transform firePoint;     // ï¿½ï¿½ï¿½uï¿½oï¿½gï¿½I

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;     // ï¿½ï¿½ï¿½aï¿½á¹³ï¿½ï¿½

    // ï¿½ï¿½Jï¿½Ü¼ï¿½
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Vector2 lookInput;

    // ï¿½Õ¥ï¿½Þ¥ï¿?
    private Rigidbody rb;
    private PlayerInput playerInput;

    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;

    void Awake()
    {
        // ï¿½ï¿½ï¿½ï¿½Õ¥ï¿?
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        // ï¿½pï¿½Gï¿½Sï¿½ï¿½ï¿½ï¿½ï¿½wï¿½á¹³ï¿½ï¿½ï¿½Aï¿½ï¿½ï¿½Õ§ï¿½ï¿½Dï¿½á¹³ï¿½ï¿½
        if (playerCamera == null)
            playerCamera = Camera.main;

        // ï¿½]ï¿½mRigidbodyï¿½]ï¿½ï¿½ï¿½ï¿½Â½ï¿½uï¿½^
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        // ï¿½jï¿½wï¿½ï¿½Jï¿½Æ¥ï¿½
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Attack"];

        moveAction.Enable();
        lookAction.Enable();
        shootAction.Enable();
    }

    void OnDisable()
    {
        // ï¿½Ñ°ï¿½ï¿½jï¿½w
        moveAction?.Disable();
        lookAction?.Disable();
        shootAction?.Disable();
    }

    void Update()
    {
        // Åªï¿½ï¿½ï¿½ï¿½J
        HandleInput();

        // ï¿½Bï¿½zï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½]ï¿½Cï¿½Vï¿½ï¿½sï¿½^
        HandleTurretRotation();
    }

    void FixedUpdate()
    {
        // ï¿½Bï¿½zï¿½ï¿½ï¿½Ê¡]ï¿½ï¿½ï¿½zï¿½ï¿½sï¿½^
        HandleMovement();
    }

    private void HandleInput()
    {
        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ê¿ï¿½J
        moveInput = moveAction.ReadValue<Vector2>();
        // ï¿½ï¿½ï¿½ï¿½Ë·ï¿?
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        // ï¿½ï¿½ï¿½ï¿½?¹ï¿½ï¿½ï¿½m
        mousePosition = lookAction.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        // ï¿½pï¿½â²¾ï¿½Ê¤ï¿½V
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // ï¿½ï¿½ï¿½Î²ï¿½ï¿½ï¿½
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);

        // ï¿½pï¿½Gï¿½ï¿½ï¿½ï¿½ï¿½Ê¿ï¿½Jï¿½Aï¿½ï¿½ï¿½à¨®ï¿½ï¿½ï¿½Â¦Vï¿½ï¿½ï¿½Ê¤ï¿½V
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
            Debug.LogWarning($"??®ç®¡???è½?å¤±æ??: turret={turret != null}, camera={playerCamera != null}");
            return;
        }

        // ï¿½Ï¥Î·Æ¹ï¿½ï¿½Ã¹ï¿½ï¿½ï¿½mï¿½à´«ï¿½ï¿½ï¿½@ï¿½É®yï¿½ï¿½
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePos = ray.GetPoint(distance);
            Vector3 direction = (worldMousePos - turret.position).normalized;
            direction.y = 0; // ï¿½Oï¿½ï¿½ï¿½ï¿½ï¿½ï¿½

            if (direction.magnitude > 0.1f)
            {
                // ï¿½pï¿½ï¿½Ø¼Ð±ï¿½ï¿½à¨¤ï¿½ï¿?
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // ï¿½ï¿½ï¿½Æ±ï¿½ï¿½ï¿½ï¿½Ø¼Ð¨ï¿½ï¿½ï¿½
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

        // ï¿½pï¿½Gï¿½Sï¿½ï¿½ï¿½]ï¿½wfirePointï¿½Aï¿½Nï¿½ï¿½turretï¿½ï¿½ï¿½ï¿½m
        if (turret != null)
        {
            return turret.position;
        }

        // ï¿½Ì«ï¿½?®×¡Gï¿½Î©Zï¿½Jï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½m
        return transform.position;
    }

    public Vector3 GetFireDirection()
    {
        return turret != null ? turret.forward : transform.forward;
    }

    public bool IsShootPressed()
    {
        // ??¹ç?ºæª¢??¥æ???????¯å?¦æ??ä½?ï¼????ä¸???¯å?®æ¬¡???ä¸?
        // ???æ¨???¯ä»¥??¯æ?´æ??ä½?å·¦é?µé??çº?å°????ï¼?ä¸?ä¸??????»æ??ç§»å??
        return shootAction.IsPressed();
    }

    // ï¿½ï¿½ï¿½ï¿½ï¿½Îªï¿½Gizmos
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
