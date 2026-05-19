using System.Collections;
using UnityEngine;

public class SlimeEnemy : Enemy
{
    [Header("Параметры рывка")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float afterDashSpeed = 0f;
    [SerializeField] private float pauseDuration = 0.5f;

    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float preparationTimer = 1.5f;


    protected override void StartMovement()
    {
        StartCoroutine(SlimeMoveRoutine());
    }

    private IEnumerator SlimeMoveRoutine()
    {
        while (true)
        {
            GameObject target = TargetNeariestPlayer();

            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);

                if (distance < detectionRange)
                {
                    //Обычное передвижение
                    agent.speed = normalSpeed;
                    agent.acceleration = 8f;

                    float timer = preparationTimer;

                    while (timer > 0)
                    {
                        agent.SetDestination(target.transform.position);
                        yield return new WaitForSeconds(0.1f);
                        timer -= 0.1f;
                    }

                    //Рывок
                    agent.speed = dashSpeed;
                    agent.acceleration = 100f;
                    agent.SetDestination(target.transform.position);

                    yield return new WaitForSeconds(dashDuration);

                    //Остановка
                    agent.speed = afterDashSpeed;
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();

                    //Время для удара
                    yield return new WaitForSeconds(pauseDuration);
                }
                else
                {
                    agent.ResetPath();
                    yield return new WaitForSeconds(pauseDuration);
                }
            }
            else
            {
                yield return new WaitForSeconds(pauseDuration);
            }

        }
    }
}
