#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;

namespace WobbleTheSpire2;

public static class WobbleModReflection
{
    private static readonly FieldInfo? ManifestField = AccessTools.Field(typeof(Mod), "manifest");
    private static readonly FieldInfo? ManifestIdField = AccessTools.Field(typeof(ModManifest), "id");

    public static string? GetModId(Mod mod)
    {
        object? manifest = ManifestField?.GetValue(mod);
        return ManifestIdField?.GetValue(manifest) as string;
    }
}
