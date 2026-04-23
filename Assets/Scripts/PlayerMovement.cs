using Mirror;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Vector2 currentInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        inputActions = new InputSystem_Actions();
    }

    public override void OnStartLocalPlayer()
    {
        inputActions.Player.Enable();
    }

    public void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        currentInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            ApplyMovement(currentInput);
        }
    }

    private void ApplyMovement(Vector2 direction)
    {
        Vector2 velocity = direction.normalized * moveSpeed;
        rb.linearVelocity = velocity;

        if (!authority)
        {
            CmdSendInput(velocity);
        }
    }

    [Command]
    private void CmdSendInput(Vector2 velocity)
    {
        // сервер рассчитывает движение
        rb.linearVelocity = velocity;
    }
}