#nullable enable

namespace WobbleTheSpire2;

public sealed class WobbleSettings
{
    public const string FileName = "wobblethespire2_settings.cfg";
    private const int MinScalePercent = 35;
    private const int MaxScalePercent = 130;

    public bool EnablePlayerWobble { get; set; } = true;
    public bool BlockBaseHitAnimation { get; set; } = true;
    public bool DisableWobbleOnDeath { get; set; } = true;
    public bool EnableHorizontalWobble { get; set; }
    public bool StrongerWobble { get; set; }
    public bool LongerWobble { get; set; }
    public int OverallWobbleScalePercent { get; set; } = 115;

    public WobbleSettings Clone()
    {
        return new WobbleSettings
        {
            EnablePlayerWobble = EnablePlayerWobble,
            BlockBaseHitAnimation = BlockBaseHitAnimation,
            DisableWobbleOnDeath = DisableWobbleOnDeath,
            EnableHorizontalWobble = EnableHorizontalWobble,
            StrongerWobble = StrongerWobble,
            LongerWobble = LongerWobble,
            OverallWobbleScalePercent = OverallWobbleScalePercent
        };
    }

    public WobbleSettings Normalize()
    {
        return new WobbleSettings
        {
            EnablePlayerWobble = EnablePlayerWobble,
            BlockBaseHitAnimation = BlockBaseHitAnimation,
            DisableWobbleOnDeath = DisableWobbleOnDeath,
            EnableHorizontalWobble = EnableHorizontalWobble,
            StrongerWobble = StrongerWobble,
            LongerWobble = LongerWobble,
            OverallWobbleScalePercent = ClampPercent(OverallWobbleScalePercent, 115)
        };
    }

    private static int ClampPercent(int value, int fallback)
    {
        if (value <= 0)
        {
            return fallback;
        }

        return value < MinScalePercent
            ? MinScalePercent
            : value > MaxScalePercent
                ? MaxScalePercent
                : value;
    }
}
