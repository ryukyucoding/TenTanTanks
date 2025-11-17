using UnityEngine;

/// <summary>
/// 用於在編輯器中視覺化反彈面
/// 只在 Gizmos 開啟時顯示
/// </summary>
public class BouncePlaneGizmo : MonoBehaviour
{
    [Header("視覺化設定")]
    public Vector3 planeSize = Vector3.one;
    public Color planeColor = new Color(0, 1, 0, 0.3f); // 綠色半透明
    public bool showWireframe = true;
    public bool showSolid = true;

    void OnDrawGizmos()
    {
        // 設置顏色
        Gizmos.color = planeColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        // 繪製半透明立方體
        if (showSolid)
        {
            Gizmos.color = new Color(planeColor.r, planeColor.g, planeColor.b, planeColor.a * 0.5f);
            Gizmos.DrawCube(Vector3.zero, planeSize);
        }

        // 繪製線框
        if (showWireframe)
        {
            Gizmos.color = new Color(planeColor.r, planeColor.g, planeColor.b, 1f);
            Gizmos.DrawWireCube(Vector3.zero, planeSize);
        }

        // 重置 matrix
        Gizmos.matrix = Matrix4x4.identity;
    }

    void OnDrawGizmosSelected()
    {
        // 選中時顯示更明顯的顏色
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, planeSize);
        Gizmos.matrix = Matrix4x4.identity;

        // 顯示法線方向
        Vector3 center = transform.position;
        Vector3 normal = transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(center, normal * 2f);

        // 在射線末端繪製箭頭
        DrawArrow(center, normal * 2f, 0.3f);
    }

    void DrawArrow(Vector3 start, Vector3 direction, float arrowHeadLength)
    {
        Vector3 end = start + direction;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;

        Gizmos.DrawRay(end, right * arrowHeadLength);
        Gizmos.DrawRay(end, left * arrowHeadLength);
    }
}
