using System.Text.Json;
using System.Text.Json.Serialization;
using MathGame.Core.Skills;

namespace MathGame.Core.Stages;

/// <param name="Skills">Skill ids; each question picks one of them at random.</param>
/// <param name="Questions">How many questions the stage has.</param>
/// <param name="Boss">The last stage of a world, mixing its skills at a higher difficulty.</param>
public sealed record Stage(
    string Id,
    string Title,
    IReadOnlyList<string> Skills,
    Difficulty Difficulty,
    int Questions,
    bool Boss = false);

public sealed record World(string Id, string Title, string Description, IReadOnlyList<Stage> Stages);

/// <summary>All worlds and stages, in play order. The default set is embedded from stages.json.</summary>
public sealed class StageCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly List<Stage> _stages;

    public StageCatalog(IReadOnlyList<World> worlds)
    {
        Worlds = worlds;
        _stages = worlds.SelectMany(w => w.Stages).ToList();
    }

    public IReadOnlyList<World> Worlds { get; }

    public IReadOnlyList<Stage> Stages => _stages;

    public static StageCatalog LoadDefault()
    {
        using var stream = typeof(StageCatalog).Assembly.GetManifestResourceStream("MathGame.Core.stages.json")
            ?? throw new InvalidOperationException("stages.json is not embedded.");
        return Load(stream);
    }

    public static StageCatalog Load(Stream json)
    {
        var file = JsonSerializer.Deserialize<CatalogFile>(json, JsonOptions)
            ?? throw new InvalidDataException("stages.json is empty.");
        return new StageCatalog(file.Worlds);
    }

    public Stage Get(string stageId) =>
        _stages.FirstOrDefault(s => s.Id == stageId)
        ?? throw new KeyNotFoundException($"Unknown stage '{stageId}'.");

    public Stage? NextAfter(Stage stage)
    {
        var index = _stages.IndexOf(stage);
        return index >= 0 && index + 1 < _stages.Count ? _stages[index + 1] : null;
    }

    /// <summary>Stages unlock in order: the first is always open, each later one needs a star on the one before.</summary>
    public bool IsUnlocked(Stage stage, Func<string, int> starsFor)
    {
        var index = _stages.IndexOf(stage);
        return index == 0 || (index > 0 && starsFor(_stages[index - 1].Id) > 0);
    }

    /// <summary>Returns a description of every problem in the catalog; empty when it is valid.</summary>
    public IReadOnlyList<string> Validate(SkillRegistry skills)
    {
        var errors = new List<string>();

        foreach (var duplicate in _stages.GroupBy(s => s.Id).Where(g => g.Count() > 1))
        {
            errors.Add($"Stage id '{duplicate.Key}' is used more than once.");
        }

        foreach (var stage in _stages)
        {
            if (stage.Questions < 1)
            {
                errors.Add($"Stage '{stage.Id}' needs at least one question.");
            }

            if (stage.Skills.Count == 0)
            {
                errors.Add($"Stage '{stage.Id}' has no skills.");
            }

            errors.AddRange(stage.Skills
                .Where(id => !skills.Contains(id))
                .Select(id => $"Stage '{stage.Id}' uses unknown skill '{id}'."));
        }

        return errors;
    }

    private sealed record CatalogFile(IReadOnlyList<World> Worlds);
}
