using UnityEngine;

public static class LeaderboardManager
{
    public const int MAX = 5;

    public static void SaveScore(string name, int score)
    {
        // carrega ranking atual
        int[] scores = new int[MAX];
        string[] names = new string[MAX];

        for (int i = 0; i < MAX; i++)
        {
            scores[i] = PlayerPrefs.GetInt("Score" + i, 0);
            names[i] = PlayerPrefs.GetString("Name" + i, "---");
        }

        // acha posição de inserção
        int insertIndex = -1;
        for (int i = 0; i < MAX; i++)
        {
            if (score > scores[i])
            {
                insertIndex = i;
                break;
            }
        }

        // se não entra no top 5, não salva
        if (insertIndex == -1) return;

        // shift pra baixo
        for (int i = MAX - 1; i > insertIndex; i--)
        {
            scores[i] = scores[i - 1];
            names[i] = names[i - 1];
        }

        // insere novo
        scores[insertIndex] = score;
        names[insertIndex] = string.IsNullOrWhiteSpace(name) ? "---" : name;

        // grava
        for (int i = 0; i < MAX; i++)
        {
            PlayerPrefs.SetInt("Score" + i, scores[i]);
            PlayerPrefs.SetString("Name" + i, names[i]);
        }

        PlayerPrefs.Save();
    }

    public static (string name, int score) GetEntry(int index)
    {
        string n = PlayerPrefs.GetString("Name" + index, "---");
        int s = PlayerPrefs.GetInt("Score" + index, 0);
        return (n, s);
    }
}