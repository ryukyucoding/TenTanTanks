using UnityEngine;

/// <summary>
/// 超级简单的变形测试 - 按空格键测试
/// </summary>
public class SimpleTransformTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("========== SPACE 键按下 - 开始测试 ==========");
            TestTransformation();
        }
    }

    void TestTransformation()
    {
        // 1. 找玩家坦克
        GameObject player = GameManager.GetPlayerTank();
        if (player == null)
        {
            Debug.LogError("❌ 找不到玩家坦克！GameManager.GetPlayerTank() 返回 null");
            return;
        }
        
        Debug.Log($"✅ 找到玩家坦克: {player.name}");

        // 2. 检查 TankTransformationManager
        var transformManager = player.GetComponent<TankTransformationManager>();
        if (transformManager == null)
        {
            Debug.LogError("❌ 玩家坦克上没有 TankTransformationManager 组件！");
            Debug.Log("玩家坦克上的所有组件：");
            var allComponents = player.GetComponents<Component>();
            foreach (var comp in allComponents)
            {
                Debug.Log($"  - {comp.GetType().Name}");
            }
            return;
        }

        Debug.Log("✅ 找到 TankTransformationManager");

        // 3. 检查子物件
        Debug.Log($"\n当前玩家坦克的子物件 ({player.transform.childCount} 个):");
        for (int i = 0; i < player.transform.childCount; i++)
        {
            Transform child = player.transform.GetChild(i);
            Debug.Log($"  [{i}] {child.name} (Active: {child.gameObject.activeSelf})");
        }

        // 4. 尝试变形为 Heavy
        Debug.Log("\n🔄 调用 OnUpgradeSelected(\"Heavy\")...");
        try
        {
            transformManager.OnUpgradeSelected("Heavy");
            Debug.Log("✅ 变形方法调用成功！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 变形失败: {e.Message}\n{e.StackTrace}");
            return;
        }

        // 5. 等待一帧后检查结果
        StartCoroutine(CheckResultAfterFrame(player));
    }

    System.Collections.IEnumerator CheckResultAfterFrame(GameObject player)
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("\n========== 变形后的状态 ==========");
        Debug.Log($"玩家坦克的子物件 ({player.transform.childCount} 个):");
        for (int i = 0; i < player.transform.childCount; i++)
        {
            Transform child = player.transform.GetChild(i);
            Debug.Log($"  [{i}] {child.name} (Active: {child.gameObject.activeSelf})");
            
            // 显示子物件的子物件
            if (child.childCount > 0)
            {
                Debug.Log($"      └─ {child.childCount} 个子物件:");
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform grandChild = child.GetChild(j);
                    Debug.Log($"         [{j}] {grandChild.name}");
                }
            }
        }
        
        Debug.Log("========================================\n");
    }
}
