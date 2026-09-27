using System.Text;
using MathGame.Core.Play;
using MathGame.Core.Skills;
using MathGame.Core.Stages;

namespace MathGame.Tests;

public class StageCatalogTests
{
    private static readonly StageCatalog Catalog = StageCatalog.LoadDefault();

    [Fact]
    public void Default_catalog_is_valid() =>
        Assert.Empty(Catalog.Validate(SkillRegistry.Default));

    [Fact]
    public void Default_catalog_has_two_worlds_each_ending_in_a_boss()
    {
        Assert.Equal(["arithmetic", "numbers"], Catalog.Worlds.Select(w => w.Id));
        Assert.All(Catalog.Worlds, w =>
        {
            Assert.True(w.Stages[^1].Boss);
            Assert.Single(w.Stages, s => s.Boss);
        });
    }

    [Fact]
    public void Every_skill_is_used_somewhere() =>
        Assert.Equal(
            SkillRegistry.Default.All.Select(s => s.Id).Order(),
            Catalog.Stages.SelectMany(s => s.Skills).Distinct().Order());

    [Fact]
    public void Validate_reports_problems()
    {
        var catalog = Load("""
            { "worlds": [ { "id": "w", "title": "W", "description": "", "stages": [
                { "id": "a", "title": "A", "skills": ["nope"], "difficulty": "easy", "questions": 0 },
                { "id": "a", "title": "A", "skills": [], "difficulty": "hard", "questions": 3 }
            ] } ] }
            """);

        var errors = catalog.Validate(SkillRegistry.Default);

        Assert.Contains(errors, e => e.Contains("used more than once"));
        Assert.Contains(errors, e => e.Contains("unknown skill 'nope'"));
        Assert.Contains(errors, e => e.Contains("at least one question"));
        Assert.Contains(errors, e => e.Contains("no skills"));
    }

    [Fact]
    public void Load_reads_difficulty_and_optional_boss_flag()
    {
        var catalog = Load("""
            { "worlds": [ { "id": "w", "title": "W", "description": "d", "stages": [
                { "id": "a", "title": "A", "skills": ["prime-or-not"], "difficulty": "medium", "questions": 5 },
                { "id": "b", "title": "B", "skills": ["prime-or-not"], "difficulty": "hard", "questions": 5, "boss": true }
            ] } ] }
            """);

        Assert.Equal(Difficulty.Medium, catalog.Get("a").Difficulty);
        Assert.False(catalog.Get("a").Boss);
        Assert.True(catalog.Get("b").Boss);
    }

    [Fact]
    public void Stages_unlock_one_after_another_across_worlds()
    {
        var stars = new Dictionary<string, int>();
        int StarsFor(string id) => stars.GetValueOrDefault(id);

        Assert.True(Catalog.IsUnlocked(Catalog.Stages[0], StarsFor));
        Assert.False(Catalog.IsUnlocked(Catalog.Stages[1], StarsFor));

        stars[Catalog.Stages[0].Id] = 1;
        Assert.True(Catalog.IsUnlocked(Catalog.Stages[1], StarsFor));

        var firstOfWorldTwo = Catalog.Worlds[1].Stages[0];
        Assert.False(Catalog.IsUnlocked(firstOfWorldTwo, StarsFor));
        stars[Catalog.Worlds[0].Stages[^1].Id] = 2;
        Assert.True(Catalog.IsUnlocked(firstOfWorldTwo, StarsFor));
    }

    [Fact]
    public void NextAfter_walks_the_whole_catalog()
    {
        Assert.Equal(Catalog.Worlds[1].Stages[0], Catalog.NextAfter(Catalog.Worlds[0].Stages[^1]));
        Assert.Null(Catalog.NextAfter(Catalog.Stages[^1]));
    }

    private static StageCatalog Load(string json) => StageCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(json)));
}

public class StageRunTests
{
    private static readonly Stage Stage = new("s", "Stage", ["guess-operator", "prime-or-not", "missing-number"], Difficulty.Easy, 5);

    private static StageRun NewRun() => new(Stage, SkillRegistry.Default, new Random(3));

    [Fact]
    public void Perfect_run_gets_three_stars()
    {
        var run = NewRun();
        do
        {
            Assert.True(run.Submit(TestHelpers.RightGuess(run.Current)));
        }
        while (run.Advance());

        Assert.True(run.IsFinished);
        Assert.Equal(new StageResult(5, 5, 3), run.Result);
        Assert.Equal(5, run.BestStreak);
    }

    [Fact]
    public void Wrong_answers_count_against_the_score()
    {
        var run = NewRun();
        run.Submit(TestHelpers.RightGuess(run.Current));
        run.Advance();
        run.Submit(TestHelpers.WrongGuess(run.Current));

        Assert.Equal(1, run.Correct);
        Assert.Equal(1, run.Mistakes);
        Assert.Equal(0, run.Streak);
        Assert.Equal(1, run.BestStreak);
        Assert.Equal(2, run.Number);
    }

    [Fact]
    public void A_question_can_only_be_answered_once()
    {
        var run = NewRun();
        run.Submit(TestHelpers.WrongGuess(run.Current));

        Assert.Throws<InvalidOperationException>(() => run.Submit(TestHelpers.RightGuess(run.Current)));
    }

    [Fact]
    public void Cannot_skip_a_question_or_read_the_result_early()
    {
        var run = NewRun();

        Assert.Throws<InvalidOperationException>(() => run.Advance());
        Assert.Throws<InvalidOperationException>(() => run.Result);
    }

    [Fact]
    public void Uses_the_stage_skills()
    {
        var seen = Enumerable.Range(0, 40)
            .Select(seed => new StageRun(Stage, SkillRegistry.Default, new Random(seed)).Current.SkillId)
            .Distinct();

        Assert.Equal(Stage.Skills.Order(), seen.Order());
    }

    [Fact]
    public void Questions_do_not_repeat_within_a_stage()
    {
        // Easy prime-or-not draws from only 2–30, which repeated questions before this was fixed.
        var stage = new Stage("p", "Primes", ["prime-or-not"], Difficulty.Easy, 10);

        for (var seed = 0; seed < 50; seed++)
        {
            var run = new StageRun(stage, SkillRegistry.Default, new Random(seed));
            var prompts = new List<string>();
            do
            {
                prompts.Add(run.Current.Prompt);
                run.Submit(TestHelpers.RightGuess(run.Current));
            }
            while (run.Advance());

            Assert.Equal(prompts.Count, prompts.Distinct().Count());
        }
    }

    [Theory]
    [InlineData(10, 10, 3)]
    [InlineData(19, 20, 3)]
    [InlineData(9, 10, 2)]
    [InlineData(8, 10, 2)]
    [InlineData(7, 10, 1)]
    [InlineData(6, 10, 1)]
    [InlineData(5, 10, 0)]
    [InlineData(0, 0, 0)]
    public void Star_rating_thresholds(int correct, int total, int stars) =>
        Assert.Equal(stars, StarRating.For(correct, total));
}

public sealed class ProgressStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "MathGameTests", Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_dir, "progress.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void Starts_empty_when_there_is_no_file()
    {
        var store = new ProgressStore(FilePath);

        Assert.Equal(0, store.StarsFor("anything"));
        Assert.Equal(0, store.TotalStars);
    }

    [Fact]
    public void Keeps_the_best_result_and_survives_a_reload()
    {
        var store = new ProgressStore(FilePath);

        Assert.True(store.Record("a", new StageResult(7, 10, 1)));
        Assert.True(store.Record("a", new StageResult(10, 10, 3)));
        Assert.False(store.Record("a", new StageResult(8, 10, 2)));
        store.Record("b", new StageResult(8, 10, 2));

        var reloaded = new ProgressStore(FilePath);
        Assert.Equal(3, reloaded.StarsFor("a"));
        Assert.Equal(2, reloaded.StarsFor("b"));
        Assert.Equal(5, reloaded.TotalStars);
    }

    [Fact]
    public void Failed_stage_is_not_recorded()
    {
        var store = new ProgressStore(FilePath);

        Assert.False(store.Record("a", new StageResult(2, 10, 0)));
        Assert.False(File.Exists(FilePath));
    }

    [Fact]
    public void Corrupt_file_is_treated_as_empty()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "{ not json");

        var store = new ProgressStore(FilePath);
        Assert.Equal(0, store.TotalStars);

        store.Record("a", new StageResult(10, 10, 3));
        Assert.Equal(3, new ProgressStore(FilePath).StarsFor("a"));
    }
}
