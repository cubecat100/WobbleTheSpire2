#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;

namespace WobbleTheSpire2;

/// <summary>
/// 게임 Mod 객체에서 공개 API로 접근하기 어려운 manifest 정보 읽기
/// </summary>
public static class WobbleModReflection
{
    private static readonly FieldInfo? ManifestField = AccessTools.Field(typeof(Mod), "manifest");
    private static readonly FieldInfo? ManifestIdField = AccessTools.Field(typeof(ModManifest), "id");

    /// <summary>
    /// Mod 인스턴스의 manifest id 값 반환
    /// </summary>
    public static string? GetModId(Mod mod)
    {
        object? manifest = ManifestField?.GetValue(mod);
        return ManifestIdField?.GetValue(manifest) as string;
    }
}
