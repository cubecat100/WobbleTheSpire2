using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WobbleTheSpire2;

internal static class BaseHitAnimationPolicy
{
    public static bool AllowOriginalShake(NCreature creatureNode, WobbleSettings settings)
    {
        if (settings.BlockBaseHitAnimation != true)
        {
            return true;
        }

        if (creatureNode.Entity is null)
        {
            return true;
        }

        if (settings.DisableWobbleOnDeath == true && creatureNode.Entity.CurrentHp <= 0)
        {
            return true;
        }

        if (creatureNode.Entity.IsEnemy != true)
        {
            return settings.EnablePlayerWobble != true;
        }

        return false;
    }

    public static bool ShouldBlockAnimationTrigger(NCreature creatureNode, string triggerName, WobbleSettings settings)
    {
        if (AllowOriginalShake(creatureNode, settings) == true)
        {
            return false;
        }

        string normalized = triggerName.Trim().ToLowerInvariant();
        return normalized.Contains("hurt") == true
            || normalized.Contains("hit") == true;
    }
}
