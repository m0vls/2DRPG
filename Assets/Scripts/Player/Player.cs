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

    protected Rigidbody2D rb;
    protected InputSystem_Actions inputActions;
    protected Vector2 currentMovementInput;


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
    }

    protected virtual void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            ApplyMovement(currentMovementInput);
        }
    }

    private void ApplyMovement(Vector2 direction)
    {
        Vector2 velocity = direction.normalized * moveSpeed;
        rb.linearVelocity = velocity;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || !isLocalPlayer)
        {
            return;
        }
        CmdTakeDamage(damageAmount);

        StartCoroutine(InvincibilityRoutine());
    }

    [Command]
    private void CmdTakeDamage(int damage)
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
}
