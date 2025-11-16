using UnityEngine;

/// <summary>
/// 精簡後版本：原本用來自動設定 LevelManager 的腳本，現在已不再使用 LevelManager。
/// 保留一個空的 MonoBehaviour 以避免編譯錯誤，也避免場景上掛著舊組件報錯。
/// </summary>
public class AutoLevelSetup : MonoBehaviour
{
    // 目前不做任何事，如果場景裡還掛著這個元件，也不會影響遊戲。
}
