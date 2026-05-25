using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : NetworkBehaviour, IDamageable
{
    protected NavMeshAgent agent;

    [Header("Характеристики")]
    [SerializeField] protected float maxHealth = 50;
    [SerializeField] protected float detectionRange = 2f;
    [SerializeField] protected float xpReward = 5f;
    [SerializeField] protected int currencyReward = 10;
    [SerializeField] protected bool isBoss = false;

    [Header("Визуал и Анимация")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Color hitFlashColor = Color.red;
    [SerializeField] protected float hitFlashDuration = 0.15f;
    [SerializeField] protected bool spritesFaceLeftByDefault = false;

    [Header("Откидывание (Knockback)")]
    [SerializeField] protected float knockbackForce = 1.5f;
    [SerializeField] protected float knockbackDuration = 0.2f;
    [SerializeField] protected bool isKnockbackable = true;

    [SyncVar][SerializeField] protected float currentHealth;

    private Vector3 _lastPosition;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
        {
            agent.enabled = false;
        }
        _lastPosition = transform.position;
    }

    protected virtual void Update()
    {
        // Вычисляем вектор смещения между кадрами
        Vector3 displacement = transform.position - _lastPosition;
        Vector3 velocity = displacement / Time.deltaTime;
        _lastPosition = transform.position;

        // Проверяем, движется ли враг
        bool isMovingNow = velocity.sqrMagnitude > 0.005f;

        if (animator != null)
        {
            animator.SetBool("isMoving", isMovingNow);

            if (isMovingNow)
            {
                Vector2 moveDirection = ((Vector2)velocity).normalized;

                animator.SetFloat("Horizontal", moveDirection.x);
                animator.SetFloat("Vertical", moveDirection.y);

                if (spriteRenderer != null && Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
                {
                    bool moveLeft = moveDirection.x < -0.1f;
                    spriteRenderer.flipX = spritesFaceLeftByDefault ? !moveLeft : moveLeft;
                }
            }
        }
    }

    protected abstract void StartMovement();

    [Server]
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        // Запускаем визуальное мигание на всех клиентах
        RpcShowHitVisual();

        // Откидывание
        if (isKnockbackable)
        {
            GameObject nearestPlayer = TargetNeariestPlayer();
            if (nearestPlayer != null)
            {
                Vector3 knockbackDir = (transform.position - nearestPlayer.transform.position).normalized;
                StartCoroutine(KnockbackRoutine(knockbackDir));
            }
        }

        if (currentHealth <= 0)
        {
            TeamStateManager.Instance.AddXP(xpReward);
            TeamStateManager.Instance.AddCurrency(currencyReward);
            TeamStateManager.Instance.AddKill();
            if (isBoss) TeamStateManager.Instance.OnBossDefeated();
            NetworkServer.Destroy(gameObject);
        }
    }

    [ClientRpc]
    protected void RpcShowHitVisual()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    [Server]
    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        if (agent.isActiveAndEnabled) agent.enabled = false; // Выключаем навигацию, чтобы не сопротивлялась

        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + (direction * knockbackForce);
        float elapsedTime = 0f;

        while (elapsedTime < knockbackDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime / knockbackDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (this != null) // Защита, если враг умер во время откидывания
        {
            agent.enabled = true; // Включаем обратно
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
