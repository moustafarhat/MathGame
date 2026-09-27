using CommunityToolkit.Mvvm.ComponentModel;
using MathGame.Core.Play;
using MathGame.Core.Skills;
using MathGame.Core.Stages;

namespace MathGame.ViewModels;

public abstract class ViewModelBase : ObservableObject;

/// <summary>Owns the game services and switches between the map, play and result screens.</summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _current;

    public MainWindowViewModel(StageCatalog catalog, SkillRegistry skills, ProgressStore progress)
    {
        Catalog = catalog;
        Skills = skills;
        Progress = progress;
        _current = new MapViewModel(this);
    }

    public StageCatalog Catalog { get; }

    public SkillRegistry Skills { get; }

    public ProgressStore Progress { get; }

    public void ShowMap() => Current = new MapViewModel(this);

    public void Play(Stage stage) => Current = new PlayViewModel(this, new StageRun(stage, Skills));

    public void Finish(StageRun run)
    {
        var previousBest = Progress.StarsFor(run.Stage.Id);
        var improved = Progress.Record(run.Stage.Id, run.Result);
        Current = new ResultViewModel(this, run, improved && previousBest > 0);
    }

    public bool IsUnlocked(Stage stage) => Catalog.IsUnlocked(stage, Progress.StarsFor);

    public static string StarsText(int stars) => new string('★', stars) + new string('☆', 3 - stars);
}
