# AI Toolbar for Unity

A lightweight Unity Editor extension that adds a button to the main toolbar for launching **Claude Code** or **GitHub Copilot** CLI directly in your project directory.

## Features

- Toolbar button next to the Play controls for one-click terminal launch
- Switch between **Claude Code** and **GitHub Copilot** from Unity preferences
- Optional **Skip Permissions** toggle (`--dangerously-skip-permissions` for Claude, `--yolo` for Copilot)
- Procedurally generated icons — no external asset dependencies
- Cross-platform: Windows, macOS, and Linux
- Also accessible via menu: **Tools > Open AI Terminal**

## Requirements

- Unity 2021.3 or later
- [Claude Code](https://docs.anthropic.com/en/docs/claude-code) and/or [GitHub Copilot CLI](https://docs.github.com/en/copilot/github-copilot-in-the-cli) installed on your system

## Installation

### Unity Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter:
   ```
   https://github.com/AdrianBotana/unity-ia-toolbar.git
   ```

### Manual

1. Clone or download this repository
2. Copy the folder into your project's `Packages/` directory

## Configuration

Go to **Edit > Preferences > AI Tool** to configure:

- **Active AI Tool** — choose between Claude and Copilot
- **Skip Permissions** — when enabled, launches the CLI in a less restrictive mode

| Tool | Normal | Skip Permissions |
|------|--------|------------------|
| Claude | `claude` | `claude --dangerously-skip-permissions` |
| Copilot | `copilot` | `copilot --yolo` |

Both settings persist between Unity sessions via `EditorPrefs`.

## How It Works

The package injects a button into Unity's main toolbar using reflection. When clicked, it opens a terminal window (`cmd.exe` on Windows, `/bin/bash` on macOS/Linux) in your project's root directory and runs the selected AI CLI tool.

## License

MIT
