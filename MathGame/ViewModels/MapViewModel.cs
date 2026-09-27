using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathGame.Core.Stages;

namespace MathGame.ViewModels;

/// <summary>The world map: every stage with its stars, locked until the one before it is passed.</summary>
public partial class MapViewModel : ViewModelBase
{
    public MapViewModel(MainWindowViewModel main)
    {
        Worlds = main.Catalog.Worlds
            .Select((world, i) => new WorldItem(
                $"World {i + 1}: {world.Title}",
                world.Description,
                world.Stages.Select((stage, j) => new StageItem(main, stage, stage.Boss ? "Boss" : $"{i + 1}-{j + 1}")).ToList()))
            .ToList();

        var maxStars = main.Catalog.Stages.Count * 3;
        StarsSummary = $"★ {main.Progress.TotalStars} / {maxStars}";
    }

    public IReadOnlyList<WorldItem> Worlds { get; }

    public string StarsSummary { get; }
}

public sealed record WorldItem(string Title, string Description, IReadOnlyList<StageItem> Stages);

public partial class StageItem : ObservableObject
{
    private readonly MainWindowViewModel _main;
    private readonly Stage _stage;

    public StageItem(MainWindowViewModel main, Stage stage, string number)
    {
        _main = main;
        _stage = stage;
        Number = number;
        IsUnlocked = main.IsUnlocked(stage);
        Stars = MainWindowViewModel.StarsText(main.Progress.StarsFor(stage.Id));
    }

    public string Number { get; }

    public string Title => _stage.Title;

    public bool IsBoss => _stage.Boss;

    public bool IsUnlocked { get; }

    public string Stars { get; }

    [RelayCommand(CanExecute = nameof(IsUnlocked))]
    private void Play() => _main.Play(_stage);
}
