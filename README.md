# AI Toolbar for Unity

A lightweight Unity Editor extension that adds an AI tool button to the main toolbar, letting you quickly launch **Claude Code** or **GitHub Copilot** CLI directly in your project directory.

## Features

- Toolbar button next to the Play controls for one-click access
- Switch between **Claude Code** and **GitHub Copilot** from Unity preferences
- Procedurally generated icons for each tool (no asset dependencies)
- Cross-platform support: Windows, macOS, and Linux
- Also accessible via menu: **Tools > Open AI Terminal**

## Requirements

- Unity 2021.3 or later
- [Claude CLI](https://docs.anthropic.com/en/docs/claude-code) and/or [GitHub Copilot CLI](https://docs.github.com/en/copilot/github-copilot-in-the-cli) installed on your system

## Installation

### Option 1: Unity Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter:
   ```
   https://github.com/AdrianBotana/unity-ia-toolbar.git
   ```

### Option 2: Manual

1. Clone or download this repository
2. Copy the folder into your project's `Packages/` directory

## Configuration

Go to **Edit > Preferences > AI Tool** to select which AI tool to use:

| Tool | Command |
|------|---------|
| Claude | `claude --dangerously-skip-permissions` |
| Copilot | `copilot` |

The selection persists between Unity sessions.

## How It Works

The package injects a button into Unity's main toolbar using reflection. When clicked, it opens a terminal window in your project's root directory and launches the selected AI CLI tool.

## License

MIT
