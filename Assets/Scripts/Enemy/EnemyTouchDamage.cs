using Mirror;
using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [SerializeField] private float damage = 5;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<IDamageable>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
