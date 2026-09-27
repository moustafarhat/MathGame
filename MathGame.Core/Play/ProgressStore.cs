using System.Text.Json;

namespace MathGame.Core.Play;

/// <summary>Remembers the best star rating per stage in a small JSON file.</summary>
public sealed class ProgressStore
{
    private readonly string _path;
    private readonly Dictionary<string, int> _stars;

    public ProgressStore(string path)
    {
        _path = path;
        _stars = Load(path);
    }

    /// <summary>%LOCALAPPDATA% on Windows, ~/.local/share on Linux, ~/Library/Application Support on macOS.</summary>
    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MathGame",
        "progress.json");

    public int TotalStars => _stars.Values.Sum();

    public int StarsFor(string stageId) => _stars.GetValueOrDefault(stageId);

    /// <summary>Saves the result if it beats the stored one. Returns true when it did.</summary>
    public bool Record(string stageId, StageResult result)
    {
        if (result.Stars <= StarsFor(stageId))
        {
            return false;
        }

        _stars[stageId] = result.Stars;
        Save();
        return true;
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);

        // Write to a temp file first so a crash mid-write can't corrupt existing progress.
        var temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(new ProgressFile(_stars)));
        File.Move(temp, _path, overwrite: true);
    }

    private static Dictionary<string, int> Load(string path)
    {
        try
        {
            var file = JsonSerializer.Deserialize<ProgressFile>(File.ReadAllText(path));
            return new Dictionary<string, int>(file?.Stars ?? []);
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            // Missing or unreadable progress just means starting fresh.
            return [];
        }
    }

    private sealed record ProgressFile(Dictionary<string, int> Stars);
}
