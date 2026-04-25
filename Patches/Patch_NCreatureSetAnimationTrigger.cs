using HarmonyLib;
using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WobbleTheSpire2;

/// <summary>
/// 피격 애니메이션 트리거 가로채기, 원본 Hit/Hurt 애니메이션 차단
/// </summary>
[HarmonyPatch]
public static class Patch_NCreatureSetAnimationTrigger
{
    /// <summary>
    /// 문자열 트리거를 받는 SetAnimationTrigger 오버로드 선택
    /// </summary>
    public static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(NCreature), nameof(NCreature.SetAnimationTrigger), new[] { typeof(string) });
    }

    /// <summary>
    /// 피격 트리거 차단 여부 확인, 원본 메서드 실행 제어
    /// </summary>
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
