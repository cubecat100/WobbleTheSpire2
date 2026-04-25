#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;

namespace WobbleTheSpire2;

/// <summary>
/// 모드 설정 화면 선택 변경 시 Wobble 설정 패널 표시 갱신
/// </summary>
[HarmonyPatch(typeof(NModdingScreen), "OnRowSelected")]
public static class Patch_ModdingScreenOnRowSelected
{
    /// <summary>
    /// 현재 선택된 모드 정보를 설정 패널에 전달
    /// </summary>
    public static void Postfix(NModdingScreen __instance, NModMenuRow row)
    {
        WobbleModdingSettingsControl? control =
            __instance.FindChild(WobbleModdingSettingsControl.NodeName, recursive: true, owned: false) as WobbleModdingSettingsControl;

        control?.UpdateSelectedMod(row.Mod);
    }
}
