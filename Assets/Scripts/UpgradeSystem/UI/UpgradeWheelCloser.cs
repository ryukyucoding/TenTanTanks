using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UpgradeWheelCloser : MonoBehaviour
{
    [Header("Close Settings")]
    [SerializeField] private UpgradeWheelUI upgradeWheel;
    [SerializeField] private bool closeOnEscape = true;
    [SerializeField] private bool closeOnBackgroundClick = true;

    [Header("Click Detection")]
    [SerializeField] private GraphicRaycaster canvasRaycaster;
    [SerializeField] private LayerMask wheelAreaLayer = -1; // What layers count as "wheel area"

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private bool isWheelOpen = false;
    private Camera uiCamera;

    void Start()
    {
        // Auto-find UpgradeWheelUI
        if (upgradeWheel == null)
        {
            upgradeWheel = GetComponent<UpgradeWheelUI>();
            if (upgradeWheel == null)
                upgradeWheel = FindObjectOfType<UpgradeWheelUI>();
        }

        // Auto-find canvas raycaster
        if (canvasRaycaster == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        // Find UI camera
        uiCamera = Camera.main;
        if (uiCamera == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.worldCamera != null)
                uiCamera = canvas.worldCamera;
        }

        DebugLog("UpgradeWheelCloser initialized");
    }

    void Update()
    {
        // Update wheel open state
        UpdateWheelState();

        // ESC key to close - using new Input System
        if (closeOnEscape && isWheelOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseUpgradeWheel();
        }

        // Background click to close - using new Input System
        if (closeOnBackgroundClick && isWheelOpen && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckBackgroundClick();
        }
    }

    private void UpdateWheelState()
    {
        if (upgradeWheel != null)
        {
            // Check if the wheel canvas is active
            var canvas = upgradeWheel.GetComponent<Canvas>();
            if (canvas != null)
            {
                isWheelOpen = canvas.gameObject.activeInHierarchy;
            }
            else
            {
                // Fallback: check if the gameObject is active
                isWheelOpen = upgradeWheel.gameObject.activeInHierarchy;
            }
        }
        else
        {
            isWheelOpen = false;
        }
    }

    private void CheckBackgroundClick()
    {
        if (canvasRaycaster == null || uiCamera == null)
        {
            DebugLog("Canvas raycaster or UI camera not found - using fallback method");
            CheckBackgroundClickFallback();
            return;
        }

        // Get mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Create pointer event data
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        // Raycast to see what was clicked
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        canvasRaycaster.Raycast(pointerEventData, raycastResults);

        bool clickedOnWheel = false;

        // Check if any of the hit objects are part of the wheel
        foreach (var result in raycastResults)
        {
            if (IsWheelAreaObject(result.gameObject))
            {
                clickedOnWheel = true;
                DebugLog($"Clicked on wheel area object: {result.gameObject.name}");
                break;
            }
        }

        if (!clickedOnWheel)
        {
            DebugLog("Clicked outside wheel area - closing wheel");
            CloseUpgradeWheel();
        }
    }

    private void CheckBackgroundClickFallback()
    {
        // Fallback method: check if click is within wheel container bounds
        if (upgradeWheel == null) return;

        // Try to find the wheel container
        var wheelContainer = upgradeWheel.transform.Find("WheelContainer");
        if (wheelContainer == null)
        {
            // Look for any child with "wheel" or "container" in the name
            for (int i = 0; i < upgradeWheel.transform.childCount; i++)
            {
                var child = upgradeWheel.transform.GetChild(i);
                if (child.name.ToLower().Contains("wheel") || child.name.ToLower().Contains("container"))
                {
                    wheelContainer = child;
                    break;
                }
            }
        }

        if (wheelContainer != null)
        {
            // Check if mouse is within the wheel container bounds
            var rectTransform = wheelContainer.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector2 localMousePosition;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, mousePosition, uiCamera, out localMousePosition))
                {
                    if (!rectTransform.rect.Contains(localMousePosition))
                    {
                        DebugLog("Clicked outside wheel container bounds - closing wheel");
                        CloseUpgradeWheel();
                    }
                    else
                    {
                        DebugLog("Clicked inside wheel container bounds");
                    }
                }
            }
        }
    }

    private bool IsWheelAreaObject(GameObject obj)
    {
        if (obj == null || upgradeWheel == null) return false;

        // Check if the object is a child of the upgrade wheel
        Transform current = obj.transform;
        while (current != null)
        {
            if (current == upgradeWheel.transform)
                return true;

            // Special cases: objects that should close the wheel even if they're children
            if (IsCloseableObject(current.gameObject))
                return false;

            current = current.parent;
        }

        return false;
    }

    private bool IsCloseableObject(GameObject obj)
    {
        // Objects that should trigger close even if they're part of the wheel
        string objName = obj.name.ToLower();

        // Background blur image should trigger close
        if (objName.Contains("blur") && objName.Contains("background"))
            return true;

        // Close button should not trigger close (handled separately)
        if (objName.Contains("close") && objName.Contains("button"))
            return false;

        // Confirm button should not trigger close
        if (objName.Contains("confirm") && objName.Contains("button"))
            return false;

        // Upgrade buttons should not trigger close
        if (objName.Contains("upgrade") && objName.Contains("button"))
            return false;

        return false;
    }

    public void CloseUpgradeWheel()
    {
        if (upgradeWheel != null)
        {
            upgradeWheel.HideWheel();
            DebugLog("Upgrade wheel closed via UpgradeWheelCloser");
        }
        else
        {
            // If no UpgradeWheelUI found, try to deactivate this object
            gameObject.SetActive(false);
            DebugLog("Upgrade wheel closed by deactivating gameObject");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[UpgradeWheelCloser] {message}");
    }

    // Public method to manually set wheel open state (called from UpgradeWheelUI if needed)
    public void SetWheelOpen(bool open)
    {
        isWheelOpen = open;
        DebugLog($"Wheel state manually set to: {(open ? "Open" : "Closed")}");
    }

    // Context menu for testing
    [ContextMenu("Test Close Wheel")]
    public void TestCloseWheel()
    {
        CloseUpgradeWheel();
    }

    [ContextMenu("Check Components")]
    public void CheckComponents()
    {
        Debug.Log("=== UpgradeWheelCloser Component Check ===");
        Debug.Log($"UpgradeWheelUI: {(upgradeWheel != null ? "y" : "n")}");
        Debug.Log($"GraphicRaycaster: {(canvasRaycaster != null ? "y" : "n")}");
        Debug.Log($"UI Camera: {(uiCamera != null ? "y" : "n")}");
        Debug.Log($"Wheel Open: {isWheelOpen}");
    }
}