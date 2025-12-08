using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Boot scene manager that loads all persistent systems and then loads Menu scene
/// Ensures proper initialization order: Boot ¡÷ Menu ¡÷ Game
/// </summary>
public class BootSceneManager : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private float loadDelay = 0.5f; // Short delay to ensure all persistent objects are created

    void Start()
    {
        Debug.Log("[BootSceneManager] Boot scene started - initializing persistent systems...");

        // Give persistent objects time to initialize
        Invoke("LoadMenuScene", loadDelay);
    }

    private void LoadMenuScene()
    {
        Debug.Log($"[BootSceneManager] Loading menu scene: {menuSceneName}");
        SceneManager.LoadScene(menuSceneName);
    }
}