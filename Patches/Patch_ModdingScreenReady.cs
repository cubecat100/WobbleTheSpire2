#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using System.Reflection;

namespace WobbleTheSpire2;

[HarmonyPatch(typeof(NModdingScreen), "_Ready")]
public static class Patch_ModdingScreenReady
{
    private static readonly FieldInfo? ModInfoContainerField = AccessTools.Field(typeof(NModdingScreen), "_modInfoContainer");

    public static void Postfix(NModdingScreen __instance)
    {
        if (__instance.FindChild(WobbleModdingSettingsControl.NodeName, recursive: true, owned: false) is WobbleModdingSettingsControl)
        {
            return;
        }

        NModInfoContainer? infoContainer = ModInfoContainerField?.GetValue(__instance) as NModInfoContainer;
        if (infoContainer is null)
        {
            return;
        }

        WobbleModdingSettingsControl control = new();
        infoContainer.AddChild(control);
        control.Initialize();
    }
}
