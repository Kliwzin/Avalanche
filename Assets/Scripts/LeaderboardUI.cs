using TMPro;
using UnityEngine;
using System.Text;

public class LeaderboardUI : MonoBehaviour
{
    public TMP_Text leaderboardText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (leaderboardText == null) return;

        var sb = new StringBuilder();

        for (int i = 0; i < LeaderboardManager.MAX; i++)
        {
            var entry = LeaderboardManager.GetEntry(i);
            sb.AppendLine($"{i + 1}. {entry.name} — {entry.score}");
        }

        leaderboardText.text = sb.ToString().TrimEnd();
    }
}