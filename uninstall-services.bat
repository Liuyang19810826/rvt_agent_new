@echo off
chcp 65001 >nul
echo ==========================================
echo AI Agent Services Uninstallation
echo ==========================================
echo.

:: Check for admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Please run as Administrator!
    pause
    exit /b 1
)

echo [1/3] Stopping services...
net stop AIAgentWeb 2>nul
net stop AIAgentApi 2>nul
timeout /t 2 /nobreak >nul

echo.
echo [2/3] Removing services...
sc delete AIAgentWeb 2>nul
if %errorlevel% equ 0 (
    echo [OK] AIAgentWeb removed
) else (
    echo [INFO] AIAgentWeb not found or already removed
)

sc delete AIAgentApi 2>nul
if %errorlevel% equ 0 (
    echo [OK] AIAgentApi removed
) else (
    echo [INFO] AIAgentApi not found or already removed
)

echo.
echo [3/3] Cleaning up scheduled tasks...
schtasks /delete /tn "AIAgentServices" /f 2>nul

echo.
echo ==========================================
echo Uninstallation Complete!
echo ==========================================
echo.
pause
