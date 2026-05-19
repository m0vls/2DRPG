using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance;

    [Header("Кнопки")]
    public Button btnUpgradeAttack;
    public Button btnUpgradeDefense;
    public Button btnUpgradeSpeed;

    [Header("Текстовые поля (TMP)")]
    [SerializeField] private TMP_Text txtSkillPointsCount;
    [SerializeField] private TMP_Text txtAttackValue;
    [SerializeField] private TMP_Text txtDefenseValue;
    [SerializeField] private TMP_Text txtSpeedValue;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        btnUpgradeAttack.onClick.AddListener(OnUpgradeAttackPressed);
        btnUpgradeDefense.onClick.AddListener(OnUpgradeDefensePressed);
        btnUpgradeSpeed.onClick.AddListener(OnUpgradeSpeedPressed);
    }

    private void OnEnable()
    {
        // Когда игрок открывает меню паузы -> панель прокачки,
        // принудительно обновляем данные на экране актуальными значениями
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (NetworkClient.localPlayer != null)
        {
            Player localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
            if (localPlayer != null)
            {
                UpdateSkillPointsText(localPlayer.availableSkillPoints);
                UpdateStatsDisplay(localPlayer.CurrentStats);
            }
        }
    }

    public void UpdateSkillPointsText(int points)
    {
        txtSkillPointsCount.text = $"Очки навыков: {points}";

        // Оптимизация: если очков нет, выключаем кликабельность кнопок плюсиков
        bool hasPoints = points > 0;
        btnUpgradeAttack.interactable = hasPoints;
        btnUpgradeDefense.interactable = hasPoints;
        btnUpgradeSpeed.interactable = hasPoints;
    }

    public void UpdateStatsDisplay(CharacterStats stats)
    {
        txtAttackValue.text = $"Атака: {stats.attackPower}";
        txtDefenseValue.text = $"Защита: {stats.defense}";
        txtSpeedValue.text = $"Скорость: {stats.moveSpeed:F1}"; // Округление до 1 знака после запятой
    }

    private void OnUpgradeAttackPressed()
    {
        if (NetworkClient.localPlayer != null)
            NetworkClient.localPlayer.GetComponent<Player>().CmdUpgradeAttack();
    }

    private void OnUpgradeDefensePressed()
    {
        if (NetworkClient.localPlayer != null)
            NetworkClient.localPlayer.GetComponent<Player>().CmdUpgradeDefense();
    }

    private void OnUpgradeSpeedPressed()
    {
        if (NetworkClient.localPlayer != null)
            NetworkClient.localPlayer.GetComponent<Player>().CmdUpgradeMoveSpeed();
    }
}