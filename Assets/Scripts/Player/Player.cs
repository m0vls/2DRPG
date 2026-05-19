using Mirror;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Player : NetworkBehaviour, IDamageable
{
    [Header("Характеристики")]
    [SerializeField] private CharacterStats baseStats;

    [SyncVar(hook = nameof(OnStatsChanged))] public CharacterStats CurrentStats;

    [SyncVar(hook = nameof(OnSkillPointsChanged))] public int availableSkillPoints = 0;

    // Настройки того, сколько дает 1 вложенное очко
    [SerializeField] private float attackUpgradeStep = 5f;
    [SerializeField] private float defenseUpgradeStep = 2f;
    [SerializeField] private float moveSpeedUpgradeStep = 0.5f;

    [Header("Анимация")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer playerSprite;
    [SerializeField] protected bool spritesFaceLeftByDefault = false;

    [Header("Кадры неуязвимости")]
    [SerializeField] protected float invincibilityDuration = 1.5f;

    protected bool isInvincible = false;
    
    [Header("Передвижение игрока")]
    [SyncVar(hook =nameof(OnDirectionChanged))]
    public Vector2 lastFacingDirection = Vector2.down;

    [SyncVar(hook = nameof(OnMovingStateChanged))]
    private bool isMoving = false;

    protected Rigidbody2D rb;
    protected InputSystem_Actions inputActions;
    protected Vector2 currentMovementInput;

    protected bool isAttacking = false;

    protected TeamStateManager teamStateManager;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        rb.gravityScale = 0;
        inputActions = new InputSystem_Actions();
    }

    protected virtual void Start()
    {
        teamStateManager = TeamStateManager.Instance;

        animator.SetFloat("Horizontal", lastFacingDirection.x);
        animator.SetFloat("Vertical", lastFacingDirection.y);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        CurrentStats = baseStats;
    }

    public override void OnStartLocalPlayer()
    {
        inputActions.Player.Enable();
        var vcam = GameObject.FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = transform;
        }
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        UIManager.Instance.HideGameplayUI();
    }

    protected virtual void OnDisable()
    {
        inputActions.Player.Disable();
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer) return;
        if (UIManager.Instance.IsInputBlock) return;

        if (isAttacking)
        {
            currentMovementInput = Vector2.zero;
            return;
        }

        currentMovementInput = inputActions.Player.Move.ReadValue<Vector2>();
        UpdateFacingDirection();

        PlayerAttack();
    }

    protected virtual void FixedUpdate()
    {
        if (isLocalPlayer && !isAttacking)
        {
            ApplyMovement(currentMovementInput);
        }
    }

    private void ApplyMovement(Vector2 direction)
    {
        Vector2 velocity = direction.normalized * CurrentStats.moveSpeed;
        rb.linearVelocity = velocity;

        float currentSpeed = direction.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        bool movingNow = currentSpeed > 0.1f;
        if (movingNow != isMoving)
        {
            CmdUpdateMovingState(movingNow);
        }
    }

    #region Направление игрока
    protected virtual void OnDirectionChanged(Vector2 olddir, Vector2 newdir)
    {
        if (newdir.x != 0)
        {
            if (spritesFaceLeftByDefault)
            {
                playerSprite.flipX = (newdir.x > 0);
            }
            else
            {
                playerSprite.flipX = (newdir.x < 0);
            }
        }

        animator.SetFloat("Horizontal", newdir.x);
        animator.SetFloat("Vertical", newdir.y);
    }

    private void OnMovingStateChanged(bool oldState, bool newState)
    {
        animator.SetFloat("Speed", newState ? 1f : 0f);
    }

    [Command]
    protected void CmdUpdateMovingState(bool moving)
    {
        isMoving = moving;
    }


    [Command]
    protected void CmdUpdateDirection(Vector2 newDir)
    {
        lastFacingDirection = newDir;
    }

    protected Vector2 GetSnapDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return input.x > 0 ? Vector2.right : Vector2.left;
        }
        return input.y > 0 ? Vector2.up : Vector2.down;
    }

    private void UpdateFacingDirection()
    {
        if (currentMovementInput.magnitude > 0.1f && !isAttacking)
        {
            // Берем преобладающее направление (крестовина: вверх, вниз, влево, вправо)
            Vector2 newDir = GetSnapDirection(currentMovementInput);

            if (newDir != lastFacingDirection)
            {
                // Отправляем новое направление на сервер
                CmdUpdateDirection(newDir);
            }
        }
    }
    #endregion

    #region Нанесение/Получение урона
    protected abstract void PlayerAttack();

    protected void TriggerAttackVisual(string triggerName = "Attack")
    {
        animator.SetTrigger(triggerName);
        CmdSyncAttackVisual(triggerName);
    }

    [Command]
    private void CmdSyncAttackVisual(string triggerName)
    {
        RpcSyncAttackVisual(triggerName);
    }

    [ClientRpc]
    private void RpcSyncAttackVisual(string triggerName)
    {
        if (isLocalPlayer) return;
        animator.SetTrigger(triggerName);
    }

    public void TakeDamage(float rawDamage)
    {
        if (isInvincible || !isLocalPlayer)
        {
            return;
        }
        CmdTakeDamage(rawDamage);

        StartCoroutine(InvincibilityRoutine());
    }

    [Command]
    private void CmdTakeDamage(float rawDamage)
    {
        float finalDamage = Mathf.Max(1f, rawDamage - CurrentStats.defense);

        teamStateManager.TakeTeamDamage(finalDamage);
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Визуальный эффект: мерцание
        float timer = 0;
        while (timer < invincibilityDuration)
        {
            playerSprite.color = new Color(1, 1, 1, 0.5f); // Полупрозрачный
            yield return new WaitForSeconds(0.1f);
            playerSprite.color = new Color(1, 1, 1, 1f); // Обычный
            yield return new WaitForSeconds(0.1f);
            timer += 0.2f;
        }

        isInvincible = false;
    }
    #endregion

    #region Прокачка

    [Server]
    public void AddSkillPoint()
    {
        availableSkillPoints++;
        // Здесь можно отправить ClientRpc, чтобы показать уведомление "+1 Очко навыков!"
    }

    private void OnSkillPointsChanged(int oldPoints, int newPoints)
    {
        if (isLocalPlayer && UpgradeUI.Instance != null)
        {
            UpgradeUI.Instance.UpdateSkillPointsText(newPoints);
        }
    }

    private void OnStatsChanged(CharacterStats oldStats, CharacterStats newStats)
    {
        if (isLocalPlayer && UpgradeUI.Instance != null)
        {
            UpgradeUI.Instance.UpdateStatsDisplay(newStats);
        }
    }

    [Command]
    public void CmdUpgradeAttack()
    {
        if (availableSkillPoints <= 0) return;

        availableSkillPoints--;

        // Копируем, меняем, перезаписываем (правило Mirror для структур)
        CharacterStats tempStats = CurrentStats;
        tempStats.attackPower += attackUpgradeStep;
        CurrentStats = tempStats;

        Debug.Log($"[Сервер] Игрок прокачал Атаку: {CurrentStats.attackPower}");
    }

    [Command]
    public void CmdUpgradeDefense()
    {
        if (availableSkillPoints <= 0) return;

        availableSkillPoints--;

        CharacterStats tempStats = CurrentStats;
        tempStats.defense += defenseUpgradeStep;
        CurrentStats = tempStats;
    }

    [Command]
    public void CmdUpgradeMoveSpeed()
    {
        if (availableSkillPoints <= 0) return;

        availableSkillPoints--;

        CharacterStats tempStats = CurrentStats;
        tempStats.moveSpeed += moveSpeedUpgradeStep;
        CurrentStats = tempStats;
    }

    #endregion
}
