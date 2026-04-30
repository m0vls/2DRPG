using Mirror;
using UnityEngine;

public class TestDamageButton : MonoBehaviour
{
    public void DealDamage()
    {
        // Находим локального игрока
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer != null)
        {
            // Получаем компонент PlayerHealth, который мы создали ранее
            var health = localPlayer.GetComponent<IDamageable>();
            health.TakeDamage(10); // Наносим 10 урона
        }
    }
}