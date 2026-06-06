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
) else (
    echo Build failed.
    exit /b 1
)
