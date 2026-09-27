# Security Policy

## Supported versions

Only the [latest release](https://github.com/moustafarhat/MathGame/releases/latest) receives fixes.

## Reporting a vulnerability

Please **don't** open a public issue. Report it privately through [GitHub security advisories](https://github.com/moustafarhat/MathGame/security/advisories/new).

Include what you found, how to reproduce it, and its impact. You'll get a reply within a few days, and credit in the release notes if you'd like.

## Scope notes

Math Game runs fully offline. It makes no network requests, collects no telemetry, and writes only one file: `progress.json` in the per-user app-data folder. The areas most worth checking are the parsing of that file and of `stages.json`, and the integrity of the release packages.
