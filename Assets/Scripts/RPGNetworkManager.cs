using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class RPGNetworkManager : NetworkManager
{
    [Header("Префабы классов")]
    public GameObject knightPrefab;
    public GameObject magePrefab;

    private float startHealth = 100f;

    public float teamHealth;

    private readonly Dictionary<NetworkConnectionToClient, PlayerClass> playerChoices = new Dictionary<NetworkConnectionToClient, PlayerClass>();

    public override void OnStartServer()
    {
        teamHealth = startHealth;

        base.OnStartServer();
        NetworkServer.RegisterHandler<CharacterSelectMessage>(OnCreateCharacter);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        UIManager.Instance.ToggleSelectUI();
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

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // Очищаем данные при выходе игрока из игры
        playerChoices.Remove(conn);
        base.OnServerDisconnect(conn);
    }
}
