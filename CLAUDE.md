# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with this repository.

## Project Overview

WinformsVibes is a .NET 10.0 Windows Forms desktop application, entirely AI-generated using a local Qwen3.6 LLM. It features a splash screen, menu bar, status bar, and tabbed content with embedded Google Maps via WebView2. Application info is fetched from a SQL Server database via Fluent NHibernate and displayed on the splash screen at startup. On first launch (or when the configured database is unavailable), a setup dialog lets the user create and name a new database. Includes a help topics browser, an AI chat window, an AI help assistant that uses HelpInfo data as context, and an AI map chat window for programmatic queries. All AI windows connect to a local OpenAI-compatible endpoint (192.168.2.15:8888).

**System requirements:** .NET 10.0 SDK, SQL Server with `sa` authentication, Windows 10+ (for WebView2).

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

## Architecture

### Entry Point
`Program.cs` — checks database connectivity. If unreachable, shows the database setup dialog (`DatabaseSetupDialog`). On failure, copies the error message to the clipboard via `Clipboard.SetText()`. On success, syncs HelpInfo with HelpTopics.xml via `DbConfig.SyncHelpTopics()`, shows the splash screen (`Application.Run(splash)`), then launches the main form after the splash is closed (`Application.Run(new MainForm())`).

### GUI (`GUI/`)
All UI forms live here under namespace `WinformsVibes.GUI`.

#### Database Setup Dialog (`GUI/DatabaseSetupDialog.cs`)
Dark-themed dialog shown when no database connection is available. Four inputs: Server (defaults to `localhost`), Database (required, no default), Username (defaults to `sa`), and Password (masked with `UseSystemPasswordChar`). Focus lands on the Database field. Exposes `Server`, `DatabaseName`, `UserId`, and `Password` properties. Pressing Enter in any field submits; Escape cancels.

#### Splash Screen (`GUI/SplashScreen.cs`)
FixedDialog form with a dark theme that displays application info (name, version, author, framework, database, server, user) fetched from the database. A bottom toolbar contains a right-aligned "Continue" button. Clicking the button or the X closes the splash and proceeds to the main form.

#### Main Form (`GUI/MainForm.cs`)
Extends `MaterialForm` from ReaLTaiizor (Material Design form base). Single-form application with:
- **CrownMenuStrip** — ReaLTaiizor's material menu control with a custom `DarkMenuRenderer` (`ToolStripProfessionalRenderer` subclass with dark colors). Menus: File (New, Open, Save, Exit), Edit (Copy, Paste), View (Toggle Fullscreen, About), Settings (Preferences), Chat (AI Chat), Help (Contents, AI Help, About)
- **TabControl** — one tab:
  - **World Map** — WebView2 control loaded with Google Maps. A bottom coordPanel (60px height) has Lat/Long `MaterialTextBox` inputs (62px tall) with rounded regions and a `MaterialButton` "Tell Me More!" button. Enter in a coord field navigates the map. The button opens the AIMapWindow and asks the agent about the first city within a 5 mile radius of the coordinates. `SourceChanged` event syncs the URL coordinates back into the Lat/Long inputs. Button is disabled when both coords are 0.
- **CrownStatusStrip** — "Ready" label at bottom, live clock updated by a 1-second `System.Windows.Forms.Timer`
- **Icon** — procedurally drawn bear face via `CreateBearIcon()`

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

#### DbConfig (`Database/DbConfig.cs`)
Fluent NHibernate config connecting to a local SQL Server. Connection details are loaded via `DbSettingsManager.Load()` from `dbconfig.json`.

- `BuildFluentConfig()` — shared helper that builds the `FluentConfiguration` with explicit `Add<ApplicationInfoMap>()` and `Add<HelpInfoMap>()` mappings. Used by both `SessionFactory` and `CreateAndSeedDatabase`.
- `CheckConnection()` — tests if the configured database is reachable
- `CreateAndSeedDatabase(server, name, userId, password, out errorMessage)` — creates the database, then uses `SchemaExport` from the Fluent NHibernate mappings to create the tables (no raw SQL DDL). Seeds `ApplicationInfo` with default app data and `HelpInfo` from `HelpTopics.xml`. Builds its own connection strings from the method parameters (not `_settings`). Updates `_settings` with all four connection details and saves to `dbconfig.json`. Resets `_sessionFactory` so subsequent calls use the new connection. Error message includes inner exception details.
- `CurrentDatabaseName` — exposes the active database name for display on the splash screen
- `CurrentServer` — exposes the active server for display on the splash screen
- `CurrentUserId` — exposes the active user ID for display on the splash screen
- `GetApplicationInfo()` — queries the single ApplicationInfo row
- `GetHelpTopics()` — returns a list of `HelpTopic` records for the HelpWindow
- `SyncHelpTopics()` — truncates HelpInfo and re-seeds from HelpTopics.xml on every launch (XML is the source of truth)

#### DbSettings (`Database/DbSettings.cs`)
`DbSettings` POCO with Server, DatabaseName, UserId, Password properties. `DbSettingsManager` reads/writes `dbconfig.json` from the output directory using `System.Text.Json`. If the file is missing, returns a `DbSettings` with defaults (`localhost`/`winformsvibes`/`sa`/`password`).

#### Config File (`dbconfig.json`)
Created automatically in the output directory (`bin/Debug/net10.0-windows/`) when the user creates a database via the setup dialog. Storing the connection here means the app remembers the database across launches. Deleting this file triggers the setup dialog on next launch.

#### Entities & Mappings
- `ApplicationInfo` (`Models/ApplicationInfo.cs`) — mapped by `ApplicationInfoMap` (`Maps/ApplicationInfoMap.cs`). `Dependencies` column uses `CustomSqlType("nvarchar(max)")`.
- `HelpInfo` (`Models/HelpInfo.cs`) — mapped by `HelpInfoMap` (`Maps/HelpInfoMap.cs`). `Content` column uses `CustomSqlType("nvarchar(max)")`.
- Proxy validation and lazy loading are disabled
- `ApplicationInfo.DatabaseName`, `Server`, and `UserId` are **not mapped** to the database — set at runtime for display purposes

#### Help Topics (`HelpTopics.xml`)
XML file (copied to output on build) that defines the help topics seeded into the `HelpInfo` table. Each `<Topic>` element has `Category` and `Name` attributes and text content. Edit this file to change the help content. On every launch, `SyncHelpTopics()` truncates HelpInfo and re-seeds from the XML — the XML is the single source of truth.

### WebView2 Initialization Pattern
Critical: use `async void` with `await EnsureCoreWebView2Async()` before calling `CoreWebView2.Navigate()`. Do NOT use `ContinueWith` with async lambdas — the inner await is not tracked by the outer task, causing silent navigation failures.

### Layout Order
Controls must be added in this order: TabControl first, StatusStrip second, MenuStrip last. `MainMenuStrip` is set after all controls are added. DockStyle.Fill on TabControl fills remaining space between menu and status bar.

### Singleton Windows
ChatWindow, AIHelpWindow, and AIMapWindow use a static `_instance` field with `GetInstance()`. Clicking X calls `Hide()` instead of closing, preserving the singleton and full chat history.

## Dependencies

- `FluentNHibernate` v3.4.0 — Fluent NHibernate mappings for database access
- `Microsoft.Web.WebView2` v1.0.3967.48 — Chromium-based web rendering inside WinForms
- `ReaLTaiizor` v3.8.1.8 — Material Design controls for WinForms
- `System.Data.SqlClient` v4.8.6 — SQL Server data access

## Known Issues

- WindowsBase version conflict warning (MSB3277) from WebView2 referencing net5.0 assemblies against net10.0 — harmless, safe to ignore
- Running via `dotnet run` in Git Bash exits immediately because the GUI detaches from the shell. Use `RunMe.bat` or the compiled `.exe` directly.
- `WinformsVibes.slnx` is a minimal stub (`<Solution></Solution>`) — do not rely on it. Build via the `.csproj` files or batch scripts.
- `Directory.Build.props` sets `obj\$(MSBuildProjectFile)\` per project to avoid MSBuild conflicts between the main and test projects.

## Configuration

The active database connection is stored in `dbconfig.json` (created automatically in the output directory). Edit `DatabaseName` to switch databases, or delete the file to trigger the setup dialog on next launch.
