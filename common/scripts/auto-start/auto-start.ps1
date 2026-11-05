# PowerShell script for automatic startup
# This script will run specified commands or programs automatically when the system starts.

# Startup programs per profile, read from apps.json next to this script
$Apps = Get-Content "$PSScriptRoot\apps.json" -Raw | ConvertFrom-Json

function Choose-Profile {
    while ($true) {
        $choice = Read-Host "Choose your profile (1: Basic, 2: Semi-Code, 3: Fully-Code)"
        switch ($choice) {
            "1" { return $Apps.BasicApps }
            "2" { return $Apps.SemiCodeApps }
            "3" { return $Apps.FullyCodeApps }
            default { Write-Host "Invalid choice '$choice'. Enter 1, 2 or 3." -ForegroundColor Yellow}
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

function Exit-OnKeyPress {
    Write-Host "Press any key to close..." -ForegroundColor Cyan
    # Waits for one key press, without showing the key
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    # exit only ends this script; this closes the whole PowerShell window
    [Environment]::Exit(0)
}

# Terminal -> Choose -> Start (skips missing apps) -> Done -> key press closes the terminal

function Main {
    $programs = Choose-Profile
    Start-Programs -Programs $Apps.AlwaysApps
    Start-Programs -Programs $programs
    Write-Host "Done starting programs." -ForegroundColor Green
    Exit-OnKeyPress
}

Main