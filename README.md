# Winforms Vibes

> This project was entirely vibe coded using a local Qwen3.6 LLM running on an NVIDIA RTX 3090 Ti.

A Windows Forms desktop application built with .NET 10.0, featuring a splash screen, menu bar, status bar, and embedded Google Maps via WebView2.

## Features

- **Splash Screen** — displays application info (name, version, author, framework, database, dependencies) fetched from the database
- **Main Form** — menu bar with File, Edit, View, Settings, and Help menus
- **World Map** — embedded Google Maps via WebView2 (Chromium-based)
- **Live Clock** — status bar clock that updates every second
- **Database Setup** — first-run wizard that creates and configures a SQL Server database automatically
- **Help Topics** — browsable help content covering project structure, GUI features, and usage

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
| `Program.cs` | Entry point — database check, splash screen, main form |
| `SplashScreen.cs` | Startup splash with application info |
| `MainForm.cs` | Main application window with menu, tabs, and status bar |
| `DatabaseSetupDialog.cs` | First-run database creation dialog |
| `DbConfig.cs` | Database connection, Fluent NHibernate config, seed logic |
| `DbSettings.cs` | Connection settings model and JSON config manager |
| `Models/` | Entity models (`ApplicationInfo`, `HelpInfo`) |
| `Maps/` | Fluent NHibernate maps (`ApplicationInfoMap`, `HelpInfoMap`) |
| `HelpTopics.xml` | Help topics seeded into the database on creation |
| `RunMe.bat` | Batch file launcher |

## Configuration

The active database connection is stored in `dbconfig.json` (created automatically in the output directory). Edit the `DatabaseName` field to switch databases, or delete the file to trigger the setup dialog on next launch.

## Dependencies

- **FluentNHibernate** v3.4.0 — Fluent NHibernate mappings
- **Microsoft.Web.WebView2** v1.0.3967.48 — Chromium-based web rendering
- **ReaLTaiizor** v3.8.1.8 — Material Design controls for WinForms
- **System.Data.SqlClient** v4.8.6 — SQL Server data access
