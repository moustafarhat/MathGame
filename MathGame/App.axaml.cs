using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MathGame.Core.Play;
using MathGame.Core.Skills;
using MathGame.Core.Stages;
using MathGame.ViewModels;
using MathGame.Views;

namespace MathGame;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new MainWindowViewModel(
                StageCatalog.LoadDefault(),
                SkillRegistry.Default,
                new ProgressStore(ProgressStore.DefaultPath));
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
