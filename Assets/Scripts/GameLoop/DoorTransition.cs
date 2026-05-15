using Mirror;
using UnityEngine;
using DG.Tweening;

public class DoorTransition : MonoBehaviour
{
    public Transform spawnPoint; // Точка внутри комнаты, где стоит игрок после входа
    private Vector2Int direction;
    private RoomData parentRoom;

    public void Setup(Vector2Int dir, RoomData room)
    {
        direction = dir;
        parentRoom = room;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем только на сервере (Host), чтобы избежать двойных срабатываний
        if (!NetworkServer.active) return;

        if (collision.CompareTag("Player"))
        {
            Vector2Int targetGridPos = parentRoom.GridPosition + direction;
            RoomData targetRoom = DungeonMaster.Instance.GetRoomAt(targetGridPos);

            if (targetRoom != null)
            {
                Transform landingTransform = targetRoom.GetLandingPoint(direction);
                // Вызываем метод у сетевого синглтона
                DungeonMaster.Instance.RequestTeleport(landingTransform.position);
            }
        }
    }
}