# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WinformsVibes is a .NET 10.0 Windows Forms desktop application featuring a menu bar, status bar, and tabbed content (live clock, embedded Google Maps via WebView2).

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
`Program.cs` — standard WinForms bootstrap with `Application.Run(new MainForm())`.

### Main Form (`MainForm.cs`)
Single-form application with:
- **MenuStrip** — File, Edit, View, Settings, Help menus with keyboard shortcuts (F11 fullscreen, F1 about)
- **TabControl** — two tabs:
  - **Clock** — Label updated by a 1-second `System.Windows.Forms.Timer`
  - **World Map** — WebView2 control initialized via `async void InitializeMapAsync` that calls `EnsureCoreWebView2Async` then navigates to Google Maps
- **StatusStrip** — "Ready" label at bottom

### WebView2 Initialization Pattern
Critical: use `async void` with `await EnsureCoreWebView2Async()` before calling `CoreWebView2.Navigate()`. Do NOT use `ContinueWith` with async lambdas — the inner await is not tracked by the outer task, causing silent navigation failures.

### Layout Order
Controls must be added in this order: TabControl first, StatusStrip second, MenuStrip last. `MainMenuStrip` is set after all controls are added. DockStyle.Fill on TabControl fills remaining space between menu and status bar.

## Dependencies

- `Microsoft.Web.WebView2` v1.0.3967.48 — Chromium-based web rendering inside WinForms

## Known Issues

- WindowsBase version conflict warning (MSB3277) from WebView2 referencing net5.0 assemblies against net10.0 — harmless, safe to ignore
