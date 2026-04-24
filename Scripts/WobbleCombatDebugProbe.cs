using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WobbleTheSpire2;

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

    private readonly Dictionary<NCreature, ActiveWobbleState> _activeWobbles = [];

    public override void _EnterTree()
    {
        Name = NodeName;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        ModEntry.WobbleSystem?.RegisterCombatProbe(this);
        Log.Warn("[WobbleTheSpire2] Combat wobble probe ready.");
    }

    public override void _Process(double delta)
    {
        UpdateActiveWobbles((float)delta);
    }

    public override void _ExitTree()
    {
        ModEntry.WobbleSystem?.UnregisterCombatProbe(this);
        RestoreAllBodies();
        _activeWobbles.Clear();
    }

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

            // Treat the hit like an impulse so the wobble starts as a springy motion, not a held lean.
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

    private void RestoreAllBodies()
    {
        foreach (ActiveWobbleState state in _activeWobbles.Values)
        {
            RestoreBody(state);
        }
    }

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

    private static Node2D? GetCreatureBody(NCreature creatureNode)
    {
        if (creatureNode.Body is not null)
        {
            return creatureNode.Body;
        }

        return creatureNode.Visuals?.GetCurrentBody();
    }

    private static string TrimSource(string source)
    {
        const string prefix = "Creature.LoseHpInternal props=";
        if (source.StartsWith(prefix) == true)
        {
            return source[prefix.Length..];
        }

        return source;
    }

    private static float GetMaxWobbleStrength(WobbleSettings settings)
    {
        return settings.StrongerWobble == true
            ? MaxBoostedWobbleStrength
            : MaxWobbleStrength;
    }

    private static void LogHit(NCreature creatureNode, int damageAmount, string source)
    {
        Log.Warn($"[WobbleTheSpire2] Hit: {creatureNode.Entity.LogName}, dmg={damageAmount}, type={TrimSource(source)}");
    }

    private static DamageWobbleProfile ComputeWobbleProfile(int damageAmount, WobbleSettings settings)
    {
        int clampedDamage = Mathf.Max(0, damageAmount);
        DamageWobbleProfile profile;

        if (clampedDamage >= 25)
        {
            profile = new DamageWobbleProfile(2.95f * BaseStrengthMultiplier, BaseDurationSeconds);
        }
        else if (clampedDamage >= 16)
        {
            profile = new DamageWobbleProfile(2.0f * BaseStrengthMultiplier, BaseDurationSeconds);
        }
        else if (clampedDamage >= 8)
        {
            profile = new DamageWobbleProfile(1.15f * BaseStrengthMultiplier, BaseDurationSeconds);
        }
        else
        {
            profile = new DamageWobbleProfile(0.52f * BaseStrengthMultiplier, BaseDurationSeconds);
        }

        float strength = settings.StrongerWobble == true
            ? profile.Strength * 1.2f
            : profile.Strength;

        strength *= settings.OverallWobbleScalePercent / 100.0f;

        float duration = settings.LongerWobble == true
            ? profile.DurationSeconds * 1.22f
            : profile.DurationSeconds;

        return new DamageWobbleProfile(strength, duration);
    }

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

    private sealed class ActiveWobbleState
    {
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

    private readonly record struct DamageWobbleProfile(float Strength, float DurationSeconds);
}
