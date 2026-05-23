# AI Agent Services Monitor
# 监控并自动重启 5080 (Web) 和 5078 (API) 端口服务

param(
    [switch]$Install,
    [switch]$Uninstall,
    [switch]$Status
)

$ServiceName = "AIAgentServices"
$ProjectRoot = "C:\Users\caac2\Documents\trae_projects\Rvt_Agent_New"
$LogFile = "$ProjectRoot\services-monitor.log"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $Message" | Out-File -Append -FilePath $LogFile
    Write-Host "$timestamp - $Message"
}

function Test-PortListening {
    param([int]$Port)
    $connection = netstat -ano | findstr ":$Port"
    return $connection -match "LISTENING"
}

function Start-WebService {
    Write-Log "Starting Web Service (port 5080)..."
    $proc = Start-Process -FilePath "dotnet" `
        -ArgumentList "run --project src\AIAgent.Web\AIAgent.Web.csproj --urls `"http://localhost:5080`"" `
        -WorkingDirectory $ProjectRoot `
        -WindowStyle Hidden `
        -PassThru
    Start-Sleep -Seconds 5
    return $proc
}

function Start-ApiService {
    Write-Log "Starting API Service (port 5078)..."
    $proc = Start-Process -FilePath "dotnet" `
        -ArgumentList "run --project src\AIAgent.Api\AIAgent.Api.csproj --urls `"http://localhost:5078`"" `
        -WorkingDirectory $ProjectRoot `
        -WindowStyle Hidden `
        -PassThru
    Start-Sleep -Seconds 5
    return $proc
}

function Stop-Services {
    Write-Log "Stopping all AI Agent services..."
    
    # Kill processes on port 5080
    $webProc = netstat -ano | findstr ":5080" | findstr "LISTENING"
    if ($webProc) {
        $webPid = ($webProc -split '\s+')[-1]
        if ($webPid -match '^\d+$') {
            Stop-Process -Id $webPid -Force -ErrorAction SilentlyContinue
            Write-Log "Stopped Web Service (PID: $webPid)"
        }
    }
    
    # Kill processes on port 5078
    $apiProc = netstat -ano | findstr ":5078" | findstr "LISTENING"
    if ($apiProc) {
        $apiPid = ($apiProc -split '\s+')[-1]
        if ($apiPid -match '^\d+$') {
            Stop-Process -Id $apiPid -Force -ErrorAction SilentlyContinue
            Write-Log "Stopped API Service (PID: $apiPid)"
        }
    }
}

function Get-ServiceStatus {
    $webRunning = Test-PortListening -Port 5080
    $apiRunning = Test-PortListening -Port 5078
    
    Write-Host "`nAI Agent Services Status:"
    Write-Host "========================="
    Write-Host "Web Service (5080): $(if ($webRunning) { 'RUNNING' } else { 'STOPPED' })" -ForegroundColor $(if ($webRunning) { 'Green' } else { 'Red' })
    Write-Host "API Service (5078): $(if ($apiRunning) { 'RUNNING' } else { 'STOPPED' })" -ForegroundColor $(if ($apiRunning) { 'Green' } else { 'Red' })
    Write-Host ""
    
    return $webRunning -and $apiRunning
}

function Install-MonitorService {
    Write-Log "Installing scheduled task for service monitoring..."
    
    # 使用 -WindowStyle Hidden 实现后台静默执行
    $Action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-WindowStyle Hidden -ExecutionPolicy Bypass -File `"$ProjectRoot\services-monitor.ps1`" -Monitor"
    $Trigger = New-ScheduledTaskTrigger -AtStartup
    # 修改检查间隔为120秒（2分钟）
    $Trigger2 = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Seconds 120) -RepetitionDuration (New-TimeSpan -Days 365)
    # 添加 -Hidden 属性确保任务在后台运行
    $Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable -Hidden
    $Principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive
    
    try {
        Register-ScheduledTask -TaskName $ServiceName -Action $Action -Trigger $Trigger,$Trigger2 -Settings $Settings -Principal $Principal -Force
        Write-Log "Scheduled task '$ServiceName' installed successfully"
        Write-Host "Service monitor installed. It will start automatically at boot and check every 120 seconds in background." -ForegroundColor Green
    }
    catch {
        Write-Log "Failed to install scheduled task: $_"
        Write-Host "Failed to install: $_" -ForegroundColor Red
    }
}

function Uninstall-MonitorService {
    Write-Log "Uninstalling scheduled task..."
    try {
        Unregister-ScheduledTask -TaskName $ServiceName -Confirm:$false -ErrorAction SilentlyContinue
        Write-Log "Scheduled task '$ServiceName' uninstalled"
        Write-Host "Service monitor uninstalled." -ForegroundColor Green
    }
    catch {
        Write-Log "Failed to uninstall: $_"
    }
}

# Main execution
if ($Status) {
    Get-ServiceStatus
    exit
}

if ($Uninstall) {
    Uninstall-MonitorService
    Stop-Services
    exit
}

if ($Install) {
    Stop-Services
    Start-Sleep -Seconds 2
    Start-WebService
    Start-ApiService
    Install-MonitorService
    exit
}

# Monitor mode (default)
Write-Log "Service monitor check started"

$webRunning = Test-PortListening -Port 5080
$apiRunning = Test-PortListening -Port 5078

if (-not $webRunning) {
    Write-Log "Web Service (5080) is not running. Restarting..."
    Start-WebService
}

if (-not $apiRunning) {
    Write-Log "API Service (5078) is not running. Restarting..."
    Start-ApiService
}

if ($webRunning -and $apiRunning) {
    Write-Log "All services are running normally"
}

Write-Log "Service monitor check completed"
