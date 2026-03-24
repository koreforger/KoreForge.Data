[CmdletBinding()]
param()

Push-Location (Resolve-Path "$PSScriptRoot\..")
try {
    dotnet clean KoreForge.Data.slnx --verbosity minimal
    Remove-Item 'out' -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item 'artifacts' -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Clean complete.' -ForegroundColor Green
} finally {
    Pop-Location
}
