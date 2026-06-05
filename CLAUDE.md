# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WinformsVibes is a .NET 10.0 Windows Forms desktop application with a splash screen, menu bar, status bar, and tabbed content (embedded Google Maps via WebView2). Application info (name, version, author, dependencies) is fetched from a SQL Server database via Fluent NHibernate and displayed on the splash screen at startup.

## Build and Run

```powershell
# Build
dotnet build WinformsVibes.csproj -p:Configuration=Debug

# Run (no IIS needed — native WinForms exe)
dotnet run --project WinformsVibes.csproj

# Run the compiled exe directly
.\bin\Debug\net10.0-windows\WinformsVibes.exe
```

## Architecture

### Entry Point
`Program.cs` — shows the splash screen first (`Application.Run(splash)`), then launches the main form after the splash is closed (`Application.Run(new MainForm())`).

### Splash Screen (`SplashScreen.cs`)
FixedDialog form with a dark theme that displays application info (name, version, author, framework, dependencies) fetched from the database. User clicks the X to close and proceed. The Dependencies field uses a multiline TextBox for text wrapping.

### Main Form (`MainForm.cs`)
Single-form application with:
- **MenuStrip** — File, Edit, View, Settings, Help menus with keyboard shortcuts (F11 fullscreen, F1 about)
- **TabControl** — two tabs:
  - **Clock** — Label updated by a 1-second `System.Windows.Forms.Timer`
  - **World Map** — WebView2 control initialized via `async void InitializeMapAsync` that calls `EnsureCoreWebView2Async` then navigates to Google Maps
- **StatusStrip** — "Ready" label at bottom

### Database (`DbConfig.cs`)
Fluent NHibernate config connecting to a local SQL Server (`localhost`, database `winformsvibes`, user `sa`). The `ApplicationInfo` entity is mapped via `ApplicationInfoMap` (Fluent mapping, no XML). Proxy validation and lazy loading are disabled.

#### Database Setup
```sql
CREATE DATABASE winformsvibes;
USE winformsvibes;
CREATE TABLE ApplicationInfo (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationName NVARCHAR(100) NOT NULL,
    Author          NVARCHAR(100) NOT NULL,
    Version         NVARCHAR(50)  NOT NULL,
    Description     NVARCHAR(500),
    Framework       NVARCHAR(50),
    Dependencies    NVARCHAR(MAX),
    CreatedAt       DATETIME2     DEFAULT SYSUTCDATETIME() NOT NULL,
    UpdatedAt       DATETIME2     DEFAULT SYSUTCDATETIME() NOT NULL
);
```

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
