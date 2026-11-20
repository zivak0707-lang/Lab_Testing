# Збирає один файл Lab_Testing.exe для поширення (не потребує встановленого .NET) у теку publish\
# і рахує його контрольну суму SHA-256 (для сторінки Release).
# Запуск: powershell -ExecutionPolicy Bypass -File publish.ps1

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$output = Join-Path $PSScriptRoot 'publish'
if (Test-Path $output) { Remove-Item $output -Recurse -Force }

dotnet publish Lab_Testing\Lab_Testing.csproj `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -o $output

if ($LASTEXITCODE -ne 0) { throw 'Публікація не вдалася' }

$exe = Join-Path $output 'Lab_Testing.exe'
$hash = (Get-FileHash $exe -Algorithm SHA256).Hash
$size = [math]::Round((Get-Item $exe).Length / 1MB, 1)

Set-Content -Path (Join-Path $output 'Lab_Testing.exe.sha256') -Value "$hash  Lab_Testing.exe" -Encoding ascii
Write-Host "Готово: $exe ($size МБ)"
Write-Host "SHA-256: $hash"
