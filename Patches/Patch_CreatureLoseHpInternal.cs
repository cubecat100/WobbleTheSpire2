using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace WobbleTheSpire2;

/// <summary>
/// Creature 체력 감소 후 실제 피해량을 wobble 시스템으로 전달
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
public static class Patch_CreatureLoseHpInternal
{
    /// <summary>
    /// 유효 피해량 확인, 피격 이벤트로 처리
    /// </summary>
    public static void Postfix(Creature __instance, ValueProp props, DamageResult __result)
    {
        if (__result.TotalDamage <= 0)
        {
            return;
        }

        ModEntry.WobbleSystem?.ReportMonsterHit(
            __instance,
            $"Creature.LoseHpInternal props={props}",
            __result.TotalDamage);
    }
}
