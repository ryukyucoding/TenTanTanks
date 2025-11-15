using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeWheelCloser : MonoBehaviour
{
    [Header("Close Settings")]
    [SerializeField] private UpgradeWheelUI upgradeWheel;
    [SerializeField] private bool closeOnEscape = true;
    [SerializeField] private bool closeOnBackgroundClick = true;

    void Update()
    {
        // ESC 鍵關閉
        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUpgradeWheel();
        }
    }

    void Start()
    {
        // 自動尋找 UpgradeWheelUI
        if (upgradeWheel == null)
        {
            upgradeWheel = GetComponent<UpgradeWheelUI>();
        }

        // 為背景添加點擊事件
        if (closeOnBackgroundClick)
        {
            SetupBackgroundClick();
        }
    }

    private void SetupBackgroundClick()
    {
        // 找到 BlurBackground
        var blurBackground = transform.GetComponentInChildren<UnityEngine.UI.Image>();
        if (blurBackground != null && blurBackground.name.Contains("Blur"))
        {
            // 添加 EventTrigger
            EventTrigger trigger = blurBackground.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = blurBackground.gameObject.AddComponent<EventTrigger>();
            }

            // 添加點擊事件
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { CloseUpgradeWheel(); });
            trigger.triggers.Add(entry);

            Debug.Log("Background click handler added");
        }
    }

    public void CloseUpgradeWheel()
    {
        if (upgradeWheel != null)
        {
            upgradeWheel.HideWheel();
            Debug.Log("Upgrade wheel closed");
        }
        else
        {
            // 如果找不到 UpgradeWheelUI，直接隱藏物件
            gameObject.SetActive(false);
        }
    }
}
