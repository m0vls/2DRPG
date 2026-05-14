using Mirror;
using UnityEngine;

public class MagePlayer : Player
{
    [Header("Настройки Мага")]
    public int maxAmmo = 5;

    [SyncVar] public int currentAmmo;

    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Transform attackPoint; // Точка вылета снаряда
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряда

    private Camera mainCamera;
    private float nextAttackTime = 0f;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        UIManager.Instance.ShowMageUI();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentAmmo = maxAmmo; // Выдаем патроны при спавне на сервере
    }

    protected override void PlayerAttack()
    {
        if (inputActions.Player.Attack.triggered && Time.time >= nextAttackTime && !isAttacking)
        {
            if (currentAmmo > 0)
            {
                FireProjectile();
                nextAttackTime = Time.time + attackCooldown;
            }
            else
            {
                Debug.Log("Нет зарядов! Собери осколки маны.");
                // Здесь можно добавить звук осечки
            }
        }
    }

    private void FireProjectile()
    {
        Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

        //Vector2 mouseScreenPos = inputActions.UI.Point.ReadValue<Vector2>();

        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        //Вычисляем вектор направления от точки атаки до курсора
        Vector2 direction = (mouseWorldPos - (Vector2)attackPoint.position).normalized;
        TriggerAttackVisual("Attack");
        CmdFireProjectile(direction);
    }

    [Command]
    private void CmdFireProjectile(Vector2 direction)
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;

        GameObject projectileInstance = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);

        projectileInstance.GetComponent<MageProjectile>().Setup(direction);

        NetworkServer.Spawn(projectileInstance);
    }

    [Server]
    public void RestoreAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    }
}
