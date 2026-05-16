<div align="center">
  <h1>Project Hub</h1>
  <p><strong>All your git repos, one dark-purple dashboard — open VS Code, Folder, or GitHub in one click.</strong></p>

  [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
  [![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
  [![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)](#)
</div>

## Features

- **Auto-discovery** — recursively scans your configured root directory and finds every git repo (up to 3 levels deep), stopping at nested repos so nothing is double-counted
- **Last commit at a glance** — each card shows the most recent commit subject so you know exactly where you left off
- **One-click launchers** — open the project in VS Code, Windows Explorer, or directly on GitHub without leaving the app
- **Manual add / remove** — add any repo outside your scan root via a folder picker; remove repos you don't want shown (persisted across restarts)
- **Live search** — filter projects by name instantly as you type
- **Persistent settings** — excluded and manually-added repos are saved to `%AppData%\ProjectHub\settings.json`

## Prerequisites

- Windows 10 or 11
- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9) (or SDK if building from source)
- [Git](https://git-scm.com/) installed and available on your `PATH`
- [VS Code](https://code.visualstudio.com/) (optional — Folder button is the fallback if `code` isn't found)

## Installation

**From source:**
```bash
git clone https://github.com/proudpurple/ProjectHub.git
cd ProjectHub
dotnet run
```

**Build a release binary:**
```bash
dotnet publish -c Release -r win-x64 --self-contained
```
The output lands in `bin\Release\net9.0-windows\win-x64\publish\`.

## Quick Start

1. Open `MainWindow.xaml.cs` and update the `CodingRoot` constant to point at your projects directory:
   ```csharp
   private const string CodingRoot = @"C:\Users\YourName\Projects";
   ```
2. Run the app (`dotnet run` or launch the published binary).
3. Project Hub scans your root folder and displays a card for every git repo it finds.

## Usage

| Action | How |
|---|---|
| Open in VS Code | Click **Code** on any project card |
| Open in Explorer | Click **Folder** |
| Open on GitHub | Click **GitHub** (only shown when a remote URL is detected) |
| Add a repo outside your root | Click **+ Add** in the header and pick a folder |
| Hide a project | Click the **✕** button on its card |
| Search | Type in the search box — filters by project name live |
| Refresh all projects | Click the **⟳** button |

Removed projects are saved to `settings.json` and stay hidden on next launch. To restore one, delete its entry from `ExcludedPaths` in the settings file.

## Configuration

The scan root is a compile-time constant in `MainWindow.xaml.cs`:

```csharp
private const string CodingRoot = @"C:\Users\YourName\Projects";
```

Settings are stored at:
```
%AppData%\ProjectHub\settings.json
```

```json
{
  "ManualRepos": [
    "C:\\Users\\YourName\\OtherProjects\\MyRepo"
  ],
  "ExcludedPaths": [
    "C:\\Users\\YourName\\Projects\\archived-thing"
  ]
}
```

| Field | Description |
|---|---|
| `ManualRepos` | Repos added via **+ Add** that live outside the scan root |
| `ExcludedPaths` | Repos hidden via the **✕** button |

## Contributing

Pull requests are welcome. For major changes, please open an issue first.

```bash
git clone https://github.com/proudpurple/ProjectHub.git
cd ProjectHub
dotnet build
```

## License

[MIT](LICENSE) © 2025 Anderson Kimball
