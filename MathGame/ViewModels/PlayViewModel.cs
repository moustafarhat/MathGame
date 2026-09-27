using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathGame.Core.Challenges;
using MathGame.Core.Play;

namespace MathGame.ViewModels;

/// <summary>Plays one stage: shows each challenge, takes the answer, and explains it.</summary>
public partial class PlayViewModel : ViewModelBase
{
    /// <summary>How long a correct answer stays on screen before moving on by itself.</summary>
    private static readonly TimeSpan CorrectPause = TimeSpan.FromMilliseconds(900);

    private readonly MainWindowViewModel _main;
    private readonly StageRun _run;

    // Bumped per question so a pending auto-advance can't skip the next one.
    private int _questionVersion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PromptFontSize))]
    private string _prompt = "";

    [ObservableProperty]
    private string _progressText = "";

    [ObservableProperty]
    private string _streakText = "";

    [ObservableProperty]
    private bool _isChoice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KeyHint))]
    private bool _isNumber;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitNumberCommand))]
    private string _answerText = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitNumberCommand))]
    [NotifyPropertyChangedFor(nameof(ShowContinue))]
    private bool _isAnswered;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowContinue))]
    private bool _wasCorrect;

    [ObservableProperty]
    private string _feedback = "";

    [ObservableProperty]
    private string _explanation = "";

    public PlayViewModel(MainWindowViewModel main, StageRun run)
    {
        _main = main;
        _run = run;
        StageTitle = run.Stage.Boss ? $"Boss: {run.Stage.Title}" : run.Stage.Title;
        ShowChallenge();
    }

    /// <summary>Raised when a number challenge appears, so the view can focus the answer box.</summary>
    public event EventHandler? AnswerFocusRequested;

    public string StageTitle { get; }

    public ObservableCollection<ChoiceItem> Choices { get; } = [];

    /// <summary>Wrong answers wait for the player to read the explanation; right ones move on by themselves.</summary>
    public bool ShowContinue => IsAnswered && !WasCorrect;

    /// <summary>Equations read best large; worded questions need to fit on a line or two.</summary>
    public double PromptFontSize => Prompt.Length > 20 ? 28 : 40;

    public string KeyHint => IsNumber
        ? "Type the answer and press Enter · Esc goes back to the map"
        : "Keys: 1–4 pick an answer · Enter continues · Esc goes back to the map";

    /// <summary>Handles a typed character. Returns true if it answered a choice challenge.</summary>
    public bool HandleText(string text)
    {
        if (IsAnswered || text.Length != 1 || _run.Current is not ChoiceChallenge challenge)
        {
            return false;
        }

        if (challenge.ChoiceForKey(text[0]) is not { } index)
        {
            return false;
        }

        Choose(index);
        return true;
    }

    /// <summary>Enter submits a typed number, or moves on after an answer.</summary>
    public bool HandleEnter()
    {
        if (IsAnswered)
        {
            Continue();
            return true;
        }

        if (IsNumber && SubmitNumberCommand.CanExecute(null))
        {
            SubmitNumber();
            return true;
        }

        return false;
    }

    public void Choose(int index)
    {
        if (IsAnswered)
        {
            return;
        }

        var correct = _run.Submit(new ChoiceGuess(index));
        for (var i = 0; i < Choices.Count; i++)
        {
            // Reveal every right answer, and mark the pick if it was wrong.
            Choices[i].IsMarkedCorrect = Choices[i].IsCorrect;
            Choices[i].IsMarkedWrong = i == index && !correct;
        }

        ShowFeedback(correct);
    }

    private bool CanSubmitNumber() => !IsAnswered && NumberChallenge.TryParseAnswer(AnswerText, out _);

    [RelayCommand(CanExecute = nameof(CanSubmitNumber))]
    private void SubmitNumber()
    {
        NumberChallenge.TryParseAnswer(AnswerText, out var value);
        ShowFeedback(_run.Submit(new NumberGuess(value)));
    }

    [RelayCommand]
    private void Continue()
    {
        if (!IsAnswered)
        {
            return;
        }

        if (_run.Advance())
        {
            ShowChallenge();
        }
        else
        {
            _main.Finish(_run);
        }
    }

    [RelayCommand]
    private void Quit() => _main.ShowMap();

    private async void ShowFeedback(bool correct)
    {
        IsAnswered = true;
        WasCorrect = correct;
        Feedback = correct ? "Correct!" : "Not quite";
        Explanation = _run.Current.Explanation;
        UpdateStatus();

        if (correct)
        {
            var version = _questionVersion;
            await Task.Delay(CorrectPause);
            if (version == _questionVersion && _main.Current == this)
            {
                Continue();
            }
        }
    }

    private void ShowChallenge()
    {
        _questionVersion++;
        var challenge = _run.Current;

        Prompt = challenge.Prompt;
        IsAnswered = false;
        WasCorrect = false;
        Feedback = "";
        Explanation = "";
        AnswerText = "";

        Choices.Clear();
        if (challenge is ChoiceChallenge choice)
        {
            for (var i = 0; i < choice.Choices.Count; i++)
            {
                Choices.Add(new ChoiceItem(this, i, choice.Choices[i]));
            }
        }

        IsChoice = challenge is ChoiceChallenge;
        IsNumber = challenge is NumberChallenge;
        UpdateStatus();

        if (IsNumber)
        {
            AnswerFocusRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateStatus()
    {
        ProgressText = $"Question {_run.Number} of {_run.Total}";
        StreakText = _run.Streak > 1 ? $"Streak {_run.Streak}" : "";
    }
}

public partial class ChoiceItem : ObservableObject
{
    private readonly PlayViewModel _owner;
    private readonly int _index;

    [ObservableProperty]
    private bool _isMarkedCorrect;

    [ObservableProperty]
    private bool _isMarkedWrong;

    public ChoiceItem(PlayViewModel owner, int index, Choice choice)
    {
        _owner = owner;
        _index = index;
        Label = choice.Label;
        IsCorrect = choice.IsCorrect;
        Shortcut = (index + 1).ToString();
    }

    public string Label { get; }

    public string Shortcut { get; }

    public bool IsCorrect { get; }

    [RelayCommand]
    private void Select() => _owner.Choose(_index);
}
