# PowerShell script for automatic startup
# This script will run specified commands or programs automatically when the system starts.

# Startup programs per mood, read from apps.json next to this script
$Apps = Get-Content "$PSScriptRoot\apps.json" -Raw | ConvertFrom-Json

function Choose-Mood {
    while ($true) {
        $mood = Read-Host "Choose your mood (1: Basic, 2: Semi-Code, 3: Fully-Code)"
        switch ($mood) {
            "1" { return $Apps.BasicApps }
            "2" { return $Apps.SemiCodeApps }
            "3" { return $Apps.FullyCodeApps }
            default { Write-Host "Invalid choice '$mood'. Enter 1, 2 or 3." -ForegroundColor Yellow}
        }
    }
}

function Start-Programs {
    param($Programs)
    # Each program has a name (e.g. Outlook) and one or more paths
    foreach ($program in $Programs.PSObject.Properties) {
        $name = $program.Name
        $path = $null

        # Use the first path that exists on this laptop
        foreach ($option in $program.Value) {
            # Turns %LOCALAPPDATA% into your real folder, e.g. C:\Users\you\AppData\Local
            $option = [Environment]::ExpandEnvironmentVariables($option)
            # * matches any text, e.g. WebStorm* finds "WebStorm 2025.3.2"
            # When more versions match, the last one is the newest
            $found = Get-Item $option -ErrorAction SilentlyContinue | Select-Object -Last 1
            if ($found) {
                $path = $found.FullName
                break
            }
        }

        if (-not $path) {
            Write-Host "Warning: $name not found, skipping." -ForegroundColor Yellow
            continue
        }
        try {
            Start-Process $path -ErrorAction Stop
            Write-Host "Started $name" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to start ${name}: $_" -ForegroundColor Red
        }
    }
}

function Exit-Countdown {
    Write-Host "Exiting powershell in 5 seconds..."
    Start-Sleep -Seconds 1

    # -ge is greater than or equal to.
    # For example, 5 -ge 3 returns True, and 3 -ge 3 also returns True.
    for ($i = 4; $i -ge 1; $i--) {
        Write-Host "$i..." -ForegroundColor Cyan
        Start-Sleep -Seconds 1
    }
    Write-Host "Exiting now..." -ForegroundColor Cyan
    Start-Sleep -Seconds 1
    exit
}

# Terminal -> Choose -> Start (skips missing apps) -> Done (closing the terminal)

function Main {
    $programs = Choose-Mood
    Start-Programs -Programs $Apps.AlwaysApps
    Start-Programs -Programs $programs
    Write-Host "Done starting programs." -ForegroundColor Green
    Exit-Countdown
}

Main