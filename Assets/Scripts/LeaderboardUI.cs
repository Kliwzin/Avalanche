using TMPro;
using UnityEngine;
using System.Text;

public class LeaderboardUI : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (leaderboardText == null) return;

        var sb = new StringBuilder();

        for (int i = 0; i < 5; i++)
        {
            var entry = LeaderboardManager.GetEntry(i);
            sb.AppendLine($"{i + 1}. {entry.name} — {entry.score}");
        }

        leaderboardText.text = sb.ToString();
    }
}