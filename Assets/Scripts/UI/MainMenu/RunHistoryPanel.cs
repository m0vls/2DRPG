using TMPro;
using UnityEngine;

public class RunHistoryPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text teammateText;

    public void Populate(RunHistoryEntry entry, int currentUserId)
    {
        victoryText.text = entry.victory ? "Победа" : "Поражение";
        victoryText.color = entry.victory ? Color.green : Color.red;

        if (System.DateTime.TryParse(entry.created_at, out var dt))
            dateText.text = dt.ToString("yyyy-MM-dd HH:mm");
        else
            dateText.text = entry.created_at;

        int totalSeconds = Mathf.RoundToInt((float)entry.duration_seconds);
        durationText.text = $"Время: {totalSeconds % 3600 / 60:D2}:{totalSeconds % 60:D2}";

        killsText.text = "Кол-во убийств: " + entry.kills.ToString();
        levelText.text = "Уровень команды: " + entry.team_level.ToString();
        currencyText.text = "Заработанные монеты: " + entry.currency_earned.ToString();

        if (entry.user_id == currentUserId)
            teammateText.text = "Связанная душа: " + entry.teammate_nickname ?? "";
        else
            teammateText.text = "Связанная душа: " + entry.host_nickname ?? "";
    }
}
