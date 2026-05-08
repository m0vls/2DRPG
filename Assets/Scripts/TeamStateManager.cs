using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class TeamStateManager : NetworkBehaviour
{
    public static TeamStateManager Instance;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public float teamHealth = 100;

    //public UnityEvent<float> OnHealthUpdated;

    private bool isGameOver = false;

    //[SerializeField] private DefeatUI defeatUI;

    public void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        RPGNetworkManager netManager = (RPGNetworkManager)NetworkManager.singleton;
        teamHealth = netManager.teamHealth;

    }

    [Server]
    public void TakeTeamDamage(float damageAmount)
    {
        if (isGameOver) return;
        
        teamHealth -= damageAmount;

        RPGNetworkManager netManager = (RPGNetworkManager)NetworkManager.singleton;
        netManager.teamHealth = teamHealth;

        if (teamHealth <= 0)
        {
            teamHealth = 0;
            netManager.teamHealth = 0;
            isGameOver = true;
            //RpcGameOver();
        }
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        //OnHealthUpdated?.Invoke(newHealth);
        UIManager.Instance?.UpdateHealthUI(newHealth);

        if (newHealth <= 0)
        {
            UIManager.Instance?.ShowDefeat();
        }
    }

    //[ClientRpc]
    //private void RpcGameOver()
    //{
    //    Debug.Log("Вся команда погибла!");
    //    defeatUI.ShowDefeatScreen();
    //}
}
