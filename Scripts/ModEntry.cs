using HarmonyLib;
using System;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace WobbleTheSpire2;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    private static Harmony? _harmony;

    internal static MonsterHitWobbleSystem? WobbleSystem { get; private set; }

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
