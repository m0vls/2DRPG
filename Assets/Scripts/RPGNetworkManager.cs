using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class RPGNetworkManager : NetworkManager
{
    [Header("Префабы классов")]
    public GameObject knightPrefab;
    public GameObject magePrefab;

    public float startHealth = 100f;

    public float teamHealth;

    private readonly Dictionary<NetworkConnectionToClient, PlayerClass> playerChoices = new Dictionary<NetworkConnectionToClient, PlayerClass>();
    public readonly Dictionary<NetworkConnectionToClient, string> playerNicknames = new Dictionary<NetworkConnectionToClient, string>();
    public readonly Dictionary<NetworkConnectionToClient, int> clientUserIds = new Dictionary<NetworkConnectionToClient, int>();
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
        playerChoices[conn] = message.characterClass;
        playerNicknames[conn] = message.nickname;
        clientUserIds[conn] = message.userId;

        SpawnPlayerForConnection(conn, message.characterClass);
    }

    public int GetTeammateUserId()
    {
        foreach (var kvp in clientUserIds)
        {
            if (kvp.Key.connectionId != 0)
                return kvp.Value;
        }
        return 0;
    }

    private void SpawnPlayerForConnection(NetworkConnectionToClient conn, PlayerClass charClass)
    {
        if (conn.identity != null) return;

        GameObject prefab = charClass == PlayerClass.Knight ? knightPrefab : magePrefab;

        Transform startPos = GetStartPosition();
        GameObject player = Instantiate(prefab, startPos.position, startPos.rotation);

        if (playerNicknames.TryGetValue(conn, out string nick))
        {
            var playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.playerNickname = nick;
            }
        }

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
        playerNicknames.Remove(conn);
        clientUserIds.Remove(conn);
        base.OnServerDisconnect(conn);
    }
}
