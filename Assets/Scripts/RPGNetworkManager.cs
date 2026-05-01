using Mirror;
using UnityEngine;

public class RPGNetworkManager : NetworkManager
{
    [Header("Префабы классов")]
    public GameObject knightPrefab;
    public GameObject magePrefab;

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

    void OnCreateCharacter(NetworkConnectionToClient conn, CharacterSelectMessage message)
    {
        // Выбираем нужный префаб на основе сообщения
        GameObject prefab = message.characterClass == PlayerClass.Knight ? knightPrefab : magePrefab;

        // Спавним объект в точке старта
        Transform startPos = GetStartPosition();
        GameObject player = Instantiate(prefab, startPos.position, startPos.rotation);

        // Критически важно для Mirror: привязываем объект к соединению
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log($"Игрок {conn.connectionId} заспавнен как {(message.characterClass == PlayerClass.Knight ? "Рыцарь" : "Маг")}");
    }
}
