using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WobbleTheSpire2;

[HarmonyPatch(typeof(NCreature), nameof(NCreature.AnimShake))]
public static class Patch_NCreatureAnimShake
{
    public static bool Prefix(NCreature __instance)
    {
        WobbleSettings settings = WobbleSettingsManager.Current;
        bool allowOriginal = BaseHitAnimationPolicy.AllowOriginalShake(__instance, settings);

        if (__instance.Entity is not null)
        {
            Log.Warn($"[WobbleTheSpire2] AnimShake: target={__instance.Entity.LogName}, enemy={__instance.Entity.IsEnemy}, allowOriginal={allowOriginal}, hp={__instance.Entity.CurrentHp}");
        }

        return allowOriginal;
    }
}
