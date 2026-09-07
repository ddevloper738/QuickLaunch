# QuickLaunch

> **For AI coding assistants:** See `.agentinstructions.md` at the project root for detailed project context, architecture guidance, and implementation instructions.

**QuickLaunch** is a calm, keyboard-first launcher for Windows, built with **WPF (.NET Framework 4.7.2)**.

QuickLaunch sits quietly in the system tray, waiting for a customizable hotkey. When invoked, it brings up a sleek overlay that lets you instantly search for and launch applications, files, folders, and commands — all from your keyboard.

## Features

- **Global hotkey** — Press `Ctrl + Space` from anywhere to show or hide the launcher.
- **Fast search** — Search built-in actions and Windows Start Menu shortcuts with keyboard navigation.
- **Safe launching** — Open apps, folders, and Windows settings with clear in-app failure feedback.
- **System tray** — Keep QuickLaunch available without taking space on the taskbar.
- **Minimal UI** — A focused dark overlay with sensible spacing, contrast, and visible keyboard hints.

## Getting Started

### Prerequisites

- Windows 7 or later
- [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended for development)

### Build & Run

```bash
git clone https://github.com/ddevloper738/QuickLaunch.git
cd QuickLaunch
# Open QuickLaunch.slnx in Visual Studio and press F5
```

Or build from the command line:

```bash
msbuild QuickLaunch.slnx /p:Configuration=Debug
```

## Project Structure

```
QuickLaunch/
├── QuickLaunch/           # WPF application source
│   ├── App.xaml(.cs)      # Application entry point
│   ├── MainWindow.xaml(.cs) # Main window UI and logic
│   └── Properties/        # Assembly info, resources, settings
├── QuickLaunch.slnx       # Solution file (new .slnx format)
└── README.md              # This file
```

## License

MIT
