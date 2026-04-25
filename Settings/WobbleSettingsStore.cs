#nullable enable
using System;
using System.IO;
using System.Text.Json;

namespace WobbleTheSpire2;

/// <summary>
/// Wobble 설정을 JSON 형식의 로컬 설정 파일로 읽기/쓰기
/// </summary>
public sealed class WobbleSettingsStore
{
    private readonly string _filePath;

    /// <summary>
    /// 지정한 파일 경로를 사용하는 설정 저장소 생성
    /// </summary>
    public WobbleSettingsStore(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// 설정 파일 읽기, 실패 시 기본 설정 반환
    /// </summary>
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

            if (settings == null)
            {
                return new WobbleSettings();
            }

            settings.OverallWobbleScalePercent = WobbleSettings.ScalePercentRangeCheck(settings.OverallWobbleScalePercent);
            return settings;
        }
        catch
        {
            return new WobbleSettings();
        }
    }

    /// <summary>
    /// 설정 정규화 후 파일 저장
    /// </summary>
    public WobbleSettings Save(WobbleSettings settings)
    {
        WobbleSettings settingClone = settings.Clone();
        string? directoryPath = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directoryPath) == false)
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonSerializer.Serialize(settingClone, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
        return settingClone;
    }

    /// <summary>
    /// 모드 DLL 폴더에 설정 파일을 저장하는 기본 저장소 생성
    /// </summary>
    public static WobbleSettingsStore CreateDefault()
    {
        string assemblyDirectory = Path.GetDirectoryName(typeof(WobbleSettingsStore).Assembly.Location) ?? AppContext.BaseDirectory;
        string filePath = Path.Combine(assemblyDirectory, WobbleSettings.FileName);
        return new WobbleSettingsStore(filePath);
    }
}
