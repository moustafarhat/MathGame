# Math Game

A desktop math game for Windows, Linux and macOS. It is split into worlds of short stages. Each stage asks 8 to 12 questions, and your score earns up to three stars. A star unlocks the next stage, and every world ends with a boss stage that mixes all of its skills at a harder level.

## Worlds

| World | Stages |
|---|---|
| **1. Arithmetic** | Find the operator (`12 ? 5 = 60`), missing numbers (`12 + ? = 20`), bigger numbers, order of operations (`3 ? 4 ? 2 = 11`), boss |
| **2. Numbers** | Prime or not, prime factors (`60 = 2 × 2 × 3 × 5`), below zero (`−7 + (−3) = ?`), harder primes, boss |

## How to play

- **Multiple choice:** click an answer, or press `1` to `4`. Some questions have their own shortcut keys:
  - Operators: `P` or `+`, `M` or `-`, `X` or `*`, `D` or `/`
  - Prime or not: `Y` or `N`
- **Number answers:** type the answer and press `Enter`. Both `-` and `−` work as the minus sign.
- If more than one answer is right (for example `2 ? 2 = 4`), any of them counts.
- After a right answer the game moves on by itself. After a wrong one, it shows the correct answer with an explanation, and you press `Enter` to continue.
- `Esc` takes you back to the map.

### Stars

| Correct | Stars |
|---|---|
| 95% or more | ★★★ |
| 80% or more | ★★☆ |
| 60% or more | ★☆☆ |
| Below 60% | Not passed; the next stage stays locked |

Your best result for each stage is saved in `progress.json` in the per-user application data folder:

| OS | Location |
|---|---|
| Windows | `%LOCALAPPDATA%\MathGame\progress.json` |
| Linux | `~/.local/share/MathGame/progress.json` |
| macOS | `~/Library/Application Support/MathGame/progress.json` |

## Build, test and run

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```sh
dotnet build MathGame.sln
dotnet test MathGame.sln
dotnet run --project MathGame/MathGame.csproj
```

To build a self-contained copy for another platform:

```sh
dotnet publish MathGame/MathGame.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

Other runtime ids: `win-x64`, `osx-arm64` (Apple Silicon), `osx-x64` (Intel Mac).

On macOS the published app is a plain executable, not a signed `.app` bundle. Run it from Terminal. If Gatekeeper blocks it, clear the quarantine flag first with `xattr -d com.apple.quarantine MathGame`.

## Project layout

| Project | What it contains |
|---|---|
| `MathGame.Core` | Game logic with no UI: skills, challenges, stages, scoring and saved progress |
| `MathGame` | The [Avalonia](https://avaloniaui.net/) desktop app: MVVM view models and views over `MathGame.Core` |
| `MathGame.Tests` | xunit tests for `MathGame.Core` |

### How the game is put together

- A **skill** (`ISkill`) generates **challenges** at a given difficulty (easy, medium or hard). There are two kinds of challenge:
  - `ChoiceChallenge`: pick from a list; more than one choice can be correct.
  - `NumberChallenge`: type a whole number.
- **Stages** are listed in [`MathGame.Core/Stages/stages.json`](MathGame.Core/Stages/stages.json). Each stage names its skills, its difficulty and how many questions it has.
- `StageRun` plays through one stage, and `ProgressStore` saves the best star rating for each stage.

### Adding content

- **New stage:** add an entry to `stages.json`. No code changes are needed.
- **New kind of question:**
  1. Write a class that implements `ISkill`.
  2. Register it in `SkillRegistry.Default`.
  3. Use its id in `stages.json`.
  4. Add it to the tests. `SkillTests` already checks that every registered skill produces well-formed challenges at every difficulty.

CI in `.github/workflows/dotnet-desktop.yml` builds and tests on Windows, Linux and macOS. It then publishes self-contained builds for all four runtime ids as build artifacts.
