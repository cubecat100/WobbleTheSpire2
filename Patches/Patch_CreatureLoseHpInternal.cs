using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace WobbleTheSpire2;

[HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
public static class Patch_CreatureLoseHpInternal
{
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
