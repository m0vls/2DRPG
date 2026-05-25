using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

public struct RunEndMessage : NetworkMessage
{
    public float durationSeconds;
    public int kills;
    public bool victory;
    public int currencyEarned;
    public int teamLevel;
    public int teammateId;
}

public class TeamStateManager : NetworkBehaviour
{
    public static TeamStateManager Instance;

    [Header("Shared HP")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float teamHealth = 100;

    [Header("Shared XP")]
    [SyncVar(hook = nameof(OnLevelChanged))] public int teamLevel = 1;
    [SyncVar(hook = nameof(OnXPChanged))] public float teamXP = 0f;
    [SyncVar] public float xpToNextLevel = 100f;

    [SerializeField] private float xpMultiplierPerLevel = 1.5f;

    [Header("Shared Currency")]
    [SyncVar(hook = nameof(OnCurrencyChanged))]
    public int teamCurrency = 0;

    private int totalKillsThisRun = 0;
    private float runStartTime;
    private bool isGameOver = false;

    public void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        RPGNetworkManager netManager = (RPGNetworkManager)NetworkManager.singleton;
        teamHealth = netManager.teamHealth;
        isGameOver = false;
        teamCurrency = 0;
        totalKillsThisRun = 0;
        runStartTime = Time.time;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<RunEndMessage>(OnRunEndMessage);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(teamHealth);
            UIManager.Instance.UpdateXPBarUI(teamXP, xpToNextLevel);
            UIManager.Instance.UpdateCurrencyUI(teamCurrency);
        }
    }

    [Server]
    public void TakeTeamDamage(float damageAmount)
    {
        if (isGameOver) return;

        Debug.Log($"Нанесено урона: {damageAmount}");

        teamHealth -= damageAmount;

        RPGNetworkManager netManager = (RPGNetworkManager)NetworkManager.singleton;
        netManager.teamHealth = teamHealth;

        if (teamHealth <= 0)
        {
            teamHealth = 0;
            netManager.teamHealth = 0;
            isGameOver = true;

            SaveEndRunProgress(false);
        }
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        UIManager.Instance?.UpdateHealthUI(newHealth);

        if (newHealth <= 0)
        {
            UIManager.Instance?.ShowDefeat();
        }
    }

    [Server]
    public void AddXP(float amount)
    {
        teamXP += amount;
        Debug.Log($"[Сервер] Команда получила {amount} XP. Текущий опыт: {teamXP}/{xpToNextLevel}");

        // Проверяем, не апнули ли мы уровень (используем while, вдруг дали очень много опыта за раз)
        while (teamXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    [Server]
    private void LevelUp()
    {
        teamXP -= xpToNextLevel; // Переносим остаток опыта на следующий уровень
        teamLevel++;
        xpToNextLevel *= xpMultiplierPerLevel; // Увеличиваем порог следующего уровня

        Debug.Log($"[Сервер] Команда достигла {teamLevel} уровня!");

        // Выдаем очки прокачки всем активным игрокам
        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in allPlayers)
        {
            player.AddSkillPoint();
        }
    }

    [Server]
    public void AddCurrency(int amount)
    {
        teamCurrency += amount;
        Debug.Log($"[Сервер] Команда получила {amount} монет. Всего: {teamCurrency}");
    }

    // [BACKEND] Увеличение счётчика убийств
    [Server]
    public void AddKill()
    {
        totalKillsThisRun++;
    }

    [Server]
    private void SaveEndRunProgress(bool victory)
    {
        float duration = Time.time - runStartTime;

        var netManager = (RPGNetworkManager)NetworkManager.singleton;
        int teammateId = netManager != null ? netManager.GetTeammateUserId() : 0;

        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.SaveEndGameProgress(
                teamCurrency, totalKillsThisRun, victory, teamLevel, duration, teammateId
            );
        }

        var msg = new RunEndMessage
        {
            durationSeconds = duration,
            kills = totalKillsThisRun,
            victory = victory,
            currencyEarned = teamCurrency,
            teamLevel = teamLevel,
            teammateId = teammateId
        };

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.connectionId != 0)
                conn.Send(msg);
        }
    }

    private void OnRunEndMessage(RunEndMessage msg)
    {
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.SaveProgressOnly(msg.currencyEarned, msg.kills);
        }
    }

    [Server]
    public void OnBossDefeated()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("Босс убит!");
        SaveEndRunProgress(true);
    }

    // [BACKEND] Хук для обновления UI валюты
    private void OnCurrencyChanged(int oldVal, int newVal)
    {
        UIManager.Instance.UpdateCurrencyUI(newVal);
    }

    // Хуки для обновления UI (если захочешь сделать полоску опыта)
    private void OnXPChanged(float oldXP, float newXP)
    {
        UIManager.Instance.UpdateXPBarUI(newXP, xpToNextLevel);
    }

    private void OnLevelChanged(int oldLevel, int newLevel)
    {
        // UIManager.Instance.UpdateLevelText(newLevel);
    }
}
