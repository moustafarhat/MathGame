using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MathGame.ViewModels;

namespace MathGame.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Tunnel so shortcut keys reach the game before a focused button or text box handles them.
        AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private PlayViewModel? Play => (DataContext as MainWindowViewModel)?.Current as PlayViewModel;

    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Text is { } text && Play?.HandleText(text) == true)
        {
            e.Handled = true;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (Play is not { } play)
        {
            return;
        }

        e.Handled = e.Key switch
        {
            Key.Enter => play.HandleEnter(),
            Key.Escape => Quit(play),
            _ => false,
        };
    }

    private static bool Quit(PlayViewModel play)
    {
        play.QuitCommand.Execute(null);
        return true;
    }
}
