@echo off
setlocal

:: Get a robust timestamp via PowerShell (YYYYMMDD_HHmmSS)
for /f "delims=" %%i in ('powershell -command "Get-Date -Format 'yyyyMMdd_HHmmSS'"') do set TIMESTAMP=%%i

set RELEASES_DIR=%~dp0Releases
if not exist "%RELEASES_DIR%" mkdir "%RELEASES_DIR%"

set RELEASE_DIR=%RELEASES_DIR%\Build-%TIMESTAMP%

:: Publish the release
dotnet publish WinformsVibes.csproj -c Release -r win-x64 --no-self-contained -o "%RELEASE_DIR%"

if %ERRORLEVEL% equ 0 (
    echo Release built: %RELEASE_DIR%

    :: Clear release config so setup dialog appears on first run of the release
    set APPDATA_CONFIG=%LOCALAPPDATA%\WinformsVibes-Release\dbconfig.release.json
    if exist "%APPDATA_CONFIG%" (
        echo Clearing release config for fresh start...
        del "%APPDATA_CONFIG%"
    )

    :: Remove WebView2 cache directory if present
    if exist "%RELEASE_DIR%\WinformsVibes.exe.WebView2" (
        echo Removing WebView2 cache from release...
        rmdir /s /q "%RELEASE_DIR%\WinformsVibes.exe.WebView2"
    )
) else (
    echo Build failed.
    exit /b 1
)
