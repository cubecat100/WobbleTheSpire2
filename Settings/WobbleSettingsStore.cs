#nullable enable
using System;
using System.IO;
using System.Text.Json;

namespace WobbleTheSpire2;

public sealed class WobbleSettingsStore
{
    private readonly string _filePath;

    public WobbleSettingsStore(string filePath)
    {
        _filePath = filePath;
    }

    public WobbleSettings Load()
    {
        if (File.Exists(_filePath) == false)
        {
            return new WobbleSettings();
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            WobbleSettings? settings = JsonSerializer.Deserialize<WobbleSettings>(json);
            return (settings ?? new WobbleSettings()).Normalize();
        }
        catch
        {
            return new WobbleSettings();
        }
    }

    public WobbleSettings Save(WobbleSettings settings)
    {
        WobbleSettings normalized = settings.Normalize();
        string? directoryPath = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directoryPath) == false)
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonSerializer.Serialize(normalized, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
        return normalized;
    }

    public static WobbleSettingsStore CreateDefault()
    {
        string assemblyDirectory = Path.GetDirectoryName(typeof(WobbleSettingsStore).Assembly.Location) ?? AppContext.BaseDirectory;
        string filePath = Path.Combine(assemblyDirectory, WobbleSettings.FileName);
        return new WobbleSettingsStore(filePath);
    }
}
