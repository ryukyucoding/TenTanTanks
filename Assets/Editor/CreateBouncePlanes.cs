using UnityEngine;
using UnityEditor;

/// <summary>
/// 自動為牆壁創建反彈面的工具
/// 會在選中的牆壁外側創建薄的平面作為反彈面
/// </summary>
public class CreateBouncePlanes : EditorWindow
{
    private float planeThickness = 0.05f;
    private float planeOffset = 0.01f; // 稍微偏移避免 Z-fighting

    [MenuItem("Tools/Create Bounce Planes for Walls")]
    static void ShowWindow()
    {
        GetWindow<CreateBouncePlanes>("創建反彈面");
    }

    void OnGUI()
    {
        GUILayout.Label("為牆壁自動創建反彈面", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("這個工具會在牆壁外側創建薄的「反彈面」");
        GUILayout.Label("子彈只會在這些面上反彈，不會碰到內側面");
        GUILayout.Space(10);

        planeThickness = EditorGUILayout.FloatField("反彈面厚度", planeThickness);
        planeOffset = EditorGUILayout.FloatField("反彈面偏移", planeOffset);
        GUILayout.Space(10);

        GUILayout.Label("使用方法：", EditorStyles.label);
        GUILayout.Label("1. 選擇一組牆壁小方塊（例如：整面北牆）");
        GUILayout.Label("2. 點擊下方按鈕");
        GUILayout.Label("3. 選擇要創建反彈面的方向");
        GUILayout.Space(10);

        if (GUILayout.Button("創建 +X 方向反彈面（東側）", GUILayout.Height(35)))
        {
            CreateBouncePlaneInDirection(Vector3.right);
        }

        if (GUILayout.Button("創建 -X 方向反彈面（西側）", GUILayout.Height(35)))
        {
            CreateBouncePlaneInDirection(Vector3.left);
        }

        if (GUILayout.Button("創建 +Z 方向反彈面（北側）", GUILayout.Height(35)))
        {
            CreateBouncePlaneInDirection(Vector3.forward);
        }

        if (GUILayout.Button("創建 -Z 方向反彈面（南側）", GUILayout.Height(35)))
        {
            CreateBouncePlaneInDirection(Vector3.back);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("自動創建所有外側反彈面（智能）", GUILayout.Height(40)))
        {
            CreateAllOuterBouncePlanes();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("列出所有反彈面（查看設定）", GUILayout.Height(30)))
        {
            ListAllBouncePlanes();
        }

        if (GUILayout.Button("移除所有反彈面", GUILayout.Height(30)))
        {
            RemoveAllBouncePlanes();
        }
    }

    void CreateBouncePlaneInDirection(Vector3 direction)
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("錯誤", "請先選擇要處理的牆壁方塊！", "確定");
            return;
        }

        // 計算所有選中物件的邊界
        Bounds totalBounds = CalculateTotalBounds(selectedObjects);

        // 創建反彈面容器
        GameObject container = new GameObject($"BouncePlane_{DirectionToString(direction)}");
        Undo.RegisterCreatedObjectUndo(container, "Create Bounce Plane");

        // 計算反彈面的位置和大小
        Vector3 planePosition = totalBounds.center;
        Vector3 planeSize = totalBounds.size;

        // 根據方向調整位置和大小
        if (direction == Vector3.right || direction == Vector3.left)
        {
            // X 方向的面
            planePosition.x = direction == Vector3.right ? totalBounds.max.x : totalBounds.min.x;
            planePosition.x += direction.x * planeOffset;
            planeSize = new Vector3(planeThickness, planeSize.y, planeSize.z);
        }
        else if (direction == Vector3.forward || direction == Vector3.back)
        {
            // Z 方向的面
            planePosition.z = direction == Vector3.forward ? totalBounds.max.z : totalBounds.min.z;
            planePosition.z += direction.z * planeOffset;
            planeSize = new Vector3(planeSize.x, planeSize.y, planeThickness);
        }

        container.transform.position = planePosition;

        // 添加 Box Collider（這就是反彈面）
        BoxCollider bounceCollider = container.AddComponent<BoxCollider>();
        bounceCollider.size = planeSize;
        bounceCollider.center = Vector3.zero;

        // 設置為 Wall Layer
        container.layer = LayerMask.NameToLayer("Wall");

        // 添加標記（可選，用於識別這是反彈面）
        container.tag = "Wall";

        // 添加 Gizmos 視覺化腳本（只在編輯器 Gizmos 開啟時顯示）
        BouncePlaneGizmo gizmo = container.AddComponent<BouncePlaneGizmo>();
        gizmo.planeSize = planeSize;
        gizmo.planeColor = new Color(0, 1, 0, 0.3f); // 綠色半透明

        Debug.Log($"✅ 已創建反彈面：{container.name}，方向：{direction}，大小：{planeSize}");
    }

    void CreateAllOuterBouncePlanes()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("錯誤", "請先選擇要處理的牆壁方塊！", "確定");
            return;
        }

        // 計算總邊界
        Bounds totalBounds = CalculateTotalBounds(selectedObjects);

        // 分析哪些面是外側面
        bool hasRightFace = false, hasLeftFace = false;
        bool hasFrontFace = false, hasBackFace = false;

        foreach (GameObject obj in selectedObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Bounds bounds = renderer.bounds;

            // 檢查是否在邊界上
            if (Mathf.Abs(bounds.max.x - totalBounds.max.x) < 0.1f) hasRightFace = true;
            if (Mathf.Abs(bounds.min.x - totalBounds.min.x) < 0.1f) hasLeftFace = true;
            if (Mathf.Abs(bounds.max.z - totalBounds.max.z) < 0.1f) hasFrontFace = true;
            if (Mathf.Abs(bounds.min.z - totalBounds.min.z) < 0.1f) hasBackFace = true;
        }

        // 創建檢測到的外側面
        int createdCount = 0;
        if (hasRightFace) { CreateBouncePlaneInDirection(Vector3.right); createdCount++; }
        if (hasLeftFace) { CreateBouncePlaneInDirection(Vector3.left); createdCount++; }
        if (hasFrontFace) { CreateBouncePlaneInDirection(Vector3.forward); createdCount++; }
        if (hasBackFace) { CreateBouncePlaneInDirection(Vector3.back); createdCount++; }

        Debug.Log($"✅ 智能創建了 {createdCount} 個反彈面");
    }

    void ListAllBouncePlanes()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;

        Debug.Log("========== 反彈面列表 ==========");

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("BouncePlane_"))
            {
                count++;
                BoxCollider collider = obj.GetComponent<BoxCollider>();
                BouncePlaneGizmo gizmo = obj.GetComponent<BouncePlaneGizmo>();

                string info = $"[{count}] {obj.name}\n";
                info += $"    位置: {obj.transform.position}\n";
                info += $"    大小: {(collider != null ? collider.size.ToString() : "無")}";
                if (gizmo != null)
                {
                    info += $"\n    視覺化: 顏色={gizmo.planeColor}, 線框={gizmo.showWireframe}, 實體={gizmo.showSolid}";
                }
                info += $"\n    Layer: {LayerMask.LayerToName(obj.layer)}";

                Debug.Log(info);
            }
        }

        if (count == 0)
        {
            Debug.LogWarning("場景中沒有反彈面！");
        }
        else
        {
            Debug.Log($"========== 共 {count} 個反彈面 ==========");
        }
    }

    void RemoveAllBouncePlanes()
    {
        if (!EditorUtility.DisplayDialog("確認", "確定要移除所有以 'BouncePlane_' 開頭的物件嗎？", "確定", "取消"))
            return;

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("BouncePlane_"))
            {
                Undo.DestroyObjectImmediate(obj);
                removedCount++;
            }
        }

        Debug.Log($"✅ 已移除 {removedCount} 個反彈面");
    }

    Bounds CalculateTotalBounds(GameObject[] objects)
    {
        Bounds bounds = new Bounds(objects[0].transform.position, Vector3.zero);

        foreach (GameObject obj in objects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            else
            {
                bounds.Encapsulate(obj.transform.position);
            }
        }

        return bounds;
    }

    string DirectionToString(Vector3 direction)
    {
        if (direction == Vector3.right) return "East";
        if (direction == Vector3.left) return "West";
        if (direction == Vector3.forward) return "North";
        if (direction == Vector3.back) return "South";
        return "Unknown";
    }
}
