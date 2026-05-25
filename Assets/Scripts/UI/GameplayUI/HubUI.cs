using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubUI : MonoBehaviour
{
    [Header("Текущие значения")]
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text attackStatusText;
    [SerializeField] private TMP_Text defenseStatusText;
    [SerializeField] private TMP_Text speedStatusText;
    [SerializeField] private TMP_Text hpStatusText;

    [Header("Кнопки покупки")]
    [SerializeField] private Button buyAttackButton;
    [SerializeField] private Button buyDefenseButton;
    [SerializeField] private Button buySpeedButton;
    [SerializeField] private Button buyHpButton;

    [Header("Текст цен")]
    [SerializeField] private TMP_Text attackCostText;
    [SerializeField] private TMP_Text defenseCostText;
    [SerializeField] private TMP_Text speedCostText;
    [SerializeField] private TMP_Text hpCostText;

    private PlayerProgress progress;

    private void Start()
    {
        buyAttackButton.onClick.AddListener(() => BuyUpgrade("attack"));
        buyDefenseButton.onClick.AddListener(() => BuyUpgrade("defense"));
        buySpeedButton.onClick.AddListener(() => BuyUpgrade("speed"));
        buyHpButton.onClick.AddListener(() => BuyUpgrade("hp"));

        if (MetaProgression.Instance != null)
            MetaProgression.Instance.OnProgressRefreshed += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        if (MetaProgression.Instance != null)
            MetaProgression.Instance.OnProgressRefreshed -= Refresh;
    }

    private void OnEnable()
    {
        if (MetaProgression.Instance != null)
            MetaProgression.Instance.LoadProgress();
        Refresh();
    }

    private void Refresh()
    {
        if (MetaProgression.Instance == null) return;
        progress = MetaProgression.Instance.CurrentProgress;
        if (progress == null)
        {
            MetaProgression.Instance.LoadProgress();
            return;
        }

        currencyText.text = progress.currency.ToString();

        SetUpgradeStatus(attackStatusText, progress.meta_attack_unlocked);
        SetUpgradeStatus(defenseStatusText, progress.meta_defense_unlocked);
        SetUpgradeStatus(speedStatusText, progress.meta_speed_unlocked);
        SetUpgradeStatus(hpStatusText, progress.meta_max_hp_unlocked);

        attackCostText.text = MetaProgression.MetaAttackCost.ToString();
        defenseCostText.text = MetaProgression.MetaDefenseCost.ToString();
        speedCostText.text = MetaProgression.MetaSpeedCost.ToString();
        hpCostText.text = MetaProgression.MetaHpCost.ToString();

        buyAttackButton.interactable = !progress.meta_attack_unlocked && progress.currency >= MetaProgression.MetaAttackCost;
        buyDefenseButton.interactable = !progress.meta_defense_unlocked && progress.currency >= MetaProgression.MetaDefenseCost;
        buySpeedButton.interactable = !progress.meta_speed_unlocked && progress.currency >= MetaProgression.MetaSpeedCost;
        buyHpButton.interactable = !progress.meta_max_hp_unlocked && progress.currency >= MetaProgression.MetaHpCost;
    }

    private void SetUpgradeStatus(TMP_Text text, bool unlocked)
    {
        if (text == null) return;
        text.text = unlocked ? "Куплено" : "Не куплено";
        text.color = unlocked ? Color.green : Color.red;
    }

    public void SetInteractive(bool state)
    {
        buyAttackButton.interactable = state && buyAttackButton.interactable;
        buyDefenseButton.interactable = state && buyDefenseButton.interactable;
        buySpeedButton.interactable = state && buySpeedButton.interactable;
        buyHpButton.interactable = state && buyHpButton.interactable;
    }

    private void BuyUpgrade(string type)
    {
        if (progress == null) return;

        int cost = 0;
        bool alreadyOwned = false;

        switch (type)
        {
            case "attack":
                cost = MetaProgression.MetaAttackCost;
                alreadyOwned = progress.meta_attack_unlocked;
                progress.meta_attack_unlocked = true;
                break;
            case "defense":
                cost = MetaProgression.MetaDefenseCost;
                alreadyOwned = progress.meta_defense_unlocked;
                progress.meta_defense_unlocked = true;
                break;
            case "speed":
                cost = MetaProgression.MetaSpeedCost;
                alreadyOwned = progress.meta_speed_unlocked;
                progress.meta_speed_unlocked = true;
                break;
            case "hp":
                cost = MetaProgression.MetaHpCost;
                alreadyOwned = progress.meta_max_hp_unlocked;
                progress.meta_max_hp_unlocked = true;
                break;
        }

        if (alreadyOwned) return;

        progress.currency -= cost;

        var update = new ProgressUpdate
        {
            currency = progress.currency,
            meta_attack_unlocked = progress.meta_attack_unlocked,
            meta_defense_unlocked = progress.meta_defense_unlocked,
            meta_speed_unlocked = progress.meta_speed_unlocked,
            meta_max_hp_unlocked = progress.meta_max_hp_unlocked,
            total_kills = progress.total_kills,
            total_games = progress.total_games,
        };

        StartCoroutine(ApiService.Instance.UpdateProgress(update,
            result =>
            {
                if (MetaProgression.Instance != null)
                    MetaProgression.Instance.RefreshBonuses();
                Refresh();
            },
            error => Debug.LogError($"[Hub] Ошибка покупки: {error}")
        ));
    }
}
