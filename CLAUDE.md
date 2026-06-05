# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WinformsVibes is a .NET 10.0 Windows Forms desktop application with a splash screen, menu bar, status bar, and tabbed content (embedded Google Maps via WebView2). Application info (name, version, author, dependencies) is fetched from a SQL Server database via Fluent NHibernate and displayed on the splash screen at startup. On first launch (or when the configured database is unavailable), a setup dialog lets the user create and name a new database.

## Build and Run

```powershell
# Build
dotnet build WinformsVibes.csproj -p:Configuration=Debug

# Run (no IIS needed — native WinForms exe)
dotnet run --project WinformsVibes.csproj

# Run via batch file (double-click or from cmd)
.\RunMe.bat

# Run the compiled exe directly
.\bin\Debug\net10.0-windows\WinformsVibes.exe
```

## Architecture

### Entry Point
`Program.cs` — checks database connectivity. If unreachable, shows the database setup dialog (`DatabaseSetupDialog`). On success, shows the splash screen (`Application.Run(splash)`), then launches the main form after the splash is closed (`Application.Run(new MainForm())`).

### Database Setup Dialog (`DatabaseSetupDialog.cs`)
Dark-themed dialog shown when no database connection is available. User enters a database name, clicks Create, and the app creates the database, tables, and seed data. Pressing Enter submits; Escape cancels.

### Splash Screen (`SplashScreen.cs`)
FixedDialog form with a dark theme that displays application info (name, version, author, framework, database, dependencies) fetched from the database. User clicks the X to close and proceed. The Dependencies field uses a multiline TextBox for text wrapping.

### Main Form (`MainForm.cs`)
Single-form application with:
- **MenuStrip** — File, Edit, View, Settings, Help menus with keyboard shortcuts (F11 fullscreen, F1 about)
- **TabControl** — one tab:
  - **World Map** — WebView2 control initialized via `async void InitializeMapAsync` that calls `EnsureCoreWebView2Async` then navigates to Google Maps
- **StatusStrip** — "Ready" label at bottom, live clock updated by a 1-second `System.Windows.Forms.Timer`

### Database (`DbConfig.cs`)
Fluent NHibernate config connecting to a local SQL Server. Connection details (server, database name, user, password) are loaded from `dbconfig.json` at startup. If the file is missing, defaults to `localhost`/`winformsvibes`/`sa`/`password`.

- `CheckConnection()` — tests if the configured database is reachable
- `CreateAndSeedDatabase(name)` — creates the database, the `ApplicationInfo` and `HelpInfo` tables, seeds `ApplicationInfo` with default app data, and seeds `HelpInfo` from `HelpTopics.xml`
- `CurrentDatabaseName` — exposes the active database name for display on the splash screen

#### Config File (`dbconfig.json`)
Created automatically in the output directory (`bin/Debug/net10.0-windows/`) when the user creates a database via the setup dialog. Storing the connection here means the app remembers the database across launches. Deleting this file triggers the setup dialog on next launch.

#### Entities & Mappings
- `ApplicationInfo` (`Models/ApplicationInfo.cs`) — mapped by `ApplicationInfoMap` (`Maps/ApplicationInfoMap.cs`)
- `HelpInfo` (`Models/HelpInfo.cs`) — mapped by `HelpInfoMap` (`Maps/HelpInfoMap.cs`)
- Proxy validation and lazy loading are disabled

#### Help Topics (`HelpTopics.xml`)
XML file (copied to output on build) that defines the help topics seeded into the `HelpInfo` table on database creation. Each `<Topic>` element has `Category` and `Name` attributes and text content. Edit this file to change the help content.

### WebView2 Initialization Pattern
Critical: use `async void` with `await EnsureCoreWebView2Async()` before calling `CoreWebView2.Navigate()`. Do NOT use `ContinueWith` with async lambdas — the inner await is not tracked by the outer task, causing silent navigation failures.

### Layout Order
Controls must be added in this order: TabControl first, StatusStrip second, MenuStrip last. `MainMenuStrip` is set after all controls are added. DockStyle.Fill on TabControl fills remaining space between menu and status bar.

## Dependencies

- `FluentNHibernate` v3.4.0 — Fluent NHibernate mappings for database access
- `Microsoft.Web.WebView2` v1.0.3967.48 — Chromium-based web rendering inside WinForms
- `ReaLTaiizor` v3.8.1.8 — Material Design controls for WinForms
- `System.Data.SqlClient` v4.8.6 — SQL Server data access

## Known Issues

- WindowsBase version conflict warning (MSB3277) from WebView2 referencing net5.0 assemblies against net10.0 — harmless, safe to ignore
- Running via `dotnet run` in Git Bash exits immediately because the GUI detaches from the shell. Use `RunMe.bat` or the compiled `.exe` directly.
