# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with this repository.

## Project Overview

WinformsVibes is a .NET 10.0 Windows Forms desktop application, entirely AI-generated using a local Qwen3.6 LLM. It features a splash screen, menu bar, status bar, and tabbed content with embedded Google Maps via WebView2. Application info is fetched from a database via Fluent NHibernate and displayed on the splash screen at startup. Supports SQL Server, PostgreSQL, and MySQL as backend databases. On first launch (or when the configured database is unavailable), a setup dialog lets the user choose a provider, create and name a new database. Includes a help topics browser, an AI chat window, an AI help assistant that uses HelpInfo data as context, and an AI map chat window for programmatic queries. All AI windows connect to a local OpenAI-compatible endpoint (192.168.2.15:8888).

**System requirements:** .NET 10.0 SDK, SQL Server with `sa` authentication, PostgreSQL 12+, or MySQL 5+, Windows 10+ (for WebView2).

## Build and Run

```powershell
# Build
dotnet build WinformsVibes.csproj -p:Configuration=Debug

# Run (no IIS needed — native WinForms exe)
dotnet run --project WinformsVibes.csproj

# Run via batch file (double-click or from cmd) — builds then launches
.\RunMe.bat

# Build a distributable release
.\BuildRelease.bat

# Run the compiled exe directly
.\bin\Debug\net10.0-windows\WinformsVibes.exe
```

### Tests

Tests are in `Tests/` under the `WinformsVibes.Tests.csproj` project, using NUnit 4.2.2.

```powershell
# Run all tests
dotnet test WinformsVibes.Tests.csproj
# Or use the batch file (runs tests and pauses)
.\RunTests.bat

# Run a single test by name
dotnet test WinformsVibes.Tests.csproj --filter "FullyQualifiedName~GetInstance_ReturnsSameInstanceOnRepeatedCalls"

# Run tests in a test fixture
dotnet test WinformsVibes.Tests.csproj --filter "FullyQualifiedName~ChatWindowTests"

# Run database tests (requires SQL Server connection)
dotnet test WinformsVibes.Tests.csproj --filter "FullyQualifiedName~DatabaseTests"
```

The test project references `WinformsVibes.csproj` and excludes the main project's source files via `<Compile Remove>` to avoid CS0311/CS0436 conflicts. Tests use reflection to access private fields and methods, and a `MockChatClient` to test the chat flow without a live endpoint. `DatabaseTests` uses a `OneTimeSetUp` to drop the test database (`testdb_schema`) before running, so the tests always work against a fresh schema.

### Test Coverage (69 tests across 6 files)

| File | Tests | Coverage |
|---|---|---|
| `ChatWindowTests.cs` | 19 | Singleton, form properties, API constants, chat log, append methods, form closing, chat flow with mock client |
| `AIHelpWindowTests.cs` | 13 | Singleton, window title, icon, form properties, API constants, connection/welcome messages, form closing |
| `AIMapWindowTests.cs` | 13 | Singleton, window title, form properties, API constants, connection message, append methods, form closing |
| `DatabaseSetupDialogTests.cs` | 12 | Dialog title, provider selector (3 options), defaults, username updates per provider |
| `DbSettingsTests.cs` | 8 | No hardcoded credentials, all 3 providers in enum, provider can be set |
| `DatabaseTests.cs` | 3 | SQL Server integration — database creation, SchemaExport, seeding |

## Architecture

### Entry Point
`Program.cs` — checks database connectivity. If unreachable, shows the database setup dialog (`DatabaseSetupDialog`). On failure, copies the error message to the clipboard via `Clipboard.SetText()`. On success, syncs HelpInfo with HelpTopics.xml via `DbConfig.SyncHelpTopics()`, shows the splash screen (`Application.Run(splash)`), then launches the main form after the splash is closed (`Application.Run(new MainForm())`).

### GUI (`GUI/`)
All UI forms live here under namespace `WinformsVibes.GUI`.

#### Database Setup Dialog (`GUI/DatabaseSetupDialog.cs`)
Dark-themed dialog shown when no database connection is available. Five inputs: Provider (ComboBox with "SQL Server", "PostgreSQL", and "MySQL", defaults to SQL Server), Server (defaults to `localhost`), Database (required, no default), Username (defaults to `sa` for SQL Server, `postgres` for PostgreSQL, `root` for MySQL — updates when provider changes), and Password (masked with `UseSystemPasswordChar`). Focus lands on the Database field. Exposes `Provider`, `Server`, `DatabaseName`, `UserId`, and `Password` properties. Pressing Enter in any field submits; Escape cancels.

#### Splash Screen (`GUI/SplashScreen.cs`)
FixedDialog form with a dark theme that displays application info (name, version, author, framework, database, server, user) fetched from the database. A `PictureBox` below the user line shows an animated dancing bear from `DancingBear.gif` (100×100px, `SizeMode.Zoom`). A bottom toolbar contains a right-aligned "Continue" button. Clicking the button or the X closes the splash and proceeds to the main form.

#### Main Form (`GUI/MainForm.cs`)
Extends `TitleBarTooltipMaterialForm` (which extends `MaterialForm` from ReaLTaiizor). `Size` is set to 1280×800 before maximizing so restoring down gives a reasonable window size. Single-form application with:
- **CrownMenuStrip** — ReaLTaiizor's material menu control with a custom `DarkMenuRenderer` (`ToolStripProfessionalRenderer` subclass with dark colors). Menus: File (New, Open, Save, Exit), Edit (Copy, Paste), View (Toggle Fullscreen, About), Settings (Preferences), Chat (AI Chat), Help (Contents, AI Help, About)
- **TabControl** — one tab:
  - **World Map** — `WorldMapTab` UserControl (see below)
- **CrownStatusStrip** — "Ready" label at bottom, live clock updated by a 1-second `System.Windows.Forms.Timer`
- **Icon** — procedurally drawn bear face via `CreateBearIcon()`

#### World Map Tab (`GUI/WorldMapTab.cs`)
`UserControl` that encapsulates the Google Maps tab. Contains a `WebView2` loaded with Google Maps and a bottom `coordPanel` (60px height) with Lat/Long `MaterialTextBoxEdit` inputs (62px tall) with rounded regions and a `MaterialButton` "Tell Me More!" button. Enter in a coord field navigates the map. The button opens the `AIMapWindow` and asks the agent about the first city within a 5 mile radius of the coordinates. `SourceChanged` event syncs the URL coordinates back into the Lat/Long inputs. Button is disabled when both coords are 0. Uses `async void` with `await EnsureCoreWebView2Async()` for WebView2 initialization.

#### Help Window (`GUI/HelpWindow.cs`)
Dark-themed window opened via Help > Contents. Has a question mark icon (`SystemIcons.Question`). Groups help topics by unique Category+Topic pairs and displays all content values when selected. Search filters across category, topic name, and all content values. Uses `GroupedHelpTopic` record with a `List<string>` of contents. Also defines the `HelpTopic` record used by `DbConfig.GetHelpTopics()`.

#### AI Chat Window (`GUI/ChatWindow.cs`)
Singleton window opened via Chat > AI Chat. Has a yellow smiley face icon drawn via `CreateSmileyIcon()`. Connects to an OpenAI-compatible endpoint at `http://192.168.2.15:8888/v1` using the `OpenAIChatClient`. API key (`"apikey"`) and model (`"Qwen3.6-27B-MTP-Q4_K_M"`) are hardcoded. Clicking X hides the window rather than closing it, preserving chat history across open/close cycles. User messages are displayed in blue. Assistant responses have a yellow "Assistant:" label with green body text. System messages are gray. RTF is preserved when removing the "Thinking..." placeholder so user message formatting is retained. Chat log uses Consolas 15f, input and send button use Segoe UI 16.5f with matching explicit heights.

#### AI Help Window (`GUI/AIHelpWindow.cs`)
Titled "Fella - AI Helper" with a question mark icon (`SystemIcons.Question`). Opened via Help > AI Help. Same singleton pattern, hides on close, preserves chat history. Uses same font sizes and layout as ChatWindow. Displays a red welcome message: "Welcome to Fella! Your helpful AI dude." Loads all HelpInfo topics from the database at startup and includes them in the system prompt so the AI can answer questions based on actual help content.

#### AI Map Window (`GUI/AIMapWindow.cs`)
Singleton chat window titled "AI Map Chat". Same UI pattern as ChatWindow (dark theme, Consolas 15f log, Segoe UI 16.5f input). Exposes `AskAsync(string message)` so other components can send messages programmatically. Assistant responses use the same yellow-label + green-body pattern as ChatWindow. Wired to the "Tell Me More!" button in the World Map tab — clicking it asks about the first city within a 5 mile radius of the selected coordinates.

### OpenAI Chat Client (`AI/OpenAIChatClient.cs`)
Uses `HttpClient` with `System.Text.Json` to call the OpenAI `/chat/completions` endpoint. Takes `apiKey`, `model`, and optional `baseUrl` in the constructor. `ChatAsync` is `virtual` (accepts an optional `systemPrompt` parameter, defaults to "You are a helpful assistant.") to allow mocking in tests. No external NuGet packages required. Implements `IDisposable` to clean up the HttpClient.

### Database (`Database/`)
Database access layer under namespace `WinformsVibes.Database`.

**IMPORTANT: Only use Fluent NHibernate for schema creation.** Never use raw DDL (CREATE TABLE, ALTER TABLE, etc.) for schema operations. Use `SchemaExport` from the Fluent NHibernate mappings. Raw SQL is only acceptable for simple data operations (INSERT, SELECT, TRUNCATE).

#### DbConfig (`Database/DbConfig.cs`)
Fluent NHibernate config connecting to SQL Server, PostgreSQL, or MySQL. Connection details and provider are loaded via `DbSettingsManager.Load()` from `dbconfig.json`.

- `LongStringSqlType` — provider-aware property that returns `"nvarchar(max)"` for SQL Server, `"text"` for PostgreSQL, `"LONGTEXT"` for MySQL. Used by the Fluent maps via `CustomSqlType()` so SchemaExport generates the correct column width per provider.
- `BuildFluentConfig()` — shared helper that builds the `FluentConfiguration` with explicit `Add<ApplicationInfoMap>()` and `Add<HelpInfoMap>()` mappings. Uses `MsSqlConfiguration.MsSql2012` for SQL Server, `PostgreSQLConfiguration.PostgreSQL82` for PostgreSQL, and `MySQLConfiguration.Standard` for MySQL. Used by both `SessionFactory` and `CreateAndSeedDatabase`.
- `CheckConnection()` — tests if the configured database is reachable
- `CreateAndSeedDatabase(provider, server, name, userId, password, out errorMessage)` — creates the database (provider-specific), then uses `SchemaExport` from the Fluent NHibernate mappings to create the tables (drop-then-create to ensure fresh schema with correct column types). Seeds `ApplicationInfo` with default app data and `HelpInfo` from `HelpTopics.xml`. Updates `_settings` with all connection details including provider and saves to `dbconfig.json`. Resets `_sessionFactory` so subsequent calls use the new connection. Error message includes inner exception details.
- `CurrentDatabaseName` — exposes the active database name for display on the splash screen
- `CurrentServer` — exposes the active server for display on the splash screen
- `CurrentUserId` — exposes the active user ID for display on the splash screen
- `Provider` — exposes the active `DatabaseProvider` enum (SqlServer, PostgreSQL, or MySql)
- `GetApplicationInfo()` — queries the single ApplicationInfo row
- `GetHelpTopics()` — returns a list of `HelpTopic` records for the HelpWindow
- `SyncHelpTopics()` — truncates HelpInfo and re-seeds from HelpTopics.xml on every launch (XML is the source of truth). Uses provider-specific SQL for TRUNCATE/INSERT.

#### DbSettings (`Database/DbSettings.cs`)
`DbSettings` POCO with Provider (enum `DatabaseProvider`), Server, DatabaseName, UserId, Password properties. All fields default to empty/null — no hardcoded credentials. `DbSettingsManager` reads/writes `dbconfig.json` from `%LOCALAPPDATA%/WinformsVibes/` (Debug) or `%LOCALAPPDATA%/WinformsVibes-Release/` (Release) using `System.Text.Json`. Debug builds migrate from old app-directory location on first run. Release builds never migrate from dev directories — they use a separate AppData path to prevent credential leakage.

#### Config File (`dbconfig.{Debug,Release}.json`)
Created automatically in `%LOCALAPPDATA%/WinformsVibes/` when the user creates a database via the setup dialog. The config filename is scoped by build configuration (e.g., `dbconfig.Debug.json`, `dbconfig.Release.json`) so debug and release builds maintain separate database connections. On first run after an update that introduces scoping, the legacy `dbconfig.json` is migrated to the appropriate scoped name. Deleting the scoped config file triggers the setup dialog on next launch for that build configuration.

#### Entities & Mappings
- `ApplicationInfo` (`Models/ApplicationInfo.cs`) — mapped by `ApplicationInfoMap` (`Maps/ApplicationInfoMap.cs`). `Dependencies` column uses `CustomSqlType(DbConfig.LongStringSqlType)` for provider-aware long strings.
- `HelpInfo` (`Models/HelpInfo.cs`) — mapped by `HelpInfoMap` (`Maps/HelpInfoMap.cs`). `Content` column uses `CustomSqlType(DbConfig.LongStringSqlType)` for provider-aware long strings.
- Proxy validation and lazy loading are disabled
- `ApplicationInfo.DatabaseName`, `Server`, and `UserId` are **not mapped** to the database — set at runtime for display purposes
- PostgreSQL uses quoted identifiers (e.g., `"ApplicationInfo"`, `"HelpInfo"`), SQL Server uses unquoted names, MySQL uses backtick-quoted identifiers

#### Help Topics (`HelpTopics.xml`)
XML file (copied to output on build) that defines the help topics seeded into the `HelpInfo` table. Each `<Topic>` element has `Category` and `Name` attributes and text content. Edit this file to change the help content. On every launch, `SyncHelpTopics()` truncates HelpInfo and re-seeds from the XML — the XML is the single source of truth.

### WebView2 Initialization Pattern
Critical: use `async void` with `await EnsureCoreWebView2Async()` before calling `CoreWebView2.Navigate()`. Do NOT use `ContinueWith` with async lambdas — the inner await is not tracked by the outer task, causing silent navigation failures.

### Layout Order
Controls must be added in this order: TabControl first, StatusStrip second, MenuStrip last. `MainMenuStrip` is set after all controls are added. DockStyle.Fill on TabControl fills remaining space between menu and status bar.

### Title Bar Tooltip Base Classes
- `TitleBarTooltipForm` (`GUI/TitleBarTooltipForm.cs`) — extends `Form`, overrides `WndProc` to intercept `WM_MOUSEMOVE` and send `WM_NCHITTEST` to detect cursor over minimize/maximize/close buttons. Shows tooltips ("Minimize", "Maximize"/"Restore Down", "Close"). Hidden when mouse enters client area. Disposes its `ToolTip` in `OnHandleDestroyed`.
- `TitleBarTooltipMaterialForm` (`GUI/TitleBarTooltipMaterialForm.cs`) — same logic but extends `MaterialForm` for MainForm.
- All standard forms inherit from `TitleBarTooltipForm`; MainForm inherits from `TitleBarTooltipMaterialForm`.

### Singleton Windows
ChatWindow, AIHelpWindow, and AIMapWindow use a static `_instance` field with `GetInstance()`. Clicking X calls `Hide()` instead of closing, preserving the singleton and full chat history.

## Dependencies

- `FluentNHibernate` v3.4.0 — Fluent NHibernate mappings for database access
- `Microsoft.Web.WebView2` v1.0.3967.48 — Chromium-based web rendering inside WinForms
- `ReaLTaiizor` v3.8.1.8 — Material Design controls for WinForms
- `System.Data.SqlClient` v4.8.6 — SQL Server data access
- `Npgsql` v6.0.11 — PostgreSQL data access
- `MySqlConnector` v2.3.5 — MySQL data access

## Known Issues

- Running via `dotnet run` in Git Bash exits immediately because the GUI detaches from the shell. Use `RunMe.bat` or the compiled `.exe` directly.
- `WinformsVibes.slnx` is a minimal stub (`<Solution></Solution>`) — do not rely on it. Build via the `.csproj` files or batch scripts.
- `Directory.Build.props` sets `obj\$(MSBuildProjectFile)\` per project to avoid MSBuild conflicts between the main and test projects.
- MSB3277 (WindowsBase version conflict from WebView2) is suppressed via `<NoWarn>MSB3277</NoWarn>` in both `.csproj` files.

## Configuration

The active database connection is stored in `dbconfig.{Debug,Release}.json` under `%LOCALAPPDATA%/WinformsVibes/` (Debug) or `%LOCALAPPDATA%/WinformsVibes-Release/` (Release). The `Provider` field determines which database type is used (`SqlServer`, `PostgreSQL`, or `MySql`). Release builds use a separate AppData directory to prevent dev credentials from leaking. Edit `DatabaseName` to switch databases, or delete the file to trigger the setup dialog on next launch.
