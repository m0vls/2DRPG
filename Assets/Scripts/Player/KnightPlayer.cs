using Mirror;
using System.Collections;
using UnityEngine;

public class KnightPlayer : Player
{
    [Header("Настройки атаки")]
    [SerializeField] private float attackDamage = 15;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackDuration = 0.2f;

    [Header("Компоненты для атаки")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Collider2D attackCollider;
    [SerializeField] private SpriteRenderer attackSprite;

    [Header("Слои")]
    [SerializeField] private LayerMask enemyLayers;

    private float nextAttackTime = 0f;
    

    protected override void Awake()
    {
        base.Awake();
        if (attackCollider != null ) 
            attackCollider.enabled = false;

        if (attackSprite != null)
            attackSprite.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        UIManager.Instance.ShowKnightUI();
    }

    protected override void OnDirectionChanged(Vector2 olddir, Vector2 newdir)
    {
        base.OnDirectionChanged(olddir, newdir);
        RotateAttackPoint(newdir);
    }

    private void RotateAttackPoint(Vector2 direction)
    {
        if (attackPoint == null) return;

        float angle = 0f;

        if (direction == Vector2.up) angle = 180f; // Вверх
        else if (direction == Vector2.down) angle = 0f; // Вниз
        else if (direction == Vector2.left) angle = -90f; // Влево
        else if (direction == Vector2.right) angle = 90f; // Вправо

        attackPoint.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected override void PlayerAttack()
    {
        if (inputActions.Player.Attack.triggered && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        attackSprite.enabled = true;
        CmdExecuteAttack();

        yield return new WaitForSeconds(attackDuration);

        attackSprite.enabled = false;
        isAttacking = false;
    }

    [Command]
    private void CmdExecuteAttack()
    {
        attackCollider.enabled = true;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayers);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int hitCount = Physics2D.OverlapCollider(attackCollider, filter, results);

        Debug.Log($"[Сервер] Атака активирована. Найдено объектов: {hitCount}");

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D enemyCollider = results[i];

            Debug.Log($"[Сервер] Попадание в объект: {results[i].name}");

            if (enemyCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"[Сервер] Рыцарь ударил {enemyCollider.name} на {attackDamage} урона");
            }
        }

        attackCollider.enabled = false;
    }
}
