#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;

namespace WobbleTheSpire2;

[HarmonyPatch(typeof(NModdingScreen), "OnRowSelected")]
public static class Patch_ModdingScreenOnRowSelected
{
    public static void Postfix(NModdingScreen __instance, NModMenuRow row)
    {
        WobbleModdingSettingsControl? control =
            __instance.FindChild(WobbleModdingSettingsControl.NodeName, recursive: true, owned: false) as WobbleModdingSettingsControl;

        control?.UpdateSelectedMod(row.Mod);
    }
}
