using System;
using System.Collections;
using UnityEngine;

public class MetaProgression : MonoBehaviour
{
    public static MetaProgression Instance { get; private set; }

    public event Action OnProgressRefreshed;

    public static float AttackBonus { get; private set; }
    public static float DefenseBonus { get; private set; }
    public static float SpeedBonus { get; private set; }
    public static float HpBonus { get; private set; }

    [Header("Бонусы мета-улучшений (одноразовые)")]
    [SerializeField] private float attackBonusValue = 10f;
    [SerializeField] private float defenseBonusValue = 5f;
    [SerializeField] private float speedBonusValue = 1f;
    [SerializeField] private float hpBonusValue = 50f;

    [Header("Стоимость улучшений")]
    [SerializeField] private int metaAttackCost = 1000;
    [SerializeField] private int metaDefenseCost = 1000;
    [SerializeField] private int metaSpeedCost = 1000;
    [SerializeField] private int metaHpCost = 1000;

    public static int MetaAttackCost => Instance ? Instance.metaAttackCost : 1000;
    public static int MetaDefenseCost => Instance ? Instance.metaDefenseCost : 1000;
    public static int MetaSpeedCost => Instance ? Instance.metaSpeedCost : 1000;
    public static int MetaHpCost => Instance ? Instance.metaHpCost : 1000;

    public PlayerProgress CurrentProgress { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (AuthManager.Instance.IsLoggedIn)
        {
            LoadProgress();
        }
    }

    public void LoadProgress()
    {
        StartCoroutine(ApiService.Instance.GetProgress(OnProgressLoaded, OnError));
    }

    private void OnProgressLoaded(PlayerProgress progress)
    {
        CurrentProgress = progress;
        ApplyBonusesToGame();

        OnProgressRefreshed?.Invoke();

        Debug.Log($"[MetaProgression] Загружено: атака +{AttackBonus}, защита +{DefenseBonus}, скорость +{SpeedBonus}, HP +{HpBonus}");
    }

    public void RefreshBonuses()
    {
        if (CurrentProgress == null) return;
        ApplyBonusesToGame();
    }

    private void ApplyBonusesToGame()
    {
        AttackBonus = CurrentProgress.meta_attack_unlocked ? attackBonusValue : 0f;
        DefenseBonus = CurrentProgress.meta_defense_unlocked ? defenseBonusValue : 0f;
        SpeedBonus = CurrentProgress.meta_speed_unlocked ? speedBonusValue : 0f;
        HpBonus = CurrentProgress.meta_max_hp_unlocked ? hpBonusValue : 0f;

        var netManager = (RPGNetworkManager)Mirror.NetworkManager.singleton;
        if (netManager != null)
        {
            netManager.startHealth = 100f + HpBonus;
        }
    }

    private void OnError(string error)
    {
        Debug.LogError($"[MetaProgression] Ошибка: {error}");
    }

    public void SaveEndGameProgress(int currency, int kills, bool victory, int teamLevel, float durationSeconds, int teammateId = 0)
    {
        if (!AuthManager.Instance.IsLoggedIn || CurrentProgress == null) return;

        var update = new ProgressUpdate
        {
            currency = CurrentProgress.currency + currency,
            total_kills = CurrentProgress.total_kills + kills,
            total_games = CurrentProgress.total_games + 1,
            meta_attack_unlocked = CurrentProgress.meta_attack_unlocked,
            meta_defense_unlocked = CurrentProgress.meta_defense_unlocked,
            meta_speed_unlocked = CurrentProgress.meta_speed_unlocked,
            meta_max_hp_unlocked = CurrentProgress.meta_max_hp_unlocked,
        };

        StartCoroutine(SaveCoroutine(update, currency, kills, victory, teamLevel, durationSeconds, teammateId));
    }

    private IEnumerator SaveCoroutine(ProgressUpdate update, int currency, int kills, bool victory, int teamLevel, float durationSeconds, int teammateId)
    {
        yield return ApiService.Instance.UpdateProgress(update,
            result =>
            {
                CurrentProgress.currency = result.currency;
                CurrentProgress.total_kills = result.total_kills;
                CurrentProgress.total_games = result.total_games;
            },
            error => Debug.LogError($"[MetaProgression] Ошибка сохранения: {error}")
        );

        var run = new RunRecord
        {
            duration_seconds = durationSeconds,
            kills = kills,
            victory = victory,
            currency_earned = currency,
            team_level = teamLevel,
            teammate_id = teammateId,
        };

        yield return ApiService.Instance.PostRun(run,
            () => Debug.Log($"[MetaProgression] Забег сохранён"),
            error => Debug.LogError($"[MetaProgression] Ошибка сохранения забега: {error}")
        );
    }

    public void SaveProgressOnly(int currency, int kills)
    {
        if (!AuthManager.Instance.IsLoggedIn || CurrentProgress == null) return;

        var update = new ProgressUpdate
        {
            currency = CurrentProgress.currency + currency,
            total_kills = CurrentProgress.total_kills + kills,
            total_games = CurrentProgress.total_games + 1,
            meta_attack_unlocked = CurrentProgress.meta_attack_unlocked,
            meta_defense_unlocked = CurrentProgress.meta_defense_unlocked,
            meta_speed_unlocked = CurrentProgress.meta_speed_unlocked,
            meta_max_hp_unlocked = CurrentProgress.meta_max_hp_unlocked,
        };

        StartCoroutine(ApiService.Instance.UpdateProgress(update,
            result =>
            {
                CurrentProgress.currency = result.currency;
                CurrentProgress.total_kills = result.total_kills;
                CurrentProgress.total_games = result.total_games;
            },
            error => Debug.LogError($"[MetaProgression] Ошибка сохранения прогресса: {error}")
        ));
    }
}
