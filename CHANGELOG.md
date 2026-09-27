# Changelog

All notable changes are listed here. Versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [3.0.0] - 2026-09-27

The first public release. The game was rebuilt from scratch as a cross-platform game.

### Added

- **Worlds, stages and stars.** Two worlds with five stages each, ending in a boss stage. Each stage earns up to three stars, and one star unlocks the next stage.
- **World 1, Arithmetic:** find the operator, missing numbers, order of operations.
- **World 2, Numbers:** prime or not, prime factors, negative numbers.
- Explanations after every wrong answer: what your pick works out to, the right answer, and the key step (for example "× first").
- Keyboard play: `1`–`4`, `P`/`M`/`X`/`D`, `Y`/`N`, `Enter`, `Esc`.
- Progress saved per user.
- Self-contained downloads for Windows, Linux and macOS (Apple Silicon and Intel).

### Fixed

- Answers that were mathematically right were marked wrong. For example, `2 ? 2 = 4` accepted only one of `+` and `×`.
- Division showed truncated results such as `39 ÷ 5 = 7`. Division questions now always divide exactly.

### Changed

- Moved from .NET Framework 4.6.1 and WinForms (Windows only) to .NET 10 and Avalonia 12.

## [1.0.0] - 2018-11-01

The original Windows Forms game: guess the hidden operator in `a ? b = c`.

[Unreleased]: https://github.com/moustafarhat/MathGame/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/moustafarhat/MathGame/releases/tag/v3.0.0
