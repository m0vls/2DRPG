using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class TeamStateManager : NetworkBehaviour
{
    public static TeamStateManager Instance;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float teamHealth = 100;

    public UnityEvent<float> OnHealthUpdated;

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
    public void TakeTeamDamage(float damageAmount)
    {
        teamHealth -= damageAmount;
        if (teamHealth <= 0)
        {
            teamHealth = 0;
            RpcGameOver();
        }
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        OnHealthUpdated?.Invoke(newHealth);
    }

    [ClientRpc]
    private void RpcGameOver()
    {
        Debug.Log("Вся команда погибла!");
    }
}
