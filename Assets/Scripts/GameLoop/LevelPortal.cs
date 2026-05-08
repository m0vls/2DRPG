using Mirror;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelPortal : NetworkBehaviour
{
    [Header("Настройки портала")]
    [Tooltip("Точное имя сцены, куда ведет портал")]
    [SerializeField] private string nextSceneName = "Level_1";

    [Tooltip("Сколько игроков нужно для запуска")]
    [SerializeField] private int requiredPlayers = 2;

    // Храним уникальные объекты игроков, которые стоят в портале
    private HashSet<GameObject> playersInPortal = new HashSet<GameObject>();

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    [ServerCallback] // Выполняется только на сервере
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Убедитесь, что у ваших префабов игроков стоит тег "Player"
        if (collision.CompareTag("Player"))
        {
            playersInPortal.Add(collision.gameObject);
            Debug.Log($"[Сервер] Игрок зашел в портал. Всего: {playersInPortal.Count}/{requiredPlayers}");

            CheckPortalReady();
        }
    }

    [ServerCallback]
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInPortal.Remove(collision.gameObject);
            Debug.Log($"[Сервер] Игрок вышел из портала. Всего: {playersInPortal.Count}/{requiredPlayers}");
        }
    }

    [Server]
    private void CheckPortalReady()
    {
        // Если нужное количество игроков собралось на платформе
        if (playersInPortal.Count >= requiredPlayers)
        {
            Debug.Log("[Сервер] Все игроки готовы! Запускаем смену сцены...");

            // Эта команда перенесет сервер и всех клиентов на новую сцену
            NetworkManager.singleton.ServerChangeScene(nextSceneName);
        }
    }
}