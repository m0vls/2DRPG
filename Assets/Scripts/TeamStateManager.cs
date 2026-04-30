using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class TeamStateManager : NetworkBehaviour
{
    public static TeamStateManager Instance;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int teamHealth = 100;

    public UnityEvent<int> OnHealthUpdated;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Server]
    public void TakeTeamDamage(int damageAmount)
    {
        teamHealth -= damageAmount;
        if (teamHealth <= 0)
        {
            teamHealth = 0;
            RpcGameOver();
        }
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        OnHealthUpdated?.Invoke(newHealth);
    }

    [ClientRpc]
    private void RpcGameOver()
    {
        Debug.Log("Вся команда погибла!");
    }
}
