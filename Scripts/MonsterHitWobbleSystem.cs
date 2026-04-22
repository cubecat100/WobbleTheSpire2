using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace WobbleTheSpire2;

public sealed class MonsterHitWobbleSystem
{
    private readonly List<WobbleCombatDebugProbe> _combatProbes = [];

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        if (IsInitialized == true)
        {
            return;
        }

        IsInitialized = true;

        Log.Warn("[WobbleTheSpire2] Monster hit wobble skeleton is ready.");
        Log.Warn("[WobbleTheSpire2] Phase 1: connect a combat hit probe and verify logs.");
        Log.Warn("[WobbleTheSpire2] Phase 3: route actual monster hit events into wobble playback.");
    }

    public void RegisterCombatProbe(WobbleCombatDebugProbe combatProbe)
    {
        if (_combatProbes.Contains(combatProbe) == true)
        {
            return;
        }

        _combatProbes.Add(combatProbe);
        Log.Warn($"[WobbleTheSpire2] Combat probe registered. Active probes={_combatProbes.Count}");
    }

    public void UnregisterCombatProbe(WobbleCombatDebugProbe combatProbe)
    {
        if (_combatProbes.Remove(combatProbe) == false)
        {
            return;
        }

        Log.Warn($"[WobbleTheSpire2] Combat probe unregistered. Active probes={_combatProbes.Count}");
    }

    public void ReportMonsterHit(Creature target, string source, int damageAmount)
    {
        foreach (WobbleCombatDebugProbe combatProbe in _combatProbes.ToArray())
        {
            combatProbe.OnMonsterHit(target, source, damageAmount);
        }
    }
}
