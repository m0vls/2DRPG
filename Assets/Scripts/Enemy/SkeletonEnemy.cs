using System.Collections;
using UnityEngine;

public class SkeletonEnemy : Enemy
{
    [Header("Настройки Скелета")]
    [SerializeField] private float updatePathInterval = 0.5f;

    protected override void StartMovement()
    {
        // Запускаем корутину преследования только на сервере
        StartCoroutine(ChasePlayerRoutine());
    }

    private IEnumerator ChasePlayerRoutine()
    {
        while (true)
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                GameObject target = TargetNeariestPlayer();

                if (target != null)
                {
                    Transform playerPoint = target.GetComponent<Player>().targetEnemyPoint;
                    float distance = Vector2.Distance(transform.position, playerPoint.position);

                    // Если игрок в радиусе обнаружения, идем к нему
                    if (distance <= detectionRange)
                    {
                        agent.SetDestination(playerPoint.position);
                    }
                    else
                    {
                        // Если игрок далеко, останавливаемся
                        agent.ResetPath();
                    }
                }
            }
            // Обновляем путь не каждый кадр (оптимизация сервера)
            yield return new WaitForSeconds(updatePathInterval);
        }
    }
}