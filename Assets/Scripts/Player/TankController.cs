using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;

    [Header("Tank Parts")]
    [SerializeField] private Transform tankBody;      // �Z�J�����]���ʮɷ|����^
    [SerializeField] private Transform turret;        // ����]���H�ƹ�����^
    [SerializeField] private Transform firePoint;     // ���u�o�g�I

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;     // ���a�ṳ��

    // ��J�ܼ�
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Vector2 lookInput;

    // �ե�ޥ�
    private Rigidbody rb;
    private PlayerInput playerInput;

    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;

    void Awake()
    {
        // ����ե�
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        // �p�G�S�����w�ṳ���A���է��D�ṳ��
        if (playerCamera == null)
            playerCamera = Camera.main;

        // �]�mRigidbody�]����½�u�^
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        // �j�w��J�ƥ�
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Attack"];

        moveAction.Enable();
        lookAction.Enable();
        shootAction.Enable();
    }

    void OnDisable()
    {
        // �Ѱ��j�w
        moveAction?.Disable();
        lookAction?.Disable();
        shootAction?.Disable();
    }

    void Update()
    {
        // Ū����J
        HandleInput();

        // �B�z�������]�C�V��s�^
        HandleTurretRotation();
    }

    void FixedUpdate()
    {
        // �B�z���ʡ]���z��s�^
        HandleMovement();
    }

    private void HandleInput()
    {
        // ������ʿ�J
        moveInput = moveAction.ReadValue<Vector2>();
        // ����˷�
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        // ����ƹ���m
        mousePosition = lookAction.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        // �p�Ⲿ�ʤ�V
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // ���β���
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);

        // �p�G�����ʿ�J�A���ਮ���¦V���ʤ�V
        if (moveDirection.magnitude > 0.1f && tankBody != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            tankBody.rotation = Quaternion.Slerp(tankBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleTurretRotation()
    {
        if (turret == null) return;

        // �ϥ�InputSystem��Look delta
        float rotationAmount = lookInput.x * rotationSpeed * Time.deltaTime;
        turret.Rotate(0, rotationAmount, 0);

        // Debug�ݼƭ�
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

    // �����Ϊ�Gizmos
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
