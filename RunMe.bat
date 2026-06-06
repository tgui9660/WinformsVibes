@echo off
call dotnet build "%~dp0WinformsVibes.csproj" -p:Configuration=Debug
start "" "%~dp0bin\Debug\net10.0-windows\WinformsVibes.exe"
