# Winforms Vibes

> This project was entirely vibe coded using a local Qwen3.6 LLM running on an NVIDIA RTX 3090 Ti with the latest Llama.cpp and MTP support.

A Windows Forms desktop application built with .NET 10.0, featuring a splash screen, menu bar, status bar, embedded Google Maps via WebView2, and AI chat powered by a local OpenAI-compatible endpoint.

## Features

- **Splash Screen** — displays application info (name, version, author, framework, database) fetched from the database
- **Main Form** — menu bar with File, Edit, View, Chat, Settings, and Help menus
- **World Map** — embedded Google Maps via WebView2 (Chromium-based)
- **Live Clock** — status bar clock that updates every second
- **Database Setup** — first-run wizard that creates and configures a SQL Server database automatically
- **Help Topics** — browsable help content covering project structure, GUI features, and usage
- **AI Chat** — chat with a local LLM via Chat > AI Chat
- **Fella - AI Helper** — context-aware help assistant that uses HelpInfo data to answer questions (Help > AI Help). Title bar shows a question mark icon and displays a red welcome message.
- **Help Sync** — HelpInfo table is automatically synced with HelpTopics.xml on every launch

## Requirements

- .NET 10.0 SDK
- SQL Server (local or remote) with `sa` authentication enabled
- Windows 10 or later (for WebView2 support)

## Quick Start

1. **Build**
   ```powershell
   dotnet build WinformsVibes.csproj -p:Configuration=Debug
   ```

2. **Run**
   ```powershell
   dotnet run --project WinformsVibes.csproj
   ```
   Or double-click `RunMe.bat` to launch directly.

3. **First Launch** — if no database is configured, a setup dialog appears. Enter a database name and click **Create**. The app handles the rest.

## Project Structure

| Path | Description |
|---|---|
| `Program.cs` | Entry point — database check, help sync, splash screen, main form |
| `SplashScreen.cs` | Startup splash with application info |
| `MainForm.cs` | Main application window with menu, tabs, and status bar |
| `DatabaseSetupDialog.cs` | First-run database creation dialog |
| `DbConfig.cs` | Database connection, Fluent NHibernate config, seed logic, help sync |
| `DbSettings.cs` | Connection settings model and JSON config manager |
| `ChatWindow.cs` | AI chat window connecting to local OpenAI-compatible endpoint |
| `AIHelpWindow.cs` | AI-powered help assistant using HelpInfo context |
| `OpenAIChatClient.cs` | HttpClient-based OpenAI chat completions client |
| `HelpWindow.cs` | Browsable help topics window — groups by unique Category+Topic, shows all content values |
| `Models/` | Entity models (`ApplicationInfo`, `HelpInfo`) |
| `Maps/` | Fluent NHibernate maps (`ApplicationInfoMap`, `HelpInfoMap`) |
| `HelpTopics.xml` | Help topics seeded into the database on creation and synced on every launch |
| `RunMe.bat` | Batch file launcher |

## Configuration

The active database connection is stored in `dbconfig.json` (created automatically in the output directory). Edit the `DatabaseName` field to switch databases, or delete the file to trigger the setup dialog on next launch.

## Help Topics

Edit `HelpTopics.xml` to add, remove, or modify help topics. On every launch, the app compares the row count in the HelpInfo table with the number of `<Topic>` elements in the XML. If they differ, the table is truncated and re-seeded from the XML.

## Dependencies

- **FluentNHibernate** v3.4.0 — Fluent NHibernate mappings
- **Microsoft.Web.WebView2** v1.0.3967.48 — Chromium-based web rendering
- **ReaLTaiizor** v3.8.1.8 — Material Design controls for WinForms
- **System.Data.SqlClient** v4.8.6 — SQL Server data access

## TODO

- [ ] Validate HelpInfo data population on application start
