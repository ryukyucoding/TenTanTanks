using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionMover : MonoBehaviour
{
    [SerializeField] float speed =  12;       // 移動速度（單位/秒）
    [SerializeField] float targetX = 12;     // 目標 X 座標
    [SerializeField] string nextScene = "Level1";

    void Update()
    {
        Vector3 pos = transform.position;

        if (pos.x < targetX)
        {
            // 朝 targetX 推進；用 MoveTowards 可避免超過
            float step = speed * Time.deltaTime;
            pos.x = Mathf.MoveTowards(pos.x, targetX, step);
            transform.position = pos;
        }
        else
        {
            LoadNext();
        }
    }

    void LoadNext()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        SceneManager.LoadScene(nextScene);
        enabled = false; // 防止重複呼叫
    }
}