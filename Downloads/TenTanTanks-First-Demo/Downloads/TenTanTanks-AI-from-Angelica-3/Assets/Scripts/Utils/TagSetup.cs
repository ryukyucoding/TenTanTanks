using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TagSetup : MonoBehaviour
{
    [System.Serializable]
    public class TagInfo
    {
        public string tagName;
        public string description;
    }

    [Header("Required Tags")]
    public TagInfo[] requiredTags = new TagInfo[]
    {
        new TagInfo { tagName = "Player", description = "Player tank" },
        new TagInfo { tagName = "Enemy", description = "Enemy tank" },
        new TagInfo { tagName = "Bullet", description = "Bullet projectile" },
        new TagInfo { tagName = "Mine", description = "Mine explosive" },
        new TagInfo { tagName = "Wall", description = "Wall obstacle" },
        new TagInfo { tagName = "Ground", description = "Ground surface" }
    };

#if UNITY_EDITOR
    [ContextMenu("Setup Tags")]
    public void SetupTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (var tagInfo in requiredTags)
        {
            bool tagExists = false;
            
            // 檢查標籤是否已存在
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagInfo.tagName))
                {
                    tagExists = true;
                    break;
                }
            }

            // 如果標籤不存在，添加它
            if (!tagExists)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
                newTagProp.stringValue = tagInfo.tagName;
                Debug.Log($"Added tag: {tagInfo.tagName} - {tagInfo.description}");
            }
            else
            {
                Debug.Log($"Tag already exists: {tagInfo.tagName}");
            }
        }

        tagManager.ApplyModifiedProperties();
        Debug.Log("Tag setup completed!");
    }
#endif
}
