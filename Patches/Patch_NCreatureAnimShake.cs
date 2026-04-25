using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WobbleTheSpire2;

/// <summary>
/// 원본 AnimShake 호출 가로채기, 기본 피격 흔들림 차단 적용
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.AnimShake))]
public static class Patch_NCreatureAnimShake
{
    /// <summary>
    /// 원본 shake 허용 여부 확인, 원본 메서드 실행 제어
    /// </summary>
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
