using UnityEngine;

public enum RoomType { Start, Normal, Boss, DeadEnd }

public class RoomData : MonoBehaviour
{
    public RoomType roomType;

    public Transform[] enemySpawnPoints;
    public Transform bossSpawnPoint;
    public Vector2Int GridPosition { get; set; }

    [Header("Двери (Триггеры)")]
    public DoorTransition topDoor;
    public DoorTransition bottomDoor;
    public DoorTransition leftDoor;
    public DoorTransition rightDoor;

    public void Init(Vector2Int pos)
    {
        GridPosition = pos;

        // Настраиваем двери, чтобы они знали, в какую сторону ведут
        if (topDoor) topDoor.Setup(Vector2Int.up, this);
        if (bottomDoor) bottomDoor.Setup(Vector2Int.down, this);
        if (leftDoor) leftDoor.Setup(Vector2Int.left, this);
        if (rightDoor) rightDoor.Setup(Vector2Int.right, this);
    }

    // Проверка: есть ли у этого префаба выход в конкретную сторону
    public bool HasExit(Vector2Int direction)
    {
         return GetExit(direction) != null;
    }

    public DoorTransition GetExit(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return topDoor;
        if (direction == Vector2Int.down) return bottomDoor;
        if (direction == Vector2Int.left) return leftDoor;
        if (direction == Vector2Int.right) return rightDoor;
        return null;
    }

    // Получаем точку спавна (куда игрок приземлится после перехода)
    public Transform GetLandingPoint(Vector2Int arrivalDirection)
    {
        DoorTransition dt = GetExit(-arrivalDirection);
        if (dt != null) return dt.spawnPoint;
        
        return null;
    }
}