@echo off
chcp 65001 >nul
echo ==========================================
echo AI Agent Services Installation
echo ==========================================
echo.

:: Check for admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Please run as Administrator!
    pause
    exit /b 1
)

set "PROJECT_ROOT=C:\Users\caac2\Documents\trae_projects\Rvt_Agent_New"
set "NSSM_URL=https://nssm.cc/release/nssm-2.24.zip"
set "NSSM_ZIP=%TEMP%\nssm.zip"
set "NSSM_DIR=%PROJECT_ROOT%\tools\nssm"

echo [1/5] Checking NSSM installation...
if not exist "%NSSM_DIR%\nssm.exe" (
    echo Downloading NSSM...
    powershell -Command "Invoke-WebRequest -Uri '%NSSM_URL%' -OutFile '%NSSM_ZIP%'"
    if not exist "%NSSM_ZIP%" (
        echo [ERROR] Failed to download NSSM
        pause
        exit /b 1
    )
    
    echo Extracting NSSM...
    if not exist "%PROJECT_ROOT%\tools" mkdir "%PROJECT_ROOT%\tools"
    powershell -Command "Expand-Archive -Path '%NSSM_ZIP%' -DestinationPath '%PROJECT_ROOT%\tools' -Force"
    
    :: Find and copy nssm.exe
    for /f "delims=" %%i in ('dir /s /b "%PROJECT_ROOT%\tools\nssm*.exe"') do (
        copy "%%i" "%NSSM_DIR%\nssm.exe" >nul 2>&1
        goto :nssm_found
    )
    :nssm_found
    
    if not exist "%NSSM_DIR%\nssm.exe" (
        echo [ERROR] Failed to extract NSSM
        pause
        exit /b 1
    )
    
    del "%NSSM_ZIP%" 2>nul
    echo [OK] NSSM installed
) else (
    echo [OK] NSSM already installed
)

echo.
echo [2/5] Stopping existing services...
net stop AIAgentWeb 2>nul
net stop AIAgentApi 2>nul
sc delete AIAgentWeb 2>nul
sc delete AIAgentApi 2>nul
timeout /t 2 /nobreak >nul

echo.
echo [3/5] Installing Web Service (port 5080)...
"%NSSM_DIR%\nssm.exe" install AIAgentWeb "dotnet"
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppParameters "run --project src\AIAgent.Web\AIAgent.Web.csproj --urls ""http://localhost:5080"""
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppDirectory "%PROJECT_ROOT%"
"%NSSM_DIR%\nssm.exe" set AIAgentWeb DisplayName "AI Agent Web Service"
"%NSSM_DIR%\nssm.exe" set AIAgentWeb Description "AI Agent Web Frontend Service on port 5080"
"%NSSM_DIR%\nssm.exe" set AIAgentWeb Start SERVICE_AUTO_START
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppStdout "%PROJECT_ROOT%\logs\web-service.log"
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppStderr "%PROJECT_ROOT%\logs\web-service.log"
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppRotateFiles 1
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppRotateOnline 0
"%NSSM_DIR%\nssm.exe" set AIAgentWeb AppRotateSeconds 86400

echo.
echo [4/5] Installing API Service (port 5078)...
"%NSSM_DIR%\nssm.exe" install AIAgentApi "dotnet"
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppParameters "run --project src\AIAgent.Api\AIAgent.Api.csproj --urls ""http://localhost:5078"""
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppDirectory "%PROJECT_ROOT%"
"%NSSM_DIR%\nssm.exe" set AIAgentApi DisplayName "AI Agent API Service"
"%NSSM_DIR%\nssm.exe" set AIAgentApi Description "AI Agent API Backend Service on port 5078"
"%NSSM_DIR%\nssm.exe" set AIAgentApi Start SERVICE_AUTO_START
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppStdout "%PROJECT_ROOT%\logs\api-service.log"
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppStderr "%PROJECT_ROOT%\logs\api-service.log"
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppRotateFiles 1
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppRotateOnline 0
"%NSSM_DIR%\nssm.exe" set AIAgentApi AppRotateSeconds 86400

echo.
echo [5/5] Starting services...
if not exist "%PROJECT_ROOT%\logs" mkdir "%PROJECT_ROOT%\logs"
net start AIAgentWeb
net start AIAgentApi

echo.
echo ==========================================
echo Installation Complete!
echo ==========================================
echo.
echo Services installed:
echo   - AIAgentWeb (port 5080)
echo   - AIAgentApi (port 5078)
echo.
echo Both services are set to start automatically.
echo They will restart automatically if they crash.
echo.
echo Log files:
echo   - %PROJECT_ROOT%\logs\web-service.log
echo   - %PROJECT_ROOT%\logs\api-service.log
echo.
pause
