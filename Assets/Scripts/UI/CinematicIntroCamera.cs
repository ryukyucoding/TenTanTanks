using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the cinematic camera intro sequence.
/// Camera spirals down from a high position to focus on the target (player tank).
/// </summary>
public class CinematicIntroCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The transform to focus on (usually the player's tank)")]
    public Transform target;

    [Header("Height Settings")]
    [Tooltip("Starting height above the target")]
    public float startHeight = 20f;

    [Tooltip("Ending height above the target")]
    public float endHeight = 5f;

    [Header("Radius Settings")]
    [Tooltip("Starting distance from the target (horizontal)")]
    public float startRadius = 15f;

    [Tooltip("Ending distance from the target (horizontal)")]
    public float endRadius = 8f;

    [Header("Animation Settings")]
    [Tooltip("Duration of the entire intro sequence in seconds")]
    public float duration = 4f;

    [Tooltip("Number of complete rotations during the spiral")]
    public float rotationSpeed = 2f;

    [Header("Look At Settings")]
    [Tooltip("Camera look at offset (Y axis). Higher = tank appears lower in frame, Lower = tank appears higher")]
    public float lookAtHeightOffset = 2f;

    [Tooltip("Additional camera height offset at end position. Negative = lower camera = more upward angle")]
    public float cameraHeightAdjustment = 0f;

    [Header("Events")]
    [Tooltip("Triggered when the intro animation completes")]
    public UnityEvent OnIntroFinished;

    // Private variables
    private float elapsedTime = 0f;
    private bool isPlaying = false;
    private Vector3 initialTargetPosition;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CinematicIntroCamera: No target assigned! Please assign the tank transform.");
            enabled = false;
            return;
        }

        // Store initial target position
        initialTargetPosition = target.position;

        // Start the intro sequence
        StartIntro();
    }

    private void Update()
    {
        if (!isPlaying) return;

        // Update elapsed time
        elapsedTime += Time.deltaTime;

        // Calculate progress (0 to 1)
        float t = Mathf.Clamp01(elapsedTime / duration);

        // Smooth progress using ease-in-out
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // Calculate current height
        float currentHeight = Mathf.Lerp(startHeight, endHeight, smoothT);

        // Calculate current radius
        float currentRadius = Mathf.Lerp(startRadius, endRadius, smoothT);

        // Calculate rotation angle (in radians)
        float angle = t * rotationSpeed * 2f * Mathf.PI;

        // Calculate camera position in a spiral
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * currentRadius,
            currentHeight + cameraHeightAdjustment,
            Mathf.Sin(angle) * currentRadius
        );

        // Position camera relative to target
        transform.position = target.position + offset;

        // Always look at the target (with adjustable height offset)
        transform.LookAt(target.position + Vector3.up * lookAtHeightOffset);

        // Check if animation is complete
        if (t >= 1f)
        {
            FinishIntro();
        }
    }

    /// <summary>
    /// Starts the cinematic intro sequence
    /// </summary>
    public void StartIntro()
    {
        elapsedTime = 0f;
        isPlaying = true;
    }

    /// <summary>
    /// Finishes the intro and triggers the completion event
    /// </summary>
    private void FinishIntro()
    {
        isPlaying = false;
        OnIntroFinished?.Invoke();
    }

    /// <summary>
    /// Skips the intro animation and goes directly to the end position
    /// </summary>
    public void SkipIntro()
    {
        if (!isPlaying) return;

        elapsedTime = duration;
        isPlaying = false;

        // Set final position
        float angle = rotationSpeed * 2f * Mathf.PI;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * endRadius,
            endHeight + cameraHeightAdjustment,
            Mathf.Sin(angle) * endRadius
        );

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * lookAtHeightOffset);

        OnIntroFinished?.Invoke();
    }

    // Gizmos for visualization in the editor
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw start position
        Gizmos.color = Color.green;
        Vector3 startPos = target.position + new Vector3(startRadius, startHeight, 0);
        Gizmos.DrawWireSphere(startPos, 0.5f);

        // Draw end position
        Gizmos.color = Color.red;
        float endAngle = rotationSpeed * 2f * Mathf.PI;
        Vector3 endPos = target.position + new Vector3(
            Mathf.Cos(endAngle) * endRadius,
            endHeight,
            Mathf.Sin(endAngle) * endRadius
        );
        Gizmos.DrawWireSphere(endPos, 0.5f);

        // Draw spiral path
        Gizmos.color = Color.yellow;
        int segments = 50;
        Vector3 prevPos = startPos;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentHeight = Mathf.Lerp(startHeight, endHeight, smoothT);
            float currentRadius = Mathf.Lerp(startRadius, endRadius, smoothT);
            float angle = t * rotationSpeed * 2f * Mathf.PI;

            Vector3 currentPos = target.position + new Vector3(
                Mathf.Cos(angle) * currentRadius,
                currentHeight,
                Mathf.Sin(angle) * currentRadius
            );

            Gizmos.DrawLine(prevPos, currentPos);
            prevPos = currentPos;
        }

        // Draw target
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position, 1f);
    }
}
