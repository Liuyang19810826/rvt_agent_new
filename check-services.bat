@echo off
chcp 65001 >nul
echo ==========================================
echo AI Agent Services Status Check
echo ==========================================
echo.

powershell -ExecutionPolicy Bypass -Command "& '%~dp0services-monitor.ps1' -Status"

echo.
pause
