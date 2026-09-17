using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static int score;
    public TextMeshProUGUI scoreText;

    float timeCounter = 0f;

    void Update()
    {
        if (Time.timeScale == 0) return;

        timeCounter += Time.deltaTime;

        if (timeCounter >= 1f)
        {
            score += 1;
            timeCounter = 0f;
        }

        scoreText.text = $"{score}";
    }

    public static void AddLineScore()
    {
        score += 100;
    }

    public static void AddDropScore()
    {
        score += 1;
    }
}
