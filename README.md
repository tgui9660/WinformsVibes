# Winforms Vibes

> This project was entirely vibe coded using a local Qwen3.6 LLM running on an NVIDIA RTX 3090 Ti with the latest Llama.cpp and MTP support.

## What is this?

Winforms Vibes is a Windows desktop application that combines an interactive world map with AI-powered chat. Click anywhere on the map to get coordinates, then ask an AI assistant about places near that location. Browse built-in help topics, chat with a general-purpose AI, or use the AI help assistant ("Fella") that knows your application's help content. First launch walks you through connecting to a database — after that, the app starts right up.

**In short:** interactive maps + AI chat + built-in help, all in one Windows desktop app.

**At a glance:**
- **Interactive world map** — click to get coordinates, ask the AI about nearby cities
- **AI chat windows** — general chat, map-specific assistant, and a help-aware AI ("Fella")
- **Built-in help browser** — searchable topics grouped by category
- **Multi-database support** — SQL Server, PostgreSQL, or MySQL
- **First-run setup** — simple dialog to choose a database provider and connect
- **One-click install** — run the installer or extract the release folder and launch

## Quick Start for Customers

1. Run the installer (or extract the release folder)
2. Launch `WinformsVibes.exe`
3. On first run, choose your database provider (SQL Server, PostgreSQL, or MySQL) and enter connection details
4. Click the map, press "Tell Me More!" to ask the AI about nearby places
5. Use **Chat > AI Chat** for general conversation, **Help > AI Help** for application assistance

---

A Windows Forms desktop application built with .NET 10.0, featuring a splash screen, menu bar, status bar, embedded Google Maps via WebView2, and AI chat powered by a local OpenAI-compatible endpoint. Entirely AI-generated.

![App Screenshot](AppScreenshot.png)

## Features

- **Splash Screen** — displays application info (name, version, author, framework, database, server, user) fetched from the database. Animated dancing bear below the user line. Bottom toolbar with a right-aligned Continue button to proceed.
- **Main Form** — menu bar with File, Edit, View, Chat, Settings, and Help menus. Restores to 1280x800 when unmaximized.
- **Title Bar Tooltips** — hovering over minimize, maximize, or close buttons on any window shows a tooltip ("Minimize", "Maximize"/"Restore Down", "Close")
- **World Map** — embedded Google Maps via WebView2 (Chromium-based). Lat/Long coordinates are populated by clicking the map or entering them manually in the coord panel.
- **Live Clock** — status bar clock that updates every second
- **Database Setup** — first-run wizard with a provider selector and inputs for server, database name, username, and password
- **Help Topics** — browsable help content covering project structure, GUI features, and usage
- **AI Chat** — chat with a local LLM via Chat > AI Chat
- **Fella - AI Helper** — context-aware help assistant that uses HelpInfo data to answer questions (Help > AI Help). Title bar shows a question mark icon and displays a red welcome message.
- **AI Map Chat** — singleton chat window opened by the "Tell Me More!" button. When coordinates are set (by clicking the map or entering Lat/Long), the button launches an AI agent that searches for the first city within a 5-mile radius and returns information about it. Exposes `AskAsync(string message)` for programmatic use. Assistant responses are green.
- **Help Sync** — HelpInfo table is automatically synced with HelpTopics.xml on every launch
- **Build Release** — BuildRelease.bat publishes a release to a timestamped folder under Releases/

## Database Support

Winforms Vibes supports three database backends. On first launch, the setup dialog lets you choose one:

| Provider | Default Username | Connection Details |
|---|---|---|
| **SQL Server** | `sa` | Server name/instance, database name, SQL authentication |
| **PostgreSQL** | `postgres` | Host, database name, username, password |
| **MySQL** | `root` | Server, database name, username, password |

Each provider uses its own connection library and NHibernate dialect. The application creates the database (if it doesn't exist), generates the schema via Fluent NHibernate `SchemaExport`, and seeds default data — all automatically.

### Schema

Both databases use the same two tables:

| Table | Columns |
|---|---|
| `ApplicationInfo` | Id, ApplicationName, Author, Version, Description, Framework, Dependencies, CreatedAt, UpdatedAt |
| `HelpInfo` | Id, Category, Topic, Content |

The `Content` and `Dependencies` columns use provider-appropriate long-string types (`nvarchar(max)` for SQL Server, `text` for PostgreSQL, `LONGTEXT` for MySQL) configured via `DbConfig.LongStringSqlType`.

## Requirements

- .NET 10.0 SDK
- One of the following databases:
  - **SQL Server** with `sa` authentication enabled
  - **PostgreSQL** 12+
  - **MySQL** 5+
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

If no database is configured (or the configured database is unreachable), a dark-themed setup dialog appears with five fields:

- **Provider** — dropdown with SQL Server, PostgreSQL, and MySQL (defaults to SQL Server)
- **Server** — defaults to `localhost`
- **Database** — required, no default
- **Username** — updates automatically when provider changes (`sa` / `postgres` / `root`)
- **Password** — masked input

Press Enter in any field to submit, Escape to cancel.

After creating a database, the app seeds it with default `ApplicationInfo` and `HelpInfo` data, shows the splash screen, then launches the main form.

### Build a Distributable Release

```powershell
.\BuildRelease.bat
```

Publishes a framework-dependent release (`win-x64`) to `Releases/Build-{timestamp}/`. Release builds use a separate configuration directory (`%LOCALAPPDATA%\WinformsVibes-Release\`) so dev credentials can never leak into a published build.

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
| `WinformsVibes.csproj` | Main project file. Targets `net10.0-windows`, enables Windows Forms and nullable refs. Excludes `Tests/` from compilation. Copies `HelpTopics.xml` and `DancingBear.gif` to output on build. Excludes `dbconfig*.json` from publish output. Suppresses MSB3277 (WebView2 WindowsBase version conflict). |
| `WinformsVibes.Tests.csproj` | Test project. References the main project, uses NUnit 4.2.2. Uses `<Compile Remove>` to avoid CS0311/CS0436 conflicts with the main project. |
| `Directory.Build.props` | Sets `obj\$(MSBuildProjectFile)\` per project to avoid MSBuild obj-folder conflicts between main and test projects. |
| `RunMe.bat` | Builds the project in Debug mode then launches the `.exe`. Quick dev launcher. |
| `BuildRelease.bat` | Runs `dotnet publish -c Release -r win-x64` to a timestamped `Releases/Build-{timestamp}/` folder. Clears the release config file so the setup dialog appears on first run. |
| `BuildInstaller.bat` | Builds a release and runs Inno Setup to create an installer executable. |
| `CreateInstaller.iss` | Inno Setup script defining the installer — app name, output path, files to include, start menu shortcut. |
| `RunTests.bat` | Runs `dotnet test` and pauses on completion so the user can see results. |
| `screenshot.ps1` | PowerShell script for capturing the main form screenshot. |

### GUI Forms (`GUI/`)

All UI forms live here under namespace `WinformsVibes.GUI`.

| File | What it does |
|---|---|
| `MainForm.cs` | Main application window. Extends `TitleBarTooltipMaterialForm` (MaterialForm subclass). `Size` set to 1280x800 before maximizing so restore-down gives a reasonable size. Contains a `CrownMenuStrip` with dark renderer (`DarkMenuRenderer`), `TabControl` with one tab (World Map), `CrownStatusStrip` with a live clock updated by a 1-second timer, and a procedurally drawn bear-face icon via `CreateBearIcon()`. Menu items: File (New/Open/Save/Exit), Edit (Copy/Paste), View (Toggle Fullscreen/About), Settings (Preferences), Chat (AI Chat), Help (Contents/AI Help/About). |
| `SplashScreen.cs` | FixedDialog splash form. Dark theme. Displays application info (name, version, author, framework, database, server, user) from the database. Shows an animated dancing bear (`DancingBear.gif`, 100x100px, `SizeMode.Zoom`) in a PictureBox below the user line. Bottom toolbar with right-aligned "Continue" button. Clicking Continue or X closes the splash and proceeds to the main form. |
| `DatabaseSetupDialog.cs` | Dark-themed first-run dialog. Five inputs: Provider (ComboBox with SQL Server, PostgreSQL, MySQL — defaults to SQL Server), Server (defaults `localhost`), Database (required, no default), Username (defaults to `sa`/`postgres`/`root` depending on selected provider), Password (masked via `UseSystemPasswordChar`). Focus lands on Database field. Exposes `Provider`, `Server`, `DatabaseName`, `UserId`, `Password` properties. Enter submits, Escape cancels. |
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
| `DbConfig.cs` | Fluent NHibernate configuration for SQL Server, PostgreSQL, and MySQL. Connection details loaded via `DbSettingsManager.Load()`. Key methods: `LongStringSqlType` returns provider-appropriate long-string SQL type (`nvarchar(max)` / `text` / `LONGTEXT`); `BuildFluentConfig()` builds the FluentConfiguration with explicit `Add<ApplicationInfoMap>()` and `Add<HelpInfoMap>()` mappings; `CheckConnection()` tests reachability; `CreateAndSeedDatabase()` creates the database, uses `SchemaExport` (drop-then-create to ensure fresh schema), seeds `ApplicationInfo` and `HelpInfo`, saves settings; `SyncHelpTopics()` truncates HelpInfo and re-seeds from XML on every launch. Properties: `CurrentDatabaseName`, `CurrentServer`, `CurrentUserId`, `Provider`. |
| `DbSettings.cs` | `DbSettings` POCO with Provider (`DatabaseProvider` enum: SqlServer, PostgreSQL, MySql), Server, DatabaseName, UserId, Password. All fields default to empty — no hardcoded credentials. `DbSettingsManager` reads/writes `dbconfig.{Debug,Release}.json` from `%LOCALAPPDATA%/WinformsVibes/` (Debug) or `%LOCALAPPDATA%/WinformsVibes-Release/` (Release) using `System.Text.Json`. Debug builds migrate from old app-directory location on first run. Release builds never migrate from dev directories — they use a separate AppData path to prevent credential leakage. |

### Entities & Mappings

| File | What it does |
|---|---|
| `Models/ApplicationInfo.cs` | Entity model: Name, Version, Author, Framework, Dependencies. `Dependencies` column uses `CustomSqlType(DbConfig.LongStringSqlType)` for provider-aware long strings. |
| `Models/HelpInfo.cs` | Entity model: Category, Topic, Content. `Content` column uses `CustomSqlType(DbConfig.LongStringSqlType)` for provider-aware long strings. |
| `Maps/ApplicationInfoMap.cs` | Fluent NHibernate map for ApplicationInfo. `DatabaseName`, `Server`, `UserId` are **not mapped** — set at runtime for display. |
| `Maps/HelpInfoMap.cs` | Fluent NHibernate map for HelpInfo. Proxy validation and lazy loading are disabled. |

### Data & Assets

| File | What it does |
|---|---|
| `HelpTopics.xml` | XML defining all help topics. Copied to output on build. Each `<Topic>` has `Category` and `Name` attributes with text content. Seeded into the `HelpInfo` table on database creation and synced on every launch. **This is the single source of truth for help content.** Edit this file to change what appears in the Help window. |
| `DancingBear.gif` | Animated dancing bear GIF. Copied to output on build. Displayed in the splash screen PictureBox (100x100px, `SizeMode.Zoom`). |

### Tests (`Tests/`)

69 tests across 6 test files, using NUnit 4.2.2.

| File | Tests | What they cover |
|---|---|---|
| `ChatWindowTests.cs` | 19 | Singleton pattern, form properties (title, size, icon), API constants (key, model), chat log content, append methods (user/assistant/system with color formatting), form closing behavior (hide vs close), chat flow with `MockChatClient` (fake responses, error handling, empty message guard) |
| `AIHelpWindowTests.cs` | 13 | Singleton pattern, window title ("Fella - AI Helper"), question mark icon, form properties, API constants, connection message, welcome message, form closing behavior |
| `AIMapWindowTests.cs` | 13 | Singleton pattern, window title ("AI Map Chat"), form properties, API constants, connection message, append methods (user/assistant), form closing behavior |
| `DatabaseSetupDialogTests.cs` | 12 | Dialog title, provider selector (3 options: SQL Server, PostgreSQL, MySQL), default provider (SQL Server), default server (`localhost`), default username (`sa`), username updates per provider selection (`sa`/`postgres`/`root`) |
| `DbSettingsTests.cs` | 8 | No hardcoded credentials (all fields default to empty), `DatabaseProvider` enum contains all 3 values (SqlServer, PostgreSQL, MySql), provider can be set on new settings |
| `DatabaseTests.cs` | 3 | SQL Server integration — database creation, `SchemaExport` table creation (validates both ApplicationInfo and HelpInfo exist), full flow with seeding (validates ApplicationInfo row count). `OneTimeSetUp` drops test database before running. |

### Configuration & Settings

| File | What it does |
|---|---|
| `.claude/settings.json` | Claude Code project settings. |
| `CLAUDE.md` | This project's Claude Code guidance file. Documents architecture, build commands, patterns, known issues, and configuration. |

## Configuration

The active database connection is stored in `dbconfig.{Debug,Release}.json` under:

- **Debug builds:** `%LOCALAPPDATA%\WinformsVibes\dbconfig.Debug.json`
- **Release builds:** `%LOCALAPPDATA%\WinformsVibes-Release\dbconfig.release.json`

The `Provider` field determines which database type is used (`SqlServer`, `PostgreSQL`, or `MySql`). Release builds use a separate AppData directory to prevent dev credentials from leaking into published builds. Edit `DatabaseName` to switch databases, or delete the file to trigger the setup dialog on next launch.

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| FluentNHibernate | 3.4.0 | Fluent NHibernate mappings for database access |
| Microsoft.Web.WebView2 | 1.0.3967.48 | Chromium-based web rendering inside WinForms |
| ReaLTaiizor | 3.8.1.8 | Material Design controls for WinForms |
| System.Data.SqlClient | 4.8.6 | SQL Server data access |
| Npgsql | 6.0.11 | PostgreSQL data access |
| MySqlConnector | 2.3.5 | MySQL data access |

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

Database providers:
  SQL Server  → MsSqlConfiguration.MsSql2012, SqlConnection
  PostgreSQL  → PostgreSQLConfiguration.PostgreSQL82, NpgsqlConnection
  MySQL       → MySQLConfiguration.Standard, MySqlConnection
```

### Key Patterns (for Claude and future developers)

1. **Only Fluent NHibernate for schema:** Use `SchemaExport` from the Fluent NHibernate mappings to create tables. Never use raw DDL (CREATE TABLE, ALTER TABLE, etc.) for schema operations. Raw SQL is only acceptable for simple data operations (INSERT, SELECT, TRUNCATE).
2. **WebView2 init:** `async void` with `await EnsureCoreWebView2Async()` before `CoreWebView2.Navigate()`. Do NOT use `ContinueWith` with async lambdas.
3. **Singleton windows:** ChatWindow, AIHelpWindow, AIMapWindow use a static `_instance` field with `GetInstance()`. X hides, doesn't close — chat history persists.
4. **Layout order:** TabControl first, StatusStrip second, MenuStrip last. `MainMenuStrip` set after all controls are added.
5. **HelpInfo sync:** XML is source of truth. `SyncHelpTopics()` truncates and re-seeds on every launch.
6. **Provider-aware column types:** Long-string columns use `CustomSqlType(DbConfig.LongStringSqlType)` so SchemaExport generates the correct type per provider.
7. **Release config isolation:** Release builds read/write from `%LOCALAPPDATA%\WinformsVibes-Release\` — completely separate from the Debug config directory.
8. **Test mocking:** `MockChatClient` subclasses `OpenAIChatClient` and overrides `ChatAsync`. Tests use reflection for private members.

## Known Issues

- `dotnet run` in Git Bash exits immediately (GUI detaches from shell). Use `RunMe.bat` or the compiled `.exe`.
- `WinformsVibes.slnx` is a minimal stub — do not rely on it. Build via `.csproj` files or batch scripts.

## TODO

- [x] Validate HelpInfo data population on application start
- [x] Add multi-database support (SQL Server, PostgreSQL, MySQL)
- [x] Fix Content column truncation for long help topic text
- [x] Prevent dev credentials from leaking into release builds
