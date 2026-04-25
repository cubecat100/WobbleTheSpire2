#nullable enable
using System;

namespace WobbleTheSpire2;

/// <summary>
/// 현재 적용 중인 설정 보관, 저장소와 동기화
/// </summary>
public static class WobbleSettingsManager
{
    private static readonly WobbleSettingsStore Store = WobbleSettingsStore.CreateDefault();
    private static WobbleSettings _current = Store.Load();

    public static event Action? SettingsChanged;

    public static WobbleSettings Current => _current;

    /// <summary>
    /// 설정 파일 다시 읽기, 변경 이벤트 발생
    /// </summary>
    public static WobbleSettings Reload()
    {
        _current = Store.Load();
        SettingsChanged?.Invoke();
        return _current;
    }

    /// <summary>
    /// 설정 파일 저장, 현재 설정으로 반영
    /// </summary>
    public static WobbleSettings Save(WobbleSettings settings)
    {
        _current = Store.Save(settings);
        SettingsChanged?.Invoke();
        return _current;
    }
}
