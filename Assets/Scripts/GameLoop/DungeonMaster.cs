using DG.Tweening;
using Mirror;
using NavMeshPlus.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    [Header("Враги")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField, Range(0f, 1f)] private float enemySpawnChance = 0.6f;

    [Header("Навигация")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    private Dictionary<Vector2Int, RoomData> spawnedRooms = new Dictionary<Vector2Int, RoomData>();
    private Queue<Vector2Int> roomsToGrow = new Queue<Vector2Int>();
    private bool bossSpawned = false;

    [SyncVar(hook = nameof(OnSeedChanged))]
    private int dungeonSeed;

    private void Awake() => Instance = this;

    public override void OnStartServer()
    {
        dungeonSeed = Random.Range(10000, 99999);
        Debug.Log($"Сид подземелья: {dungeonSeed}");
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

        float duration = 0.3f;
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
        bossSpawned = false;

        SpawnRoom(Vector2Int.zero, startRoomPrefab);
        roomsToGrow.Enqueue(Vector2Int.zero);

        int count = 1;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (roomsToGrow.Count > 0 && count < maxRooms)
        {
            Vector2Int currentPos = roomsToGrow.Dequeue();
            RoomData currentRoom = spawnedRooms[currentPos];

            foreach (var dir in directions)
            {
                Vector2Int nextPos = currentPos + dir;
                if (!currentRoom.HasExit(dir) || spawnedRooms.ContainsKey(nextPos))
                    continue;
                

                if (count >= maxRooms) break;

                GameObject prefabToSpawn = null;

                // Условие для Босса: если это последняя комната и мы идем ВВЕРХ
                if (count == maxRooms - 1 && dir == Vector2Int.up && !bossSpawned)
                {
                    prefabToSpawn = bossRoomPrefab;
                    bossSpawned = true;
                }
                else
                {
                    // Ищем обычную комнату, у которой есть вход с нужной стороны
                    var valid = normalRoomPrefabs.Where(p => p.GetComponent<RoomData>().HasExit(-dir)).ToList();
                    if (valid.Count > 0) prefabToSpawn = valid[Random.Range(0, valid.Count)];
                }

                if (prefabToSpawn != null)
                {
                    SpawnRoom(nextPos, prefabToSpawn);
                    roomsToGrow.Enqueue(nextPos);
                    count++;
                }
            }
        }

        // Если босс так и не заспавнился (не было хода вверх), пробуем форсировать его на любом свободном верхнем выходе
        if (!bossSpawned) ForceSpawnBoss();

        FinalizeDeadEnds();

        foreach (var room in spawnedRooms)
        {
            Vector2Int currentPos = room.Key;
            RoomData currentRoom = room.Value;

            foreach (var dir in directions)
            {
                Vector2Int nextPos = currentPos + dir;

                // ПРОВЕРКА: Если комната там уже создана другой веткой генерации
                if (spawnedRooms.ContainsKey(nextPos))
                {
                    RoomData neighborRoom = spawnedRooms[nextPos];
                    // Если у соседа нет ответного входа с нашей стороны (-dir)
                    if (!neighborRoom.HasExit(-dir))
                    {
                        DoorTransition door = currentRoom.GetExit(dir);
                        if (door != null)
                        {
                            Collider2D col = door.GetComponent<Collider2D>();
                            if (col != null) col.isTrigger = false; // Закрываем проход физически
                        }
                    }
                }
            }
        }

        if (isServer)
        {
            CompressAllDungeonTilemaps();

            navMeshSurface.BuildNavMesh();

            PopulateDungeon();
        }
    }

    private void CompressAllDungeonTilemaps()
    {
        Tilemap[] allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);

        foreach (Tilemap tilemap in allTilemaps)
        {
            tilemap.CompressBounds();
        }
    }

    private void ForceSpawnBoss()
    {
        foreach (var room in spawnedRooms.Values.ToList())
        {
            if (room.HasExit(Vector2Int.up))
            {
                Vector2Int nextPos = room.GridPosition + Vector2Int.up;
                if (!spawnedRooms.ContainsKey(nextPos))
                {
                    SpawnRoom(nextPos, bossRoomPrefab);
                    bossSpawned = true;
                    return;
                }
            }
        }
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

    [Server]
    private void PopulateDungeon()
    {
        foreach (var room in spawnedRooms.Values)
        {
            // 1. Спавн обычных врагов в обычных комнатах и тупиках
            if (room.roomType == RoomType.Normal || room.roomType == RoomType.DeadEnd)
            {
                if (room.enemySpawnPoints == null || room.enemySpawnPoints.Length == 0) continue;

                foreach (Transform spawnPoint in room.enemySpawnPoints)
                {
                    // Проверяем шанс спавна (чтобы комнаты не всегда были забиты битком)
                    if (Random.value <= enemySpawnChance)
                    {
                        // Выбираем случайного врага из списка
                        GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

                        // Создаем объект на сервере
                        GameObject enemyInstance = Instantiate(randomEnemyPrefab, spawnPoint.position, Quaternion.identity);

                        // Регистрируем объект в сети
                        NetworkServer.Spawn(enemyInstance);
                    }
                }
            }

            // 2. Спавн Босса в комнате босса
            else if (room.roomType == RoomType.Boss)
            {
                if (bossPrefab != null && room.bossSpawnPoint != null)
                {
                    GameObject bossInstance = Instantiate(bossPrefab, room.bossSpawnPoint.position, Quaternion.identity);
                    NetworkServer.Spawn(bossInstance);
                }
                else
                {
                    Debug.LogWarning("[DungeonMaster] Не назначен префаб босса или точка спавна в комнате босса!");
                }
            }
        }
    }
}