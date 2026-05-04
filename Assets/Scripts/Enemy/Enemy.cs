using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : NetworkBehaviour, IDamageable
{
    protected NavMeshAgent agent;

    [SerializeField] protected float maxHealth = 50;
    [SerializeField] protected float detectionRange = 2f;

    //public virtual int MaxHealth 
    //{ 
    //    get
    //    {
    //        return maxHealth;
    //    }
    //    [Server]
    //    set
    //    {
    //        maxHealth = value;
    //    }
    //}

    [SyncVar] [SerializeField] protected float currentHealth;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        StartMovement();
    }

    protected abstract void StartMovement();

    [Server]
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    protected GameObject TargetNeariestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0) return null;

        return players
            .OrderBy(p => Vector2.Distance(transform.position, p.transform.position))
            .FirstOrDefault();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
