# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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

## Architecture

### Entry Point
`Program.cs` — checks database connectivity. If unreachable, shows the database setup dialog (`DatabaseSetupDialog`). On success, syncs HelpInfo with HelpTopics.xml via `DbConfig.SyncHelpTopics()`, shows the splash screen (`Application.Run(splash)`), then launches the main form after the splash is closed (`Application.Run(new MainForm())`).

### GUI (`GUI/`)
All UI forms live here under namespace `WinformsVibes.GUI`.

#### Database Setup Dialog (`GUI/DatabaseSetupDialog.cs`)
Dark-themed dialog shown when no database connection is available. Inputs for Server (defaults to `localhost`), Database (required, no default), Username (defaults to `sa`), and Password (masked with `UseSystemPasswordChar`). Exposes `Server`, `DatabaseName`, `UserId`, and `Password` properties. Pressing Enter in any field submits; Escape cancels.

#### Splash Screen (`GUI/SplashScreen.cs`)
FixedDialog form with a dark theme that displays application info (name, version, author, framework, database) fetched from the database. User clicks the X to close and proceed.

#### Main Form (`GUI/MainForm.cs`)
Extends `MaterialForm` from ReaLTaiizor (Material Design form base). Single-form application with:
- **MenuStrip** — File (New, Open, Save, Exit), Edit (Copy, Paste), View (Toggle Fullscreen, About), Settings (Preferences), Chat (AI Chat), Help (Contents, AI Help, About)
- **TabControl** — one tab:
  - **World Map** — WebView2 control loaded with Google Maps. A bottom coordPanel has Lat/Long TextBox inputs and a "Tell Me More!" button. Enter in a coord field navigates the map. The button opens the AIMapWindow and asks the agent about the first city within a 5 mile radius of the coordinates. `SourceChanged` event syncs the URL coordinates back into the Lat/Long inputs. Button is disabled when both coords are 0.
- **StatusStrip** — "Ready" label at bottom, live clock updated by a 1-second `System.Windows.Forms.Timer`
- **Icon** — procedurally drawn bear face via `CreateBearIcon()`

#### Help Window (`GUI/HelpWindow.cs`)
Dark-themed window opened via Help > Contents. Has a question mark icon (`SystemIcons.Question`). Groups help topics by unique Category+Topic pairs and displays all content values when selected. Search filters across category, topic name, and all content values. Uses `GroupedHelpTopic` record with a `List<string>` of contents. Also defines the `HelpTopic` record used by `DbConfig.GetHelpTopics()`.

#### AI Chat Window (`GUI/ChatWindow.cs`)
Singleton window opened via Chat > AI Chat. Has a yellow smiley face icon drawn via `CreateSmileyIcon()`. Connects to an OpenAI-compatible endpoint at `http://192.168.2.15:8888/v1` using the `OpenAIChatClient`. API key (`"apikey"`) and model (`"Qwen3.6-27B-MTP-Q4_K_M"`) are hardcoded. Clicking X hides the window rather than closing it, preserving chat history across open/close cycles. User messages are displayed in blue and retain formatting via RTF preservation when removing the "Thinking..." placeholder. Chat log uses Consolas 15f, input and send button use Segoe UI 16.5f with matching explicit heights.

#### AI Help Window (`GUI/AIHelpWindow.cs`)
Titled "Fella - AI Helper" with a question mark icon (`SystemIcons.Question`). Opened via Help > AI Help. Same singleton pattern, hides on close, preserves chat history. Uses same font sizes and layout as ChatWindow. Displays a red welcome message: "Welcome to Fella! Your helpful AI dude." Loads all HelpInfo topics from the database at startup and includes them in the system prompt so the AI can answer questions based on actual help content.

#### AI Map Window (`GUI/AIMapWindow.cs`)
Singleton chat window titled "AI Map Chat". Same UI pattern as ChatWindow (dark theme, Consolas 15f log, Segoe UI 16.5f input). Exposes `AskAsync(string message)` so other components can send messages programmatically. Assistant responses are green (vs gray in ChatWindow). Wired to the "Tell Me More!" button in the World Map tab — clicking it asks about the first city within a 5 mile radius of the selected coordinates.

### OpenAI Chat Client (`AI/OpenAIChatClient.cs`)
Uses `HttpClient` with `System.Text.Json` to call the OpenAI `/chat/completions` endpoint. Takes `apiKey`, `model`, and optional `baseUrl` in the constructor. `ChatAsync` accepts an optional `systemPrompt` parameter (defaults to "You are a helpful assistant."). No external NuGet packages required. Implements `IDisposable` to clean up the HttpClient.

### Database (`Database/`)
Database access layer under namespace `WinformsVibes.Database`.

#### DbConfig (`Database/DbConfig.cs`)
Fluent NHibernate config connecting to a local SQL Server. Connection details are loaded via `DbSettingsManager.Load()` from `dbconfig.json`.

- `CheckConnection()` — tests if the configured database is reachable
- `CreateAndSeedDatabase(name, out errorMessage)` — creates the database, the `ApplicationInfo` and `HelpInfo` tables, seeds `ApplicationInfo` with default app data, and seeds `HelpInfo` from `HelpTopics.xml`. Resets `_sessionFactory` so subsequent calls use the new connection.
- `CurrentDatabaseName` — exposes the active database name for display on the splash screen
- `GetApplicationInfo()` — queries the single ApplicationInfo row
- `GetHelpTopics()` — returns a list of `HelpTopic` records for the HelpWindow
- `SyncHelpTopics()` — truncates HelpInfo and re-seeds from HelpTopics.xml on every launch (XML is the source of truth)

#### DbSettings (`Database/DbSettings.cs`)
`DbSettings` POCO with Server, DatabaseName, UserId, Password properties. `DbSettingsManager` reads/writes `dbconfig.json` from the output directory using `System.Text.Json`. If the file is missing, returns a `DbSettings` with defaults (`localhost`/`winformsvibes`/`sa`/`password`).

#### Config File (`dbconfig.json`)
Created automatically in the output directory (`bin/Debug/net10.0-windows/`) when the user creates a database via the setup dialog. Storing the connection here means the app remembers the database across launches. Deleting this file triggers the setup dialog on next launch.

#### Entities & Mappings
- `ApplicationInfo` (`Models/ApplicationInfo.cs`) — mapped by `ApplicationInfoMap` (`Maps/ApplicationInfoMap.cs`)
- `HelpInfo` (`Models/HelpInfo.cs`) — mapped by `HelpInfoMap` (`Maps/HelpInfoMap.cs`)
- Proxy validation and lazy loading are disabled
- `ApplicationInfo.DatabaseName` is **not mapped** to the database — set at runtime for display purposes

#### Help Topics (`HelpTopics.xml`)
XML file (copied to output on build) that defines the help topics seeded into the `HelpInfo` table. Each `<Topic>` element has `Category` and `Name` attributes and text content. Edit this file to change the help content. On every launch, `SyncHelpTopics()` truncates HelpInfo and re-seeds from the XML — the XML is the single source of truth.

#### Build Release (`BuildRelease.bat`)
Publishes the project in Release mode for win-x64 and outputs to `Releases/Build-{timestamp}/` with a `YYYYMMDD_HHmmSS` timestamp. Framework-dependent build (not self-contained).

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
