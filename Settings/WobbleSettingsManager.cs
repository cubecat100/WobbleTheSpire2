#nullable enable
using System;

namespace WobbleTheSpire2;

public static class WobbleSettingsManager
{
    private static readonly WobbleSettingsStore Store = WobbleSettingsStore.CreateDefault();
    private static WobbleSettings _current = Store.Load();

    public static event Action? SettingsChanged;

    public static WobbleSettings Current => _current;

    public static WobbleSettings Reload()
    {
        _current = Store.Load();
        SettingsChanged?.Invoke();
        return _current;
    }

    public static WobbleSettings Save(WobbleSettings settings)
    {
        _current = Store.Save(settings);
        SettingsChanged?.Invoke();
        return _current;
    }
}
