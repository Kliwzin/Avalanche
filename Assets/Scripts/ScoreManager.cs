using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static int score;
    public TMP_Text scoreText;
    public static int linesCleared;

    [Header("Design")]
    [Tooltip("Dar 1 ponto por segundo só por estar vivo. Ver nota de design.")]
    [SerializeField] bool scorePerSecond = true;

    float timeCounter = 0f;
    int lastShownScore = -1;

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (scorePerSecond)
        {
            timeCounter += Time.deltaTime;

            while (timeCounter >= 1f)
            {
                score += 1;
                timeCounter -= 1f;
            }
        }

        if(score != lastShownScore)
        {
            lastShownScore = score;
            if (scoreText) scoreText.text = $"{score}";
        }
    }


    //lines = quantas linhas sumiram
    //chain = elo da cascata

    public static void AddLineScore(int lines, int chain = 1)
    {
        linesCleared += lines;

        int baseScore;

        switch (lines)
        {
            case 1: baseScore = 100; break;
            case 2: baseScore = 300; break;
            case 3: baseScore = 500; break;
            default: baseScore = 800; break;
        }

        score += baseScore * chain;
    }

    public static void AddDropScore()
    {
        score += 1;
    }
}
