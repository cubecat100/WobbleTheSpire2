#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using System.Reflection;

namespace WobbleTheSpire2;

/// <summary>
/// 모드 설정 화면에 WobbleTheSpire2 전용 설정 패널 삽입
/// </summary>
[HarmonyPatch(typeof(NModdingScreen), "_Ready")]
public static class Patch_ModdingScreenReady
{
    private static readonly FieldInfo? ModInfoContainerField = AccessTools.Field(typeof(NModdingScreen), "_modInfoContainer");

    /// <summary>
    /// 게임 내부 모드 정보 컨테이너 검색, 설정 컨트롤 추가
    /// </summary>
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
