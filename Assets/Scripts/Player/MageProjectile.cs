using UnityEngine;
using Mirror;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class MageProjectile : NetworkBehaviour
{
    [Header("Настройки снаряда")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 5;

    [Header("Спавн осколков")]
    [SerializeField] private GameObject manaShardPrefab;
    [SerializeField] private LayerMask enemyLayers;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifeTime);
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);

            SpawnManaShard();

            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    private void SpawnManaShard()
    {
        GameObject shard = Instantiate(manaShardPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(shard);
    }
}
