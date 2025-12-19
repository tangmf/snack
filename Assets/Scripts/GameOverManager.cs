using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI")]
    public GameObject gameOverUI;   // Assign your Game Over Canvas root

    bool isGameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    /// <summary>
    /// Call once when the player dies.
    /// </summary>
    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over");

        Time.timeScale = 0f;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }

    /// <summary>
    /// Called by the Restart button OnClick().
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
