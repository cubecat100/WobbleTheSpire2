using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WobbleTheSpire2;

/// <summary>
/// 전투방 피격 이벤트 수신, wobble 애니메이션 재생
/// </summary>
public sealed partial class WobbleCombatDebugProbe : Node
{
    public const string NodeName = "WobbleCombatDebugProbe";
    private const float BaseStrengthMultiplier = 1.15f;
    private const float BaseDurationSeconds = 1.02f;
    private const float MaxRotationRadians = 0.68f;
    private const float DefaultPivotOffset = 28.0f;
    private const float PivotPadding = 4.0f;
    private const float MinPivotOffset = 18.0f;
    private const float MaxPivotOffset = 64.0f;
    private const float MaxHorizontalOffset = 24.0f;
    private const float RotationCyclesPerSecond = 3.25f;
    private const float DampingPerSecond = 2.45f;
    private const float MinWobbleStrength = 0.35f;
    private const float MaxWobbleStrength = 3.1f;
    private const float MaxBoostedWobbleStrength = 4.0f;

    private static int[] clampedDamageThreshold = [0, 8, 16, 25];
    private static float[] clampedDamageStrengthMultiplier = [0.52f, 1.15f, 2.0f, 2.95f];

    private readonly Dictionary<NCreature, ActiveWobbleState> _activeWobbles = [];

    /// <summary>
    /// Godot 트리 진입 시 노드 이름과 처리 모드 설정
    /// </summary>
    public override void _EnterTree()
    {
        Name = NodeName;
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary>
    /// 전투방 probe를 wobble 시스템에 등록
    /// </summary>
    public override void _Ready()
    {
        ModEntry.WobbleSystem?.RegisterCombatProbe(this);
        Log.Warn("[WobbleTheSpire2] Combat wobble probe ready.");
    }

    /// <summary>
    /// 매 프레임 활성 wobble 상태 갱신
    /// </summary>
    public override void _Process(double delta)
    {
        UpdateActiveWobbles((float)delta);
    }

    /// <summary>
    /// 노드 제거 시 등록 해제, 변형된 body 복구
    /// </summary>
    public override void _ExitTree()
    {
        ModEntry.WobbleSystem?.UnregisterCombatProbe(this);
        RestoreAllBodies();
        _activeWobbles.Clear();
    }

    /// <summary>
    /// 피격된 Creature를 전투방 NCreature 노드로 변환, wobble 시작
    /// </summary>
    public void OnMonsterHit(Creature target, string source, int damageAmount)
    {
        WobbleSettings settings = WobbleSettingsManager.Current;
        if (target.IsEnemy != true && settings.EnablePlayerWobble != true)
        {
            return;
        }

        if (settings.DisableWobbleOnDeath == true && target.CurrentHp <= 0)
        {
            return;
        }

        NCombatRoom? combatRoom = GetParentOrNull<NCombatRoom>();
        if (combatRoom is null)
        {
            Log.Warn("[WobbleTheSpire2] Hit ignored: combat room is unavailable.");
            return;
        }

        NCreature? creatureNode = combatRoom.GetCreatureNode(target);
        if (creatureNode is null)
        {
            Log.Warn($"[WobbleTheSpire2] Hit received but creature node was not found: target={target.LogName}");
            return;
        }

        DamageWobbleProfile wobbleProfile = ComputeWobbleProfile(damageAmount, settings);
        TriggerWobble(creatureNode, wobbleProfile, source, damageAmount, settings);
    }

    /// <summary>
    /// 대상 body wobble 상태 생성, 기존 wobble 강도와 시간 갱신
    /// </summary>
    private void TriggerWobble(
        NCreature creatureNode,
        DamageWobbleProfile wobbleProfile,
        string source,
        int damageAmount,
        WobbleSettings settings)
    {
        Node2D? body = GetCreatureBody(creatureNode);
        if (body is null)
        {
            Log.Warn("[WobbleTheSpire2] Wobble test skipped: enemy body node was not found.");
            return;
        }

        if (_activeWobbles.TryGetValue(creatureNode, out ActiveWobbleState? existingState) == false)
        {
            existingState = CreateActiveWobbleState(creatureNode, body, wobbleProfile, settings);
            if (existingState is null)
            {
                return;
            }

            _activeWobbles[creatureNode] = existingState;
        }

        float maxWobbleStrength = GetMaxWobbleStrength(settings);
        existingState.Strength = Mathf.Clamp(
            Mathf.Max(existingState.Strength, wobbleProfile.Strength) + (wobbleProfile.Strength * 0.2f),
            Mathf.Clamp(wobbleProfile.Strength, MinWobbleStrength, maxWobbleStrength),
            maxWobbleStrength);
        existingState.TotalDuration = Mathf.Max(existingState.TotalDuration, wobbleProfile.DurationSeconds);
        existingState.TimeRemaining = existingState.TotalDuration;
        LogHit(creatureNode, damageAmount, source);
    }

    /// <summary>
    /// 활성 wobble 목록 순회, 회전과 수평 이동 및 감쇠 적용
    /// </summary>
    private void UpdateActiveWobbles(float delta)
    {
        if (_activeWobbles.Count == 0)
        {
            return;
        }

        List<NCreature> finished = [];

        foreach (KeyValuePair<NCreature, ActiveWobbleState> pair in _activeWobbles)
        {
            ActiveWobbleState state = pair.Value;
            if (GodotObject.IsInstanceValid(state.CreatureNode) == false
                || GodotObject.IsInstanceValid(state.Body) == false
                || GodotObject.IsInstanceValid(state.PivotWrapper) == false)
            {
                RestoreBody(state);
                finished.Add(pair.Key);
                continue;
            }

            state.TimeRemaining = Mathf.Max(0.0f, state.TimeRemaining - delta);
            float elapsed = state.TotalDuration - state.TimeRemaining;
            float phase = elapsed * Mathf.Pi * 2.0f * RotationCyclesPerSecond;
            float envelope = Mathf.Exp(-DampingPerSecond * elapsed) * state.Strength;
            WobbleSettings settings = WobbleSettingsManager.Current;

            // 피격을 순간 충격으로 처리, 스프링 같은 첫 흔들림 생성
            float directionalEnvelope = envelope * state.ImpulseDirection;
            float rotation = Mathf.Sin(phase) * MaxRotationRadians * directionalEnvelope;
            float horizontalOffset = settings.EnableHorizontalWobble == true
                ? Mathf.Sin(phase + 0.45f) * MaxHorizontalOffset * directionalEnvelope
                : 0.0f;

            state.PivotWrapper.Rotation = state.BaseWrapperRotation + rotation;
            state.PivotWrapper.Position = state.BaseWrapperPosition + new Vector2(horizontalOffset, 0.0f);
            state.Body.Position = state.BaseBodyPosition;
            state.Body.Rotation = state.BaseBodyRotation;
            state.Body.Scale = state.BaseBodyScale;

            if (state.TimeRemaining <= 0.0f)
            {
                RestoreBody(state);
                finished.Add(pair.Key);
            }
        }

        foreach (NCreature creatureNode in finished)
        {
            _activeWobbles.Remove(creatureNode);
        }
    }

    /// <summary>
    /// 현재 활성 wobble 대상 전체 body 복구
    /// </summary>
    private void RestoreAllBodies()
    {
        foreach (ActiveWobbleState state in _activeWobbles.Values)
        {
            RestoreBody(state);
        }
    }

    /// <summary>
    /// wobble 적용을 위해 감싼 body를 원래 부모 노드 위치로 복구
    /// </summary>
    private static void RestoreBody(ActiveWobbleState state)
    {
        if (GodotObject.IsInstanceValid(state.Body) == false)
        {
            return;
        }

        state.Body.Position = state.OriginalBodyPosition;
        state.Body.Rotation = state.OriginalBodyRotation;
        state.Body.Scale = state.OriginalBodyScale;

        if (GodotObject.IsInstanceValid(state.PivotWrapper) == false || GodotObject.IsInstanceValid(state.OriginalParent) == false)
        {
            return;
        }

        Node currentParent = state.Body.GetParent();
        if (ReferenceEquals(currentParent, state.OriginalParent) == false)
        {
            currentParent?.RemoveChild(state.Body);
            state.OriginalParent.AddChild(state.Body);
        }

        int targetIndex = Mathf.Clamp(state.OriginalBodyIndex, 0, Mathf.Max(0, state.OriginalParent.GetChildCount() - 1));
        state.OriginalParent.MoveChild(state.Body, targetIndex);

        Node pivotParent = state.PivotWrapper.GetParent();
        pivotParent?.RemoveChild(state.PivotWrapper);
        state.PivotWrapper.QueueFree();
    }

    /// <summary>
    /// NCreature에서 실제 시각 body 노드 검색
    /// </summary>
    private static Node2D? GetCreatureBody(NCreature creatureNode)
    {
        if (creatureNode.Body is not null)
        {
            return creatureNode.Body;
        }

        return creatureNode.Visuals?.GetCurrentBody();
    }

    /// <summary>
    /// 로그 표시용 피격 원본 문자열 정리
    /// </summary>
    private static string TrimSource(string source)
    {
        const string prefix = "Creature.LoseHpInternal props=";
        if (source.StartsWith(prefix) == true)
        {
            return source[prefix.Length..];
        }

        return source;
    }

    /// <summary>
    /// 현재 설정 기준, 최대 wobble 강도 반환
    /// </summary>
    private static float GetMaxWobbleStrength(WobbleSettings settings)
    {
        return settings.StrongerWobble == true
            ? MaxBoostedWobbleStrength
            : MaxWobbleStrength;
    }

    /// <summary>
    /// 피격 감지 결과 로그 출력
    /// </summary>
    private static void LogHit(NCreature creatureNode, int damageAmount, string source)
    {
        Log.Warn($"[WobbleTheSpire2] Hit: {creatureNode.Entity.LogName}, dmg={damageAmount}, type={TrimSource(source)}");
    }

    /// <summary>
    /// 피해량 구간과 설정 옵션 기준, wobble 강도와 지속 시간 계산
    /// </summary>
    private static DamageWobbleProfile ComputeWobbleProfile(int damageAmount, WobbleSettings settings)
    {
        int clampedDamage = Mathf.Max(0, damageAmount);
        DamageWobbleProfile profile;

        profile = DamageProfileRes(clampedDamage);

        float strength = settings.StrongerWobble == true
            ? profile.Strength * 1.2f
            : profile.Strength;

        strength *= settings.OverallWobbleScalePercent / 100.0f;

        float duration = settings.LongerWobble == true
            ? profile.DurationSeconds * 1.22f
            : profile.DurationSeconds;

        return new DamageWobbleProfile(strength, duration);
    }

    /// <summary>
    /// 피해량 프로필 구성, 구간별로 미리 정의된 강도 배율과 기본 지속 시간 반환
    /// </summary>
    private static DamageWobbleProfile DamageProfileRes(int clampedDamage)
    {
        for(int i = clampedDamageThreshold.Length - 1; i > 0; i--)
        {
            if (clampedDamage >= clampedDamageThreshold[i])
            {
                return new DamageWobbleProfile(clampedDamageStrengthMultiplier[i] * BaseStrengthMultiplier, BaseDurationSeconds);
            }
        }

        return new DamageWobbleProfile(clampedDamageStrengthMultiplier[0] * BaseStrengthMultiplier, BaseDurationSeconds);
    }

    /// <summary>
    /// body를 피벗 래퍼 아래로 이동, 하단 기준 회전 wobble 준비
    /// </summary>
    private static ActiveWobbleState? CreateActiveWobbleState(
        NCreature creatureNode,
        Node2D body,
        DamageWobbleProfile wobbleProfile,
        WobbleSettings settings)
    {
        Node? originalParent = body.GetParent();
        if (originalParent is null)
        {
            Log.Warn("[WobbleTheSpire2] Wobble skipped: body parent node was not found.");
            return null;
        }

        int originalBodyIndex = body.GetIndex();
        Vector2 originalBodyPosition = body.Position;
        float originalBodyRotation = body.Rotation;
        Vector2 originalBodyScale = body.Scale;

        float pivotOffset = EstimatePivotOffset(body);
        float impulseDirection = DetermineImpulseDirection(creatureNode, body);
        Node2D pivotWrapper = new()
        {
            Name = $"{body.Name}_WobblePivot",
            Position = originalBodyPosition + new Vector2(0.0f, pivotOffset)
        };

        // body 자체 원점 대신 하단 피벗 기준으로 흔들리도록 임시 래퍼 노드 삽입
        originalParent.AddChild(pivotWrapper);
        originalParent.MoveChild(pivotWrapper, originalBodyIndex);
        originalParent.RemoveChild(body);
        pivotWrapper.AddChild(body);

        body.Position = new Vector2(0.0f, -pivotOffset);
        body.Rotation = originalBodyRotation;
        body.Scale = originalBodyScale;

        return new ActiveWobbleState(
            creatureNode,
            body,
            pivotWrapper,
            originalParent,
            originalBodyIndex,
            originalBodyPosition,
            originalBodyRotation,
            originalBodyScale,
            pivotWrapper.Position,
            pivotWrapper.Rotation,
            body.Position,
            body.Rotation,
            body.Scale,
            impulseDirection,
            Mathf.Clamp(wobbleProfile.Strength, MinWobbleStrength, GetMaxWobbleStrength(settings)),
            wobbleProfile.DurationSeconds,
            wobbleProfile.DurationSeconds);
    }

    /// <summary>
    /// 적과 플레이어의 첫 충격 방향 결정
    /// </summary>
    private static float DetermineImpulseDirection(NCreature creatureNode, Node2D body)
    {
        if (creatureNode.Entity?.IsEnemy == true)
        {
            return 1.0f;
        }

        if (Mathf.IsZeroApprox(body.Scale.X) == false)
        {
            return body.Scale.X > 0.0f
                ? -1.0f
                : 1.0f;
        }

        return -1.0f;
    }

    /// <summary>
    /// 시각 노드 경계 기준, 회전 피벗 하단 오프셋 추정
    /// </summary>
    private static float EstimatePivotOffset(Node2D body)
    {
        float maxY = float.MinValue;
        bool hasVisualBounds = false;
        CollectLowerExtent(body, Transform2D.Identity, ref maxY, ref hasVisualBounds);

        if (hasVisualBounds == false)
        {
            return DefaultPivotOffset;
        }

        return Mathf.Clamp(maxY + PivotPadding, MinPivotOffset, MaxPivotOffset);
    }

    /// <summary>
    /// body 하위 노드 순회, 시각적 최하단 Y 좌표 수집
    /// </summary>
    private static void CollectLowerExtent(Node node, Transform2D transformToBody, ref float maxY, ref bool hasVisualBounds)
    {
        if (node is not Node2D node2D)
        {
            return;
        }

        Transform2D nodeTransformToBody = transformToBody * node2D.Transform;
        if (node is Sprite2D sprite && sprite.Texture is not null)
        {
            Rect2 rect = sprite.GetRect();
            IncludeRect(rect, nodeTransformToBody, ref maxY);
            hasVisualBounds = true;
        }
        else if (ReferenceEquals(node, node2D.Owner) == false)
        {
            maxY = Mathf.Max(maxY, nodeTransformToBody.Origin.Y);
        }

        foreach (Node child in node.GetChildren())
        {
            CollectLowerExtent(child, nodeTransformToBody, ref maxY, ref hasVisualBounds);
        }
    }

    /// <summary>
    /// Sprite2D 사각형 모서리 변환, 하단 경계 계산에 포함
    /// </summary>
    private static void IncludeRect(Rect2 rect, Transform2D transformToBody, ref float maxY)
    {
        Vector2[] corners =
        [
            rect.Position,
            rect.Position + new Vector2(rect.Size.X, 0.0f),
            rect.Position + rect.Size,
            rect.Position + new Vector2(0.0f, rect.Size.Y)
        ];

        foreach (Vector2 corner in corners)
        {
            Vector2 transformedCorner = transformToBody * corner;
            maxY = Mathf.Max(maxY, transformedCorner.Y);
        }
    }

    /// <summary>
    /// wobble 진행 중 원본 노드 상태와 현재 애니메이션 값 보관
    /// </summary>
    private sealed class ActiveWobbleState
    {
        /// <summary>
        /// wobble 대상과 복구용 원본 상태 저장
        /// </summary>
        public ActiveWobbleState(
            NCreature creatureNode,
            Node2D body,
            Node2D pivotWrapper,
            Node originalParent,
            int originalBodyIndex,
            Vector2 originalBodyPosition,
            float originalBodyRotation,
            Vector2 originalBodyScale,
            Vector2 baseWrapperPosition,
            float baseWrapperRotation,
            Vector2 baseBodyPosition,
            float baseBodyRotation,
            Vector2 baseBodyScale,
            float impulseDirection,
            float strength,
            float totalDuration,
            float timeRemaining)
        {
            CreatureNode = creatureNode;
            Body = body;
            PivotWrapper = pivotWrapper;
            OriginalParent = originalParent;
            OriginalBodyIndex = originalBodyIndex;
            OriginalBodyPosition = originalBodyPosition;
            OriginalBodyRotation = originalBodyRotation;
            OriginalBodyScale = originalBodyScale;
            BaseWrapperPosition = baseWrapperPosition;
            BaseWrapperRotation = baseWrapperRotation;
            BaseBodyPosition = baseBodyPosition;
            BaseBodyRotation = baseBodyRotation;
            BaseBodyScale = baseBodyScale;
            ImpulseDirection = impulseDirection;
            Strength = strength;
            TotalDuration = totalDuration;
            TimeRemaining = timeRemaining;
        }

        public NCreature CreatureNode { get; }
        public Node2D Body { get; }
        public Node2D PivotWrapper { get; }
        public Node OriginalParent { get; }
        public int OriginalBodyIndex { get; }
        public Vector2 OriginalBodyPosition { get; }
        public float OriginalBodyRotation { get; }
        public Vector2 OriginalBodyScale { get; }
        public Vector2 BaseWrapperPosition { get; }
        public float BaseWrapperRotation { get; }
        public Vector2 BaseBodyPosition { get; }
        public float BaseBodyRotation { get; }
        public Vector2 BaseBodyScale { get; }
        public float ImpulseDirection { get; }
        public float Strength { get; set; }
        public float TotalDuration { get; set; }
        public float TimeRemaining { get; set; }
    }

    /// <summary>
    /// 피해량 기준으로 계산된 wobble 강도와 지속 시간 값
    /// </summary>
    private readonly record struct DamageWobbleProfile(float Strength, float DurationSeconds);
}
