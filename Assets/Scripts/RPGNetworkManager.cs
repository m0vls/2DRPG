using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class RPGNetworkManager : NetworkManager
{
    [Header("Префабы классов")]
    public GameObject knightPrefab;
    public GameObject magePrefab;

    public float teamHealth = 100;

    private readonly Dictionary<NetworkConnectionToClient, PlayerClass> playerChoices = new Dictionary<NetworkConnectionToClient, PlayerClass>();

    [Header("UI")]
    [SerializeField] private GameObject selectPanel;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CharacterSelectMessage>(OnCreateCharacter);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        selectPanel.SetActive(true);
    }

    private void OnCreateCharacter(NetworkConnectionToClient conn, CharacterSelectMessage message)
    {
        // 1. Запоминаем выбор игрока
        playerChoices[conn] = message.characterClass;
        
        // 2. Вызываем метод спавна
        SpawnPlayerForConnection(conn, message.characterClass);
    }

    private void SpawnPlayerForConnection(NetworkConnectionToClient conn, PlayerClass charClass)
    {
        if (conn.identity != null) return;

        GameObject prefab = charClass == PlayerClass.Knight ? knightPrefab : magePrefab;

        Transform startPos = GetStartPosition();
        GameObject player = Instantiate(prefab, startPos.position, startPos.rotation);

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        if (conn.identity == null)
        {
            if (playerChoices.TryGetValue(conn, out var playerClass))
            {
                SpawnPlayerForConnection(conn, playerClass);
            }
        }
    }

    // Этот метод вызывается автоматически после смены сцены на сервере
    /*public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        // Пересоздаем игроков для всех активных соединений
        foreach (var entry in playerChoices)
        {
            NetworkConnectionToClient conn = entry.Key;
            PlayerClass chosenClass = entry.Value;

            // Если объект игрока был уничтожен при смене сцены, спавним его заново
            if (conn.identity == null)
            {
                SpawnPlayerForConnection(conn, chosenClass);
            }
        }
    }*/

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // Очищаем данные при выходе игрока из игры
        playerChoices.Remove(conn);
        base.OnServerDisconnect(conn);
    }
}
