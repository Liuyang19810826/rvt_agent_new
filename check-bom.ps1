# 检测项目文件中的 BOM
$files = Get-ChildItem -Path "src" -Recurse -Include "*.csproj", "*.razor", "*.json", "*.cs"

foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        Write-Host "发现 BOM: $($file.FullName)" -ForegroundColor Red
    }
}

Write-Host "检查完成" -ForegroundColor Green
