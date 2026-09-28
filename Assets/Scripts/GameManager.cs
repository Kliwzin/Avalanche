using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static bool IsGameOver { get; private set; }

    public GameObject gameOverUI;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        IsGameOver = false;
        Time.timeScale = 1;

        GridManager.ResetGrid();
        ScoreManager.score = 0;
        ScoreManager.linesCleared = 0;

        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0;

        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    public void Restart()
    {
        IsGameOver = false;
        Time.timeScale = 1;
        ScoreManager.score = 0;
        ScoreManager.linesCleared = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}