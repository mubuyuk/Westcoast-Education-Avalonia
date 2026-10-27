# Westcoast Education – Claude Code Instructions

## Start of every session

Read `PROJECT_CONTEXT.md` first. It describes the current architecture, decisions, test results, status and Git state. Keep it updated when the project changes.

## Project context

This is an educational C#/.NET assignment to build a cross-platform desktop application using Avalonia UI.

The goal is to fulfill all requirements for grade VG.

## Core principles

- Keep the implementation simple, readable and beginner-friendly.
- Avoid overengineering and unnecessary abstractions.
- Implement only the features required by the assignment.
- Prefer straightforward C# over complex architectural patterns.
- Use the minimum number of projects and dependencies necessary.
- Follow the MVVM pattern.
- Use Dependency Injection as required for VG.
- Separate RestDB.io HTTP communication into interfaces and implementations.
- Use dynamic resources for application styling.
- Never hardcode API keys or secrets.

## Existing console project

The previous console application is located at:

`C:\Users\murat\source\repos\WestcoastEducation\WestcoastEducation`

This project is strictly read-only.

Reuse suitable models or logic only when doing so simplifies the new application.

## Development workflow

- Analyze requirements before implementing.
- Present architectural decisions before major changes.
- Do not add features outside the assignment.
- Build and verify changes.
- Report what has actually been tested.
- Explain technical decisions in Swedish, using relevant English programming terms.

## Scope

This is a school assignment, not an enterprise application.

The objective is the simplest maintainable solution that satisfies the requirements, not the most sophisticated architecture.