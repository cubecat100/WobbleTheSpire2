using HarmonyLib;
using System;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace WobbleTheSpire2;

/// <summary>
/// 모드 로드 진입점, Harmony 패치와 wobble 시스템 초기화
/// </summary>
[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    private static Harmony? _harmony;

    internal static MonsterHitWobbleSystem? WobbleSystem { get; private set; }

    /// <summary>
    /// Wobble 시스템 생성, Harmony 패치 등록
    /// </summary>
    public static void Initialize()
    {
        Log.Warn("[WobbleTheSpire2] Initialize");

        WobbleSystem = new MonsterHitWobbleSystem();
        WobbleSystem.Initialize();

        try
        {
            _harmony ??= new Harmony("wobblethespire2.mod");
            _harmony.PatchAll();
            Log.Warn("[WobbleTheSpire2] Harmony patches applied");
        }
        catch (Exception ex)
        {
            Log.Error($"[WobbleTheSpire2] Harmony patch initialization failed: {ex}");
            throw;
        }
    }
}
