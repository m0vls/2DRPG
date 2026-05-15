using UnityEngine;

public enum RoomType { Start, Normal, Boss, DeadEnd }

public class RoomData : MonoBehaviour
{
    public RoomType roomType;
    public Vector2Int GridPosition { get; set; }

    [Header("Двери (Триггеры)")]
    // Перетащи сюда объекты с компонентом DoorTransition из префаба
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
        if (direction == Vector2Int.up) return topDoor != null;
        if (direction == Vector2Int.down) return bottomDoor != null;
        if (direction == Vector2Int.left) return leftDoor != null;
        if (direction == Vector2Int.right) return rightDoor != null;
        return false;
    }

    // Получаем точку спавна (куда игрок приземлится после перехода)
    public Transform GetLandingPoint(Vector2Int arrivalDirection)
    {
        // Если мы пришли СНИЗУ (up), то должны оказаться у НИЖНЕЙ двери
        if (arrivalDirection == Vector2Int.up) return bottomDoor.spawnPoint;
        if (arrivalDirection == Vector2Int.down) return topDoor.spawnPoint;
        if (arrivalDirection == Vector2Int.left) return rightDoor.spawnPoint;
        if (arrivalDirection == Vector2Int.right) return leftDoor.spawnPoint;
        return transform;
    }
}