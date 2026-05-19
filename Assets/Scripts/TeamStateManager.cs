using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

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
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(teamHealth);
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
