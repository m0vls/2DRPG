using Mirror;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Player : NetworkBehaviour, IDamageable
{
    [Header("Кадры неуязвимости")]
    [SerializeField] protected SpriteRenderer playerSprite;
    [SerializeField] protected float invincibilityDuration = 1.5f;

    protected bool isInvincible = false;
    

    [Header("Передвижение игрока")]
    [SerializeField] protected float moveSpeed = 5f;

    [SyncVar(hook =nameof(OnDirectionChanged))]
    public Vector2 lastFacingDirection = Vector2.down;

    protected Rigidbody2D rb;
    protected InputSystem_Actions inputActions;
    protected Vector2 currentMovementInput;

    protected bool isAttacking = false;

    protected TeamStateManager teamStateManager;

    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        inputActions = new InputSystem_Actions();
    }

    protected virtual void Start()
    {
        teamStateManager = TeamStateManager.Instance;
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

    protected virtual void OnDisable()
    {
        inputActions.Player.Disable();
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer) return;

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
        Vector2 velocity = direction.normalized * moveSpeed;
        rb.linearVelocity = velocity;
    }

    #region Направление игрока
    protected virtual void OnDirectionChanged(Vector2 olddir, Vector2 newdir)
    {
        //обновление аниматора в будущем
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

    public void TakeDamage(float damageAmount)
    {
        if (isInvincible || !isLocalPlayer)
        {
            return;
        }
        CmdTakeDamage(damageAmount);

        StartCoroutine(InvincibilityRoutine());
    }

    [Command]
    private void CmdTakeDamage(float damage)
    {
        teamStateManager.TakeTeamDamage(damage);
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
}
