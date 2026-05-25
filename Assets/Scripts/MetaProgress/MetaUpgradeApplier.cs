using Mirror;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class MetaUpgradeApplier : NetworkBehaviour
{
    private void Start()
    {
        if (!isServer) return;

        var player = GetComponent<Player>();

        if (MetaProgression.AttackBonus == 0 &&
            MetaProgression.DefenseBonus == 0 &&
            MetaProgression.SpeedBonus == 0)
            return;

        var stats = player.CurrentStats;
        stats.attackPower += MetaProgression.AttackBonus;
        stats.defense += MetaProgression.DefenseBonus;
        stats.moveSpeed += MetaProgression.SpeedBonus;

        player.CurrentStats = stats;
    }
}
