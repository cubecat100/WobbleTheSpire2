using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WobbleTheSpire2;

[HarmonyPatch(typeof(NCombatRoom), "_Ready")]
public static class Patch_CombatRoomReady
{
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
