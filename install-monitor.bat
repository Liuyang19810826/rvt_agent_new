@echo off
chcp 65001 >nul
echo ==========================================
echo AI Agent Services Monitor Installation
echo ==========================================
echo.

powershell -ExecutionPolicy Bypass -Command "& '%~dp0services-monitor.ps1' -Install"

echo.
pause
