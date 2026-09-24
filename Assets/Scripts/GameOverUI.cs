using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TextMeshProUGUI finalScoreText;

    bool saved = false;

    void OnEnable()
    {
        saved = false;

        if (finalScoreText != null)
            finalScoreText.text = $"{ScoreManager.score}";

        if (nameInput != null)
        {
            nameInput.text = "";
            nameInput.ActivateInputField();
        }
    }

    public void SaveScoreButton()
    {
        if (saved) return;

        string playerName = nameInput != null ? nameInput.text : "";
        LeaderboardManager.SaveScore(playerName, ScoreManager.score);

        saved = true;

        // atualiza o leaderboard
        LeaderboardUI ui = FindAnyObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
        if (ui != null) ui.Refresh();
    }
}