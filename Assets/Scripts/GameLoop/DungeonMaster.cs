using DG.Tweening;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonMaster : NetworkBehaviour
{
    public static DungeonMaster Instance;

    [Header("Настройки")]
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private List<GameObject> normalRoomPrefabs;
    [SerializeField] private List<GameObject> deadEndPrefabs;
    [SerializeField] private GameObject bossRoomPrefab;
    [SerializeField] private float roomSpacing = 60f;
    [SerializeField] private int maxRooms = 12;

    private Dictionary<Vector2Int, RoomData> spawnedRooms = new Dictionary<Vector2Int, RoomData>();
    private Queue<Vector2Int> roomsToGrow = new Queue<Vector2Int>();

    [SyncVar(hook = nameof(OnSeedChanged))]
    private int dungeonSeed;

    private void Awake() => Instance = this;

    public override void OnStartServer()
    {
        dungeonSeed = Random.Range(10000, 99999);
        GenerateDungeon(dungeonSeed);
    }

    private void OnSeedChanged(int oldSeed, int newSeed)
    {
        if (!isServer) GenerateDungeon(newSeed);
    }

    [Server]
    public void RequestTeleport(Vector2 targetPos)
    {
        // Сервер командует всем клиентам выполнить переход
        RpcDoTeleport(targetPos);
    }

    [ClientRpc]
    private void RpcDoTeleport(Vector3 targetPos)
    {
        // Здесь твоя логика с DOTween и UIManager из DoorTransition

        float duration = 0.4f;
        UIManager.Instance.FadeScreen(1f, duration).OnComplete(() => {
        UIManager.Instance.IsInputBlock = true;

        if (NetworkClient.localPlayer != null)
        {
            NetworkClient.localPlayer.transform.position = targetPos;
            if (Camera.main != null)
                Camera.main.transform.position = new Vector3(targetPos.x, targetPos.y, -10f);
        }

            UIManager.Instance.FadeScreen(0f, duration).OnComplete(() =>
            {
                UIManager.Instance.IsInputBlock = false;
            });
        });
    }

    private void GenerateDungeon(int seed)
    {
        Random.InitState(seed);
        ClearDungeon();

        // 1. Стартовая комната (0,0)
        SpawnRoom(Vector2Int.zero, startRoomPrefab);
        roomsToGrow.Enqueue(Vector2Int.zero);

        int count = 1;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // 2. Основная генерация
        while (roomsToGrow.Count > 0 && count < maxRooms)
        {
            Vector2Int currentPos = roomsToGrow.Dequeue();
            RoomData currentRoom = spawnedRooms[currentPos];

            foreach (var dir in directions)
            {
                if (count >= maxRooms) break;

                Vector2Int nextPos = currentPos + dir;

                // Если в текущей комнате есть дверь В ЭТУ сторону И там еще нет комнаты
                if (currentRoom.HasExit(dir) && !spawnedRooms.ContainsKey(nextPos))
                {
                    GameObject prefab = GetCompatiblePrefab(dir, count == maxRooms - 1);
                    if (prefab != null)
                    {
                        SpawnRoom(nextPos, prefab);
                        roomsToGrow.Enqueue(nextPos);
                        count++;
                    }
                }
            }
        }

        // 3. Закрываем все висящие выходы тупиками
        FinalizeDeadEnds();
    }

    private GameObject GetCompatiblePrefab(Vector2Int moveDir, bool isBoss)
    {
        if (isBoss) return bossRoomPrefab;

        // Нам нужен вход с противоположной стороны (если идем Вверх, нужен вход Снизу)
        Vector2Int requiredEntry = -moveDir;

        var validPrefabs = normalRoomPrefabs
            .Where(p => p.GetComponent<RoomData>().HasExit(requiredEntry))
            .ToList();

        return validPrefabs.Count > 0 ? validPrefabs[Random.Range(0, validPrefabs.Count)] : null;
    }

    private void FinalizeDeadEnds()
    {
        var currentKeys = spawnedRooms.Keys.ToList();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var pos in currentKeys)
        {
            RoomData room = spawnedRooms[pos];
            foreach (var dir in directions)
            {
                if (room.HasExit(dir) && !spawnedRooms.ContainsKey(pos + dir))
                {
                    // Ищем тупик, у которого есть вход с нужной стороны
                    GameObject deadEnd = deadEndPrefabs.FirstOrDefault(p => p.GetComponent<RoomData>().HasExit(-dir));
                    if (deadEnd) SpawnRoom(pos + dir, deadEnd);
                }
            }
        }
    }

    private void SpawnRoom(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, gridPos.y * roomSpacing, 0);
        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        RoomData data = go.GetComponent<RoomData>();
        data.Init(gridPos);
        spawnedRooms.Add(gridPos, data);
    }

    public RoomData GetRoomAt(Vector2Int pos) => spawnedRooms.ContainsKey(pos) ? spawnedRooms[pos] : null;

    private void ClearDungeon()
    {
        foreach (var room in spawnedRooms.Values) if (room) Destroy(room.gameObject);
        spawnedRooms.Clear();
        roomsToGrow.Clear();
    }
}