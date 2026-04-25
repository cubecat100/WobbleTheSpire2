using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WobbleTheSpire2;

/// <summary>
/// 전투방 준비 시 wobble 이벤트 처리 probe 노드 연결
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), "_Ready")]
public static class Patch_CombatRoomReady
{
    /// <summary>
    /// 전투방 probe 검색, 없으면 새 probe 추가
    /// </summary>
    public static void Postfix(NCombatRoom __instance)
    {
        if (__instance.GetNodeOrNull<WobbleCombatDebugProbe>(WobbleCombatDebugProbe.NodeName) is not null)
        {
            Log.Warn("[WobbleTheSpire2] Combat debug probe already attached.");
            return;
        }

        __instance.AddChild(new WobbleCombatDebugProbe());
        Log.Warn("[WobbleTheSpire2] Combat debug probe attached.");
    }
}
