using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

public class TeamStateManager : NetworkBehaviour
{
    public static TeamStateManager Instance;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float teamHealth = 100;

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
}
