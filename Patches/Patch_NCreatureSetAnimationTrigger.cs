using HarmonyLib;
using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WobbleTheSpire2;

[HarmonyPatch]
public static class Patch_NCreatureSetAnimationTrigger
{
    public static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(NCreature), nameof(NCreature.SetAnimationTrigger), new[] { typeof(string) });
    }

    public static bool Prefix(NCreature __instance, string trigger)
    {
        WobbleSettings settings = WobbleSettingsManager.Current;
        bool shouldBlock = BaseHitAnimationPolicy.ShouldBlockAnimationTrigger(__instance, trigger, settings);

        if (__instance.Entity is not null)
        {
            Log.Warn($"[WobbleTheSpire2] SetAnimationTrigger: target={__instance.Entity.LogName}, trigger={trigger}, blocked={shouldBlock}");
        }

        return shouldBlock == false;
    }
}
