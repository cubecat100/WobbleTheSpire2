#nullable enable

namespace WobbleTheSpire2;

/// <summary>
/// WobbleTheSpire2 설정 파일에 저장되는 사용자 옵션 값
/// </summary>
public sealed class WobbleSettings
{
    public const string FileName = "wobblethespire2_settings.cfg";
    private const int MinScalePercent = 70;
    private const int MaxScalePercent = 130;

    public bool EnablePlayerWobble { get; set; } = true;
    public bool BlockBaseHitAnimation { get; set; } = true;
    public bool DisableWobbleOnDeath { get; set; } = true;
    public bool EnableHorizontalWobble { get; set; }
    public bool StrongerWobble { get; set; }
    public bool LongerWobble { get; set; }
    public int OverallWobbleScalePercent { get; set; } = 100;

    /// <summary>
    /// 현재 설정 값을 복사한 새 인스턴스 생성
    /// </summary>
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

    /// <summary>
    /// 전체 wobble 강도 퍼센트 값을 허용 범위로 보정
    /// </summary>
    public static int ScalePercentRangeCheck(int scale)
    {
        return scale switch
        {
            < MinScalePercent => MinScalePercent,
            > MaxScalePercent => MaxScalePercent,
            _ => scale
        };
    }
}
