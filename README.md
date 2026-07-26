# QuickLaunch 🚀

> **For AI coding assistants:** See `.agentinstructions.md` at the project root for detailed project context, architecture guidance, and implementation instructions.

**QuickLaunch** is a global keyboard launcher for Windows — built with **WPF (.NET Framework 4.7.2)**.

QuickLaunch sits quietly in the system tray, waiting for a customizable hotkey. When invoked, it brings up a sleek overlay that lets you instantly search for and launch applications, files, folders, and commands — all from your keyboard.

## Features (Planned)

- ⚡ **Global Hotkey** — Summon the launcher from anywhere with a single keystroke
- 🔍 **Fuzzy Search** — Find what you need fast with smart, typo-tolerant matching
- 📂 **App & File Launching** — Launch installed programs, documents, folders, and URLs
- 🧩 **Plugin System** — Extend QuickLaunch with custom commands and providers
- 🎨 **Clean WPF UI** — Modern, lightweight overlay that blends into any workflow
- ⚙️ **Customizable** — Configure hotkeys, themes, search sources, and more

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
