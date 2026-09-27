using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MathGame.ViewModels;

namespace MathGame.Views;

public partial class PlayView : UserControl
{
    private PlayViewModel? _viewModel;

    public PlayView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel is not null)
        {
            _viewModel.AnswerFocusRequested -= OnAnswerFocusRequested;
        }

        _viewModel = DataContext as PlayViewModel;
        if (_viewModel is not null)
        {
            _viewModel.AnswerFocusRequested += OnAnswerFocusRequested;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        FocusForCurrentChallenge();
    }

    private void OnAnswerFocusRequested(object? sender, EventArgs e) => FocusForCurrentChallenge();

    // Posted so the answer box is visible and enabled again before it takes focus.
    private void FocusForCurrentChallenge() => Dispatcher.UIThread.Post(() =>
    {
        if (_viewModel?.IsNumber == true)
        {
            AnswerBox.Focus();
        }
        else
        {
            Focus();
        }
    });
}
