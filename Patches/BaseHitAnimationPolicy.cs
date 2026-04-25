using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WobbleTheSpire2;

/// <summary>
/// 기본 피격 애니메이션 유지 여부 판단
/// </summary>
internal static class BaseHitAnimationPolicy
{
    /// <summary>
    /// 현재 설정과 대상 상태 기준, 원본 shake 애니메이션 허용 여부 반환
    /// </summary>
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

    /// <summary>
    /// 애니메이션 트리거 이름 기준, 피격 계열 트리거 차단 여부 반환
    /// </summary>
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
