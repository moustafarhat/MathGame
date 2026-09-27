# Contributing to Math Game

Thanks for your interest! Bug reports, puzzle ideas and pull requests are all welcome. The easiest way to help is to **add a new kind of question**. It's one small C# class, and the tests check most of it for you.

## Reporting bugs and ideas

Use the [issue forms](https://github.com/moustafarhat/MathGame/issues/new/choose). For a wrong answer or a confusing question, a screenshot helps most. Report security problems through [private reporting](SECURITY.md), not public issues.

## Getting set up

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```sh
git clone https://github.com/moustafarhat/MathGame.git
cd MathGame
dotnet run --project MathGame/MathGame.csproj   # play
dotnet test                                     # run the tests
```

## Where things live

| Folder | What's in it |
|---|---|
| `MathGame.Core/Skills/` | One class per kind of question (`ISkill`) |
| `MathGame.Core/Stages/stages.json` | Worlds and stages: which skills, which difficulty, how many questions |
| `MathGame.Core/Play/` | Playing a stage, stars, saved progress |
| `MathGame/` | The Avalonia app (view models and views) |
| `MathGame.Tests/` | xunit tests for everything in `MathGame.Core` |

`MathGame.Core` has no UI code. Keep game rules there, so they can be tested and the UI stays thin.

## Adding a new kind of question

1. **Write the skill.** Create a class in `MathGame.Core/Skills/` that implements `ISkill`. `MissingNumberSkill.cs` is a short example to copy.
   - `Id`: a stable kebab-case id, e.g. `"compare-fractions"`.
   - `Generate(random, difficulty)` returns one of:
     - `ChoiceChallenge`: 2–4 labelled choices. Mark every correct one, and make sure at least one choice is wrong.
     - `NumberChallenge`: a whole-number answer.
   - Always write an `Explanation`. Players see it after a wrong answer, so it should teach the idea, not just give the answer.
   - Use only the `random` you're given, so the same seed always produces the same questions.
2. **Register it** in `SkillRegistry.Default`.
3. **Use it** in a stage in `stages.json`, or add a new world.
4. **Test it.** `SkillTests` already checks every registered skill at every difficulty: the right answer is accepted, the choices are unique, and the same seed gives the same questions. Add one test that checks the math itself against an independent calculation. `TestHelpers.Evaluate` works for arithmetic.

## Pull requests

1. Fork the repo, then create a branch from `master`.
2. Keep each PR focused on one change.
3. Make sure `dotnet test` passes. CI runs it on Windows, Linux and macOS, and the Release workflow checks that every platform still packages.
4. If you change the UI, add a screenshot to the PR.

Maintainers handle releases, so you don't need to change the version number.

## Good first contributions

Look for issues labelled [`good first issue`](https://github.com/moustafarhat/MathGame/labels/good%20first%20issue), or pick an item from the [Roadmap](README.md#roadmap). New skills and translations are especially welcome.

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE) and that you'll follow the [Code of Conduct](CODE_OF_CONDUCT.md).
