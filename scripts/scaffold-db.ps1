<#
.SYNOPSIS
    Scaffolds EF Core contexts and entities from databases defined in config/scaffold-config.json.

.DESCRIPTION
    Reads config/scaffold-config.json and runs 'dotnet ef dbcontext scaffold' for each
    configured database. Generated output is placed in the configured outputDir.
    Existing generated files are removed before scaffolding so stale entities are cleaned.

.PARAMETER ConfigPath
    Path to the scaffold config file. Defaults to config/scaffold-config.json.

.PARAMETER Database
    Optional. Name of a single database to scaffold (must match a name in the config).
    When omitted, all configured databases are scaffolded.
#>
[CmdletBinding()]
param(
    [string]$ConfigPath,
    [string]$Database
)

$ErrorActionPreference = 'Stop'
Push-Location (Resolve-Path "$PSScriptRoot\..")
try {
    # Resolve config path
    if (-not $ConfigPath) { $ConfigPath = 'config/scaffold-config.json' }
    if (-not (Test-Path $ConfigPath)) {
        Write-Error "Config file not found: $ConfigPath"
        return
    }

    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json

    # Restore local tools (ensures dotnet-ef is available)
    dotnet tool restore --verbosity quiet

    foreach ($db in $config.databases) {
        if ($Database -and $db.name -ne $Database) { continue }

        Write-Host "Scaffolding $($db.name)..." -ForegroundColor Cyan

        # Clean existing generated output
        if (Test-Path $db.outputDir) {
            Write-Host "  Cleaning $($db.outputDir)..."
            Remove-Item "$($db.outputDir)\*" -Recurse -Force -ErrorAction SilentlyContinue
        }

        # Build the scaffold command arguments
        $args = @(
            'ef', 'dbcontext', 'scaffold',
            $db.connectionString,
            $db.provider,
            '--project', 'src/KF.Data/KF.Data.csproj',
            '--context', $db.context,
            '--output-dir', $db.outputDir,
            '--force'
        )

        # Add schema filters
        foreach ($schema in $db.schemas) {
            $args += '--schema'
            $args += $schema
        }

        # Add table filters
        foreach ($table in $db.tables) {
            $args += '--table'
            $args += $table
        }

        # Add flags
        if ($db.useDatabaseNames) { $args += '--use-database-names' }
        if ($db.noOnConfiguring)  { $args += '--no-onconfiguring' }

        Write-Host "  Running: dotnet $($args -join ' ')" -ForegroundColor DarkGray
        & dotnet @args

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Scaffold failed for $($db.name) (exit code $LASTEXITCODE)"
        }

        Write-Host "  Done: $($db.name)" -ForegroundColor Green
    }

    Write-Host 'Scaffold complete.' -ForegroundColor Green
} finally {
    Pop-Location
}
