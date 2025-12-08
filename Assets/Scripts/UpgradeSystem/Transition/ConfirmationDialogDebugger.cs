using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Debug script to inspect and fix confirmation dialog issues
/// Add this to your TransitionConfirmationDialog GameObject to debug
/// </summary>
public class ConfirmationDialogDebugger : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoFindComponents = true;

    private void Start()
    {
        if (showDebugInfo)
        {
            DebugDialogComponents();
        }

        if (autoFindComponents)
        {
            AutoAssignComponents();
        }
    }

    [ContextMenu("Debug Dialog Components")]
    public void DebugDialogComponents()
    {
        Debug.Log("=== CONFIRMATION DIALOG DEBUG ===");

        // Check if this GameObject has TransitionConfirmationDialog
        var dialogScript = GetComponent<TransitionConfirmationDialog>();
        if (dialogScript != null)
        {
            Debug.Log("TransitionConfirmationDialog script found");
        }
        else
        {
            Debug.LogError("TransitionConfirmationDialog script NOT found!");
        }

        // Check for SimpleTransitionDialog
        var simpleDialog = GetComponent<SimpleTransitionDialog>();
        if (simpleDialog != null)
        {
            Debug.Log("SimpleTransitionDialog script found");
        }

        // Check hierarchy structure
        Debug.Log("\n=== HIERARCHY STRUCTURE ===");
        LogChildren(transform, 0);

        // Check for UI components
        Debug.Log("\n=== UI COMPONENT CHECK ===");
        CheckUIComponents();

        // Check for Canvas
        var canvas = FindInParents<Canvas>(transform);
        Debug.Log($"Canvas in parents: {(canvas != null ? "Found" : "Not found")}");

        Debug.Log("=== END DEBUG ===");
    }

    [ContextMenu("Auto-Assign Components")]
    public void AutoAssignComponents()
    {
        var dialogScript = GetComponent<TransitionConfirmationDialog>();
        if (dialogScript == null)
        {
            Debug.LogError("No TransitionConfirmationDialog script to assign to!");
            return;
        }

        Debug.Log("=== AUTO-ASSIGNING COMPONENTS ===");

        // Try to find components by name
        var dialogPanel = FindChildByName("ContentPanel");
        if (dialogPanel == null)
            dialogPanel = transform; // Use this GameObject if ContentPanel not found

        var messageText = FindChildByName("Message")?.GetComponent<TextMeshProUGUI>();
        var upgradeNameText = FindChildByName("UpgradeName")?.GetComponent<TextMeshProUGUI>();
        var upgradeDescText = FindChildByName("UpgradeDescription")?.GetComponent<TextMeshProUGUI>();
        var yesButton = FindChildByName("Button_YES")?.GetComponent<Button>();
        var noButton = FindChildByName("Button_NO")?.GetComponent<Button>();

        Debug.Log($"Found DialogPanel: {dialogPanel?.name}");
        Debug.Log($"Found MessageText: {messageText?.name}");
        Debug.Log($"Found UpgradeNameText: {upgradeNameText?.name}");
        Debug.Log($"Found UpgradeDescText: {upgradeDescText?.name}");
        Debug.Log($"Found YesButton: {yesButton?.name}");
        Debug.Log($"Found NoButton: {noButton?.name}");

        // Use reflection to assign (since the fields are private)
        var dialogType = typeof(TransitionConfirmationDialog);

        SetPrivateField(dialogScript, "dialogPanel", dialogPanel?.gameObject);
        SetPrivateField(dialogScript, "messageText", messageText);
        SetPrivateField(dialogScript, "upgradeNameText", upgradeNameText);
        SetPrivateField(dialogScript, "upgradeDescriptionText", upgradeDescText);
        SetPrivateField(dialogScript, "confirmButton", yesButton);
        SetPrivateField(dialogScript, "cancelButton", noButton);

        Debug.Log("=== AUTO-ASSIGN COMPLETE ===");
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(obj, value);
            Debug.Log($"Assigned {fieldName}: {value?.ToString() ?? "null"}");
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found");
        }
    }

    private Transform FindChildByName(string name)
    {
        return FindInChildren(transform, name);
    }

    private Transform FindInChildren(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == name)
                return child;

            var found = FindInChildren(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    private T FindInParents<T>(Transform current) where T : Component
    {
        while (current != null)
        {
            var component = current.GetComponent<T>();
            if (component != null)
                return component;
            current = current.parent;
        }
        return null;
    }

    private void LogChildren(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}- {parent.name} ({parent.gameObject.activeInHierarchy})");

        for (int i = 0; i < parent.childCount; i++)
        {
            LogChildren(parent.GetChild(i), depth + 1);
        }
    }

    private void CheckUIComponents()
    {
        var images = GetComponentsInChildren<Image>();
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        var buttons = GetComponentsInChildren<Button>();

        Debug.Log($"Images found: {images.Length}");
        foreach (var img in images)
        {
            Debug.Log($"  - {img.name} (active: {img.gameObject.activeInHierarchy})");
        }

        Debug.Log($"Texts found: {texts.Length}");
        foreach (var txt in texts)
        {
            Debug.Log($"  - {txt.name}: '{txt.text}' (active: {txt.gameObject.activeInHierarchy})");
        }

        Debug.Log($"Buttons found: {buttons.Length}");
        foreach (var btn in buttons)
        {
            Debug.Log($"  - {btn.name} (active: {btn.gameObject.activeInHierarchy}, interactable: {btn.interactable})");
        }
    }

    [ContextMenu("Test Show Dialog")]
    public void TestShowDialog()
    {
        var dialogScript = GetComponent<TransitionConfirmationDialog>();
        if (dialogScript != null)
        {
            var testUpgrade = new WheelUpgradeSystem.WheelUpgradeOption("Test", "Test Description", 1);
            dialogScript.ShowDialog(testUpgrade,
                () => Debug.Log("TEST: Confirmed!"),
                () => Debug.Log("TEST: Cancelled!"));
        }
    }

    [ContextMenu("Test Hide Dialog")]
    public void TestHideDialog()
    {
        var dialogScript = GetComponent<TransitionConfirmationDialog>();
        if (dialogScript != null)
        {
            dialogScript.HideDialog();
        }
    }
}