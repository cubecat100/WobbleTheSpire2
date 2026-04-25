using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace WobbleTheSpire2;

/// <summary>
/// 피격 이벤트를 현재 전투방 probe들에게 전달하는 중앙 라우터
/// </summary>
public sealed class MonsterHitWobbleSystem
{
    private readonly List<WobbleCombatDebugProbe> _combatProbes = [];

    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 시스템 초기화, 준비 로그 출력
    /// </summary>
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

    /// <summary>
    /// 전투방에 연결된 probe 등록
    /// </summary>
    public void RegisterCombatProbe(WobbleCombatDebugProbe combatProbe)
    {
        if (_combatProbes.Contains(combatProbe) == true)
        {
            return;
        }

        _combatProbes.Add(combatProbe);
        Log.Warn($"[WobbleTheSpire2] Combat probe registered. Active probes={_combatProbes.Count}");
    }

    /// <summary>
    /// 전투방에서 빠지는 probe 등록 해제
    /// </summary>
    public void UnregisterCombatProbe(WobbleCombatDebugProbe combatProbe)
    {
        if (_combatProbes.Remove(combatProbe) == false)
        {
            return;
        }

        Log.Warn($"[WobbleTheSpire2] Combat probe unregistered. Active probes={_combatProbes.Count}");
    }

    /// <summary>
    /// 감지된 피격 정보를 현재 활성 전투방 probe들에게 전달
    /// </summary>
    public void ReportMonsterHit(Creature target, string source, int damageAmount)
    {
        foreach (WobbleCombatDebugProbe combatProbe in _combatProbes.ToArray())
        {
            combatProbe.OnMonsterHit(target, source, damageAmount);
        }
    }
}
