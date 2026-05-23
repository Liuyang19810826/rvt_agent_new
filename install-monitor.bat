@echo off
chcp 65001 >nul
echo ==========================================
echo AI Agent Services Monitor Installation
echo ==========================================
echo.

powershell -WindowStyle Hidden -ExecutionPolicy Bypass -Command "& '%~dp0services-monitor.ps1' -Install"

echo.
echo Service monitor installed. It will check every 120 seconds in background.
echo.
pause
