# Winforms Vibes

> This project was entirely vibe coded using a local Qwen3.6 LLM running on an NVIDIA RTX 3090 Ti with the latest Llama.cpp and MTP support.

## What is this?

Winforms Vibes is a Windows desktop application that combines an interactive world map with AI-powered chat. Click anywhere on the map to get coordinates, then ask an AI assistant about places near that location. Browse built-in help topics, chat with a general-purpose AI, or use the AI help assistant ("Fella") that knows your application's help content. First launch walks you through connecting to a SQL Server database—after that, the app starts right up.

**In short:** interactive maps + AI chat + built-in help, all in one Windows desktop app.

**At a glance:**
- **Interactive world map** — click to get coordinates, ask the AI about nearby cities
- **AI chat windows** — general chat, map-specific assistant, and a help-aware AI ("Fella")
- **Built-in help browser** — searchable topics grouped by category
- **First-run setup** — simple dialog to connect your SQL Server database
- **One-click install** — run the installer or extract the release folder and launch

## Quick Start for Customers

1. Run the installer (or extract the release folder)
2. Launch `WinformsVibes.exe`
3. On first run, enter your SQL Server details (server, database name, username, password)
4. Click the map, press "Tell Me More!" to ask the AI about nearby places
5. Use **Chat > AI Chat** for general conversation, **Help > AI Help** for application assistance

---

A Windows Forms desktop application built with .NET 10.0, featuring a splash screen, menu bar, status bar, embedded Google Maps via WebView2, and AI chat powered by a local OpenAI-compatible endpoint. Entirely AI-generated.

![App Screenshot](AppScreenshot.png)

## Features

- **Splash Screen** — displays application info (name, version, author, framework, database, server, user) fetched from the database. Animated dancing bear below the user line. Bottom toolbar with a right-aligned Continue button to proceed.
- **Main Form** — menu bar with File, Edit, View, Chat, Settings, and Help menus. Restores to 1280×800 when unmaximized.
- **Title Bar Tooltips** — hovering over minimize, maximize, or close buttons on any window shows a tooltip ("Minimize", "Maximize"/"Restore Down", "Close")
- **World Map** — embedded Google Maps via WebView2 (Chromium-based). Lat/Long coordinates are populated by clicking the map or entering them manually in the coord panel.
- **Live Clock** — status bar clock that updates every second
- **Database Setup** — first-run wizard with inputs for server, database name, username, and password
- **Help Topics** — browsable help content covering project structure, GUI features, and usage
- **AI Chat** — chat with a local LLM via Chat > AI Chat
- **Fella - AI Helper** — context-aware help assistant that uses HelpInfo data to answer questions (Help > AI Help). Title bar shows a question mark icon and displays a red welcome message.
- **AI Map Chat** — singleton chat window opened by the "Tell Me More!" button. When coordinates are set (by clicking the map or entering Lat/Long), the button launches an AI agent that searches for the first city within a 5-mile radius and returns information about it. Exposes `AskAsync(string message)` for programmatic use. Assistant responses are green.
- **Help Sync** — HelpInfo table is automatically synced with HelpTopics.xml on every launch
- **Build Release** — BuildRelease.bat publishes a release to a timestamped folder under Releases/

## Requirements

- .NET 10.0 SDK
- SQL Server (local or remote) with `sa` authentication enabled
- Windows 10 or later (for WebView2 support)

## How to Run

### Quick Start (Development)

```powershell
# Option A: batch file (builds then launches)
.\RunMe.bat

# Option B: dotnet CLI
dotnet build WinformsVibes.csproj -p:Configuration=Debug
dotnet run --project WinformsVibes.csproj
```

> **Git Bash users:** `dotnet run` exits immediately because the GUI detaches from the shell. Use `RunMe.bat` or the compiled `.exe` instead.

### First Launch

If no database is configured (or the configured database is unreachable), a dark-themed setup dialog appears with four fields: Server (defaults to `localhost`), Database (required), Username (defaults to `sa`), Password (masked). Press Enter in any field to submit, Escape to cancel.

After creating a database, the app seeds it with default `ApplicationInfo` and `HelpInfo` data, shows the splash screen, then launches the main form.

### Build a Distributable Release

```powershell
.\BuildRelease.bat
```

Publishes a self-contained release to `Releases/Build-{timestamp}/`.

### Build an Installer

```powershell
.\BuildInstaller.bat
```

Creates a Windows installer package via Inno Setup (`CreateInstaller.iss`).

### Run Tests

```powershell
# All tests
.\RunTests.bat
# or
dotnet test WinformsVibes.Tests.csproj

# Single test
dotnet test WinformsVibes.Tests.csproj --filter "FullyQualifiedName~GetInstance_ReturnsSameInstanceOnRepeatedCalls"

# Database tests (requires SQL Server connection, drops test schema before running)
dotnet test WinformsVibes.Tests.csproj --filter "FullyQualifiedName~DatabaseTests"
```

## Complete File Reference

Every file in this project, what it does, and how it fits together.

### Entry Point

| File | What it does |
|---|---|
| `Program.cs` | Application entry point. Checks database connectivity via `DbConfig.CheckConnection()`. If unreachable, shows `DatabaseSetupDialog`. On failure, copies the error to the clipboard (`Clipboard.SetText()`). On success, syncs HelpInfo with HelpTopics.xml via `DbConfig.SyncHelpTopics()`, runs the splash screen (`Application.Run(splash)`), then launches `MainForm` after the splash closes. |

### Project & Build Files

| File | What it does |
|---|---|
| `WinformsVibes.csproj` | Main project file. Targets `net10.0-windows`, enables Windows Forms and nullable refs. Excludes `Tests/` from compilation. Copies `HelpTopics.xml` and `DancingBear.gif` to output on build. Suppresses MSB3277 (WebView2 WindowsBase version conflict). |
| `WinformsVibes.Tests.csproj` | Test project. References the main project, uses NUnit 4.2.2. Uses `<Compile Remove>` to avoid CS0311/CS0436 conflicts with the main project. |
| `Directory.Build.props` | Sets `obj\$(MSBuildProjectFile)\` per project to avoid MSBuild obj-folder conflicts between main and test projects. |
| `RunMe.bat` | Builds the project in Debug mode then launches the `.exe`. Quick dev launcher. |
| `BuildRelease.bat` | Runs `dotnet publish -c Release -r win-x64 --self-contained` to a timestamped `Releases/Build-{timestamp}/` folder. |
| `BuildInstaller.bat` | Builds a release and runs Inno Setup to create an installer executable. |
| `CreateInstaller.iss` | Inno Setup script defining the installer — app name, output path, files to include, start menu shortcut. |
| `RunTests.bat` | Runs `dotnet test` and pauses on completion so the user can see results. |
| `screenshot.ps1` | PowerShell script for capturing the main form screenshot. |

### GUI Forms (`GUI/`)

All UI forms live here under namespace `WinformsVibes.GUI`.

| File | What it does |
|---|---|
| `MainForm.cs` | Main application window. Extends `TitleBarTooltipMaterialForm` (MaterialForm subclass). `Size` set to 1280×800 before maximizing so restore-down gives a reasonable size. Contains a `CrownMenuStrip` with dark renderer (`DarkMenuRenderer`), `TabControl` with one tab (World Map), `CrownStatusStrip` with a live clock updated by a 1-second timer, and a procedurally drawn bear-face icon via `CreateBearIcon()`. Menu items: File (New/Open/Save/Exit), Edit (Copy/Paste), View (Toggle Fullscreen/About), Settings (Preferences), Chat (AI Chat), Help (Contents/AI Help/About). |
| `SplashScreen.cs` | FixedDialog splash form. Dark theme. Displays application info (name, version, author, framework, database, server, user) from the database. Shows an animated dancing bear (`DancingBear.gif`, 100×100px, `SizeMode.Zoom`) in a PictureBox below the user line. Bottom toolbar with right-aligned "Continue" button. Clicking Continue or X closes the splash and proceeds to the main form. |
| `DatabaseSetupDialog.cs` | Dark-themed first-run dialog. Four inputs: Server (defaults `localhost`), Database (required, no default), Username (defaults `sa`), Password (masked via `UseSystemPasswordChar`). Focus lands on Database field. Exposes `Server`, `DatabaseName`, `UserId`, `Password` properties. Enter submits, Escape cancels. |
| `WorldMapTab.cs` | `UserControl` for the World Map tab. Contains a `WebView2` loaded with Google Maps and a bottom `coordPanel` (60px) with two `MaterialTextBoxEdit` inputs (62px, rounded regions) for Lat/Long and a "Tell Me More!" `MaterialButton`. Enter in a coord field navigates the map. The button opens `AIMapWindow` and asks about the first city within a 5-mile radius of the coordinates. `SourceChanged` event syncs URL coordinates back into the Lat/Long inputs. Button is disabled when both coords are 0. Uses `async void` with `await EnsureCoreWebView2Async()` for WebView2 init. |
| `ChatWindow.cs` | Singleton AI chat window. Yellow smiley face icon (`CreateSmileyIcon()`). Connects to OpenAI-compatible endpoint at `http://192.168.2.15:8888/v1` using `OpenAIChatClient`. API key `"apikey"`, model `"Qwen3.6-27B-MTP-Q4_K_M"`. X hides (doesn't close), preserving chat history. User messages: blue. Assistant: yellow label + green body. System: gray. RTF preserved when removing "Thinking..." placeholder. Chat log: Consolas 15f. Input/send: Segoe UI 16.5f with matching explicit heights. |
| `AIHelpWindow.cs` | Singleton titled "Fella - AI Helper". Question mark icon (`SystemIcons.Question`). Same singleton pattern as ChatWindow (hide on close, preserve history). Same font sizes/layout. Displays red welcome message: "Welcome to Fella! Your helpful AI dude." Loads all HelpInfo topics from database at startup and includes them in the system prompt so the AI can answer based on actual help content. |
| `AIMapWindow.cs` | Singleton chat window titled "AI Map Chat". Same UI pattern as ChatWindow (dark theme, Consolas 15f log, Segoe UI 16.5f input). Exposes `AskAsync(string message)` for programmatic use. Assistant responses: yellow label + green body. Wired to the "Tell Me More!" button in WorldMapTab — clicking asks about the first city within a 5-mile radius of the selected coordinates. |
| `HelpWindow.cs` | Dark-themed help browser. Question mark icon (`SystemIcons.Question`). Groups help topics by unique Category+Topic pairs and displays all content values when selected. Search filters across category, topic name, and all content values. Uses `GroupedHelpTopic` record with a `List<string>` of contents. Also defines the `HelpTopic` record used by `DbConfig.GetHelpTopics()`. |
| `TitleBarTooltipForm.cs` | Base class extending `Form`. Overrides `WndProc` to intercept `WM_MOUSEMOVE` and send `WM_NCHITTEST` to detect cursor over minimize/maximize/close buttons. Shows tooltips ("Minimize", "Maximize"/"Restore Down", "Close"). Hidden when mouse enters client area. Disposes its `ToolTip` in `OnHandleDestroyed`. All standard forms inherit from this. |
| `TitleBarTooltipMaterialForm.cs` | Same tooltip logic as `TitleBarTooltipForm` but extends `MaterialForm` instead of `Form`. Used by MainForm. |

### AI Layer (`AI/`)

| File | What it does |
|---|---|
| `OpenAIChatClient.cs` | HttpClient-based OpenAI `/chat/completions` client. Takes `apiKey`, `model`, optional `baseUrl` in constructor. `ChatAsync` is `virtual` (accepts optional `systemPrompt`, defaults to "You are a helpful assistant.") to allow mocking in tests. Uses `System.Text.Json`, no external NuGet packages. Implements `IDisposable` to clean up HttpClient. |

### Database Layer (`Database/`)

All under namespace `WinformsVibes.Database`.

| File | What it does |
|---|---|
| `DbConfig.cs` | Fluent NHibernate configuration. Connection details loaded via `DbSettingsManager.Load()` from `dbconfig.json`. Key methods: `BuildFluentConfig()` builds the FluentConfiguration with explicit `Add<ApplicationInfoMap>()` and `Add<HelpInfoMap>()` mappings (used by both `SessionFactory` and `CreateAndSeedDatabase`); `CheckConnection()` tests reachability; `CreateAndSeedDatabase(server, name, userId, password, out errorMessage)` creates the database, uses `SchemaExport` from the Fluent NHibernate mappings to create tables (no raw SQL DDL), seeds `ApplicationInfo` with default app data and `HelpInfo` from `HelpTopics.xml`, updates `_settings` and saves to `dbconfig.json`, resets `_sessionFactory`; `SyncHelpTopics()` truncates HelpInfo and re-seeds from XML on every launch. Properties: `CurrentDatabaseName`, `CurrentServer`, `CurrentUserId` for splash screen display. `GetApplicationInfo()`, `GetHelpTopics()` for data access. |
| `DbSettings.cs` | `DbSettings` POCO with Server, DatabaseName, UserId, Password. `DbSettingsManager` reads/writes `dbconfig.json` from `%LOCALAPPDATA%/WinformsVibes/` using `System.Text.Json`. Migrates from old app-directory location on first run. Returns defaults (`localhost`/`winformsvibes`/`sa`/`password`) if file is missing. |

### Entities & Mappings

| File | What it does |
|---|---|
| `Models/ApplicationInfo.cs` | Entity model: Name, Version, Author, Framework, Dependencies. `Dependencies` column mapped as `nvarchar(max)`. |
| `Models/HelpInfo.cs` | Entity model: Category, Topic, Content. `Content` column mapped as `nvarchar(max)`. |
| `Maps/ApplicationInfoMap.cs` | Fluent NHibernate map for ApplicationInfo. `DatabaseName`, `Server`, `UserId` are **not mapped** — set at runtime for display. |
| `Maps/HelpInfoMap.cs` | Fluent NHibernate map for HelpInfo. Proxy validation and lazy loading are disabled. |

### Data & Assets

| File | What it does |
|---|---|
| `HelpTopics.xml` | XML defining all help topics. Copied to output on build. Each `<Topic>` has `Category` and `Name` attributes with text content. Seeded into the `HelpInfo` table on database creation and synced on every launch. **This is the single source of truth for help content.** Edit this file to change what appears in the Help window. |
| `DancingBear.gif` | Animated dancing bear GIF. Copied to output on build. Displayed in the splash screen PictureBox (100×100px, `SizeMode.Zoom`). |
| `dbconfig.json` | Application settings file stored at `%LOCALAPPDATA%/WinformsVibes/`. Contains `Server`, `DatabaseName`, `UserId`, `Password`. Created automatically by the DatabaseSetupDialog on first successful database creation. Deleting this file triggers the setup dialog on next launch. On first run after an update, migrated from the old app-directory location if found. |

### Tests (`Tests/`)

| File | What it does |
|---|---|
| `ChatWindowTests.cs` | Tests for ChatWindow singleton pattern, visibility, icon, and chat flow. Uses `MockChatClient` (subclass of `OpenAIChatClient` that overrides `ChatAsync` to return fake responses) to avoid needing a live endpoint. Uses reflection to access private fields and methods. |
| `DatabaseTests.cs` | Integration tests for database operations. `OneTimeSetUp` drops the test database (`testdb_schema`) before running to ensure a fresh schema. Validates `SchemaExport` table creation and data seeding. Requires a running SQL Server connection. |

### Configuration & Settings

| File | What it does |
|---|---|
| `.claude/settings.json` | Claude Code project settings. |
| `CLAUDE.md` | This project's Claude Code guidance file. Documents architecture, build commands, patterns, known issues, and configuration. |

## Configuration

The active database connection is stored in `dbconfig.json` under `%LOCALAPPDATA%/WinformsVibes/`. Edit `DatabaseName` to switch databases, or delete the file to trigger the setup dialog on next launch.

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| FluentNHibernate | 3.4.0 | Fluent NHibernate mappings for database access |
| Microsoft.Web.WebView2 | 1.0.3967.48 | Chromium-based web rendering inside WinForms |
| ReaLTaiizor | 3.8.1.8 | Material Design controls for WinForms |
| System.Data.SqlClient | 4.8.6 | SQL Server data access |

## Architecture Summary

```
Program.cs (entry)
├── DbConfig.CheckConnection() → DatabaseSetupDialog if unreachable
├── DbConfig.SyncHelpTopics() → truncate + re-seed HelpInfo from XML
├── SplashScreen (Application.Run) → displays app info, animated bear
└── MainForm (Application.Run)
    ├── CrownMenuStrip (File, Edit, View, Settings, Chat, Help)
    ├── TabControl → WorldMapTab (WebView2 + coord panel)
    └── CrownStatusStrip (live clock, 1-second timer)

Menu actions:
  Chat > AI Chat → ChatWindow (singleton, OpenAIChatClient)
  Help > Contents → HelpWindow (browsable help topics)
  Help > AI Help → AIHelpWindow ("Fella", HelpInfo in system prompt)
  WorldMap "Tell Me More!" → AIMapWindow (city lookup via AskAsync)
```

### Key Patterns (for Claude and future developers)

1. **WebView2 init:** `async void` with `await EnsureCoreWebView2Async()` before `CoreWebView2.Navigate()`. Do NOT use `ContinueWith` with async lambdas.
2. **Singleton windows:** ChatWindow, AIHelpWindow, AIMapWindow use a static `_instance` field with `GetInstance()`. X hides, doesn't close — chat history persists.
3. **Layout order:** TabControl first, StatusStrip second, MenuStrip last. `MainMenuStrip` set after all controls are added.
4. **HelpInfo sync:** XML is source of truth. `SyncHelpTopics()` truncates and re-seeds on every launch.
5. **Config location:** `%LOCALAPPDATA%/WinformsVibes/dbconfig.json` — writable even when installed to Program Files.
6. **Database seeding:** `SchemaExport` from Fluent NHibernate mappings (no raw SQL DDL). `BuildFluentConfig()` is the shared helper.
7. **Test mocking:** `MockChatClient` subclasses `OpenAIChatClient` and overrides `ChatAsync`. Tests use reflection for private members.
8. **PreBuild xcopy:** Shared JS/DLLs copied into each module before build. Changes to `Assets/` require rebuilding dependent modules.

## Known Issues

- `dotnet run` in Git Bash exits immediately (GUI detaches from shell). Use `RunMe.bat` or the compiled `.exe`.
- `WinformsVibes.slnx` is a minimal stub — do not rely on it. Build via `.csproj` files or batch scripts.

## TODO

- [x] Validate HelpInfo data population on application start
