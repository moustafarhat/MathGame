using CommunityToolkit.Mvvm.Input;
using MathGame.Core.Play;
using MathGame.Core.Stages;

namespace MathGame.ViewModels;

public partial class ResultViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly Stage _stage;
    private readonly Stage? _next;

    public ResultViewModel(MainWindowViewModel main, StageRun run, bool newBest)
    {
        _main = main;
        _stage = run.Stage;
        _next = main.Catalog.NextAfter(run.Stage);

        var result = run.Result;
        StageTitle = run.Stage.Title;
        Stars = MainWindowViewModel.StarsText(result.Stars);
        Score = $"{result.Correct} of {result.Total} correct · best streak {run.BestStreak}";
        IsPassed = result.Stars > 0;
        Headline = result.Stars switch
        {
            3 => "Perfect!",
            2 => "Great job!",
            1 => "Stage passed",
            _ => "Not passed yet",
        };
        Detail = IsPassed
            ? (newBest ? "New best score!" : "")
            : "Get at least 60% right to earn a star and unlock the next stage.";
    }

    public string StageTitle { get; }

    public string Headline { get; }

    public string Stars { get; }

    public string Score { get; }

    public string Detail { get; }

    public bool IsPassed { get; }

    public bool HasNext => _next is not null && _main.IsUnlocked(_next);

    [RelayCommand]
    private void Retry() => _main.Play(_stage);

    [RelayCommand(CanExecute = nameof(HasNext))]
    private void Next() => _main.Play(_next!);

    [RelayCommand]
    private void Map() => _main.ShowMap();
}
