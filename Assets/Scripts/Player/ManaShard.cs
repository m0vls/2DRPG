using Mirror;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ManaShard : NetworkBehaviour
{
    [Header("Настройки разлета")]
    [SerializeField] private float minForce = 3f;
    [SerializeField] private float maxForce = 6f;

    
    [SerializeField] private float pickupDelay = 0.5f;

    private bool canBePickedUp = false;

    public override void OnStartServer()
    {
        // Даем случайный импульс в случайном направлении
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomForce = Random.Range(minForce, maxForce);

        // Используем Impulse для резкого отскока
        rb.AddForce(randomDir * randomForce, ForceMode2D.Impulse);

        // Добавляем сопротивление, чтобы он остановился через пару метров
        rb.linearDamping = 5f;

        StartCoroutine(EnablePickupCoroutine());
    }

    private IEnumerator EnablePickupCoroutine()
    {
        yield return new WaitForSeconds(pickupDelay);
        canBePickedUp = true;
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBePickedUp) return;

        if (collision.TryGetComponent<MagePlayer>(out var mage))
        {
            mage.RestoreAmmo(1);

            NetworkServer.Destroy(gameObject);
        }
    }
}
