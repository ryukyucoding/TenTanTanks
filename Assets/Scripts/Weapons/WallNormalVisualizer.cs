using UnityEngine;

/// <summary>
/// 掛在牆壁上，可以視覺化顯示 6 個面的法線方向
/// 用來除錯牆壁的碰撞法線是否正確
/// </summary>
public class WallNormalVisualizer : MonoBehaviour
{
    [Header("視覺化設定")]
    [SerializeField] private bool showNormals = true;
    [SerializeField] private float normalLength = 1f;
    [SerializeField] private Color normalColor = Color.cyan;

    void OnDrawGizmos()
    {
        if (!showNormals) return;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        if (boxCollider != null)
        {
            // 取得 Box Collider 的中心（世界座標）
            Vector3 center = transform.TransformPoint(boxCollider.center);
            
            // 取得 Box Collider 的大小（考慮 Scale）
            Vector3 size = boxCollider.size;
            Vector3 scaledSize = new Vector3(
                size.x * transform.lossyScale.x,
                size.y * transform.lossyScale.y,
                size.z * transform.lossyScale.z
            );

            // 計算 6 個面的中心點
            Vector3 frontCenter = center + transform.forward * (scaledSize.z * 0.5f);
            Vector3 backCenter = center - transform.forward * (scaledSize.z * 0.5f);
            Vector3 rightCenter = center + transform.right * (scaledSize.x * 0.5f);
            Vector3 leftCenter = center - transform.right * (scaledSize.x * 0.5f);
            Vector3 topCenter = center + transform.up * (scaledSize.y * 0.5f);
            Vector3 bottomCenter = center - transform.up * (scaledSize.y * 0.5f);

            // 顯示 6 個面的法線
            Gizmos.color = normalColor;

            // 前面 (Front) - 藍色
            Gizmos.color = Color.blue;
            DrawNormalArrow(frontCenter, transform.forward, "Front +Z");

            // 後面 (Back) - 深藍色
            Gizmos.color = new Color(0, 0, 0.5f);
            DrawNormalArrow(backCenter, -transform.forward, "Back -Z");

            // 右面 (Right) - 紅色
            Gizmos.color = Color.red;
            DrawNormalArrow(rightCenter, transform.right, "Right +X");

            // 左面 (Left) - 深紅色
            Gizmos.color = new Color(0.5f, 0, 0);
            DrawNormalArrow(leftCenter, -transform.right, "Left -X");

            // 上面 (Top) - 綠色
            Gizmos.color = Color.green;
            DrawNormalArrow(topCenter, transform.up, "Top +Y");

            // 下面 (Bottom) - 深綠色
            Gizmos.color = new Color(0, 0.5f, 0);
            DrawNormalArrow(bottomCenter, -transform.up, "Bottom -Y");

            // 顯示中心點
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, 0.1f);
        }
        else
        {
            // 如果沒有 Box Collider，顯示警告
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    private void DrawNormalArrow(Vector3 position, Vector3 direction, string label)
    {
        // 畫法線箭頭
        Vector3 endPoint = position + direction * normalLength;
        Gizmos.DrawRay(position, direction * normalLength);

        // 畫箭頭尖端
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.1f;
        if (right == Vector3.zero) right = Vector3.Cross(direction, Vector3.right).normalized * 0.1f;
        
        Gizmos.DrawLine(endPoint, endPoint - direction * 0.2f + right);
        Gizmos.DrawLine(endPoint, endPoint - direction * 0.2f - right);

        // 在 Scene 視窗顯示標籤（需要 UnityEditor）
#if UNITY_EDITOR
        UnityEditor.Handles.Label(endPoint, label);
#endif
    }

    void OnDrawGizmosSelected()
    {
        // 被選中時顯示更多資訊
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Vector3 center = transform.TransformPoint(boxCollider.center);
            
            // 顯示 Transform 資訊
#if UNITY_EDITOR
            string info = $"Rotation: {transform.rotation.eulerAngles}\n" +
                         $"Scale: {transform.lossyScale}\n" +
                         $"Layer: {LayerMask.LayerToName(gameObject.layer)}";
            UnityEditor.Handles.Label(center + Vector3.up * 2f, info);
#endif
        }
    }
}
