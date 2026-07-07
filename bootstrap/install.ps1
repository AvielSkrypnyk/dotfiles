$Repo = "https://github.com/AvielSkrypnyk/dotfiles.git"
$Dotfiles = "$HOME\dotfiles"

# ------------------------------------
# Shared helpers
# ------------------------------------

# Use the local helpers when cloned; otherwise fetch them over the network
# so the piped "irm | iex" install keeps working.
$Helpers = if ($PSScriptRoot) { Join-Path $PSScriptRoot "lib\helpers.ps1" }

if ($Helpers -and (Test-Path $Helpers)) {
    . $Helpers
}
else {
    Invoke-Expression (
        Invoke-RestMethod "https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/lib/helpers.ps1"
    )
}

Show-Banner "Windows dotfiles bootstrap"

Show-Loading "Installing packages..."

$Packages = @(
    "Git.Git",
    "Starship.Starship",
    "Neovim.Neovim",
    "LGUG2Z.komorebi",
    "LGUG2Z.whkd",
    "Microsoft.WindowsTerminal",
    "Microsoft.PowerShell"
)

foreach ($Package in $Packages) {
    Invoke-WithSpinner "Installing $Package" {
        param($Id)
        winget install `
            --id $Id `
            --silent `
            --accept-package-agreements `
            --accept-source-agreements
    } -ArgumentList $Package
}

# ------------------------------------
# Clone dotfiles
# ------------------------------------

if (!(Test-Path $Dotfiles)) {
    git clone $Repo $Dotfiles
}

# ------------------------------------
# Link helper
# ------------------------------------

function New-DotfileLink {
    param(
        [string]$Source,
        [string]$Target
    )

    $Parent = Split-Path $Target -Parent

    if (!(Test-Path $Parent)) {
        New-Item `
            -ItemType Directory `
            -Force `
            -Path $Parent | Out-Null
    }

    if (Test-Path $Target) {
        Remove-Item `
            -Path $Target `
            -Force `
            -Recurse
    }

    try {

        # Link the config into place: folders use a junction, files a hard link
        # (a plain symlink would need admin or developer mode on Windows)
        if ((Get-Item $Source).PSIsContainer) {

            New-Item `
                -ItemType Junction `
                -Path $Target `
                -Target $Source `
                -ErrorAction Stop | Out-Null

            Write-Host "[OK] Junction $Target" -ForegroundColor Green
        }
        else {

            New-Item `
                -ItemType HardLink `
                -Path $Target `
                -Target $Source `
                -ErrorAction Stop | Out-Null

            Write-Host "[OK] Link $Target" -ForegroundColor Green
        }

    }
    catch {

        Write-Host "[FAIL] $Target" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor DarkRed

    }
}

# ------------------------------------
# Create ~/.config
# ------------------------------------

New-Item `
    -ItemType Directory `
    -Force `
    -Path "$HOME\.config" | Out-Null

# ------------------------------------
# Common configs
# ------------------------------------

$CommonConfig = Join-Path $Dotfiles "common\.config"

if (Test-Path $CommonConfig) {

    Get-ChildItem $CommonConfig | ForEach-Object {

        New-DotfileLink `
            -Source $_.FullName `
            -Target "$HOME\.config\$($_.Name)"
    }
}

# ------------------------------------
# Windows configs
# ------------------------------------

$WindowsConfig = Join-Path $Dotfiles "windows\.config"

if (Test-Path $WindowsConfig) {

    Get-ChildItem $WindowsConfig | ForEach-Object {

        New-DotfileLink `
            -Source $_.FullName `
            -Target "$HOME\.config\$($_.Name)"
    }
}

# ------------------------------------
# PowerShell Profile
# ------------------------------------

if (!(Test-Path $PROFILE)) {

    $ProfileDir = Split-Path $PROFILE -Parent

    if (!(Test-Path $ProfileDir)) {

        New-Item `
            -ItemType Directory `
            -Force `
            -Path $ProfileDir | Out-Null
    }

    New-Item `
        -ItemType File `
        -Force `
        -Path $PROFILE | Out-Null
}

$ProfileText = @"

Invoke-Expression (& starship init powershell)

"@

$ProfileContent = Get-Content `
    $PROFILE `
    -Raw `
    -ErrorAction SilentlyContinue

if ($ProfileContent -notmatch "starship init powershell") {

    Add-Content `
        -Path $PROFILE `
        -Value $ProfileText
}

# ------------------------------------
# Windows Terminal Theme
# ------------------------------------

$SettingsPath =
"$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json"

if (Test-Path $SettingsPath) {

    try {

        # Merge our theme into the existing settings instead of overwriting them
        $Settings =
            Get-Content $SettingsPath -Raw |
            ConvertFrom-Json

        if ($null -eq $Settings.schemes) {
            $Settings.schemes = @()
        }

        $CatppuccinExists =
            $Settings.schemes |
            Where-Object {
                $_.name -eq "Catppuccin Macchiato"
            }

        if (-not $CatppuccinExists) {

            $CatppuccinScheme = [PSCustomObject]@{
                name                = "Catppuccin Macchiato"

                background          = "#24273A"
                foreground          = "#CAD3F5"

                black               = "#494D64"
                red                 = "#ED8796"
                green               = "#A6DA95"
                yellow              = "#EED49F"
                blue                = "#8AADF4"
                purple              = "#C6A0F6"
                cyan                = "#8BD5CA"
                white               = "#B8C0E0"

                brightBlack         = "#5B6078"
                brightRed           = "#ED8796"
                brightGreen         = "#A6DA95"
                brightYellow        = "#EED49F"
                brightBlue          = "#8AADF4"
                brightPurple        = "#C6A0F6"
                brightCyan          = "#8BD5CA"
                brightWhite         = "#A5ADCB"

                cursorColor         = "#F4DBD6"
                selectionBackground = "#5B6078"
            }

            $Settings.schemes += $CatppuccinScheme
        }

        if (-not $Settings.profiles.defaults) {
            $Settings.profiles | Add-Member `
                -Name defaults `
                -MemberType NoteProperty `
                -Value ([PSCustomObject]@{})
        }

        $Settings.profiles.defaults |
            Add-Member `
            -MemberType NoteProperty `
            -Name colorScheme `
            -Value "Catppuccin Macchiato" `
            -Force

        $Settings.profiles.defaults |
            Add-Member `
            -MemberType NoteProperty `
            -Name font `
            -Value ([PSCustomObject]@{
                face = "Hack Nerd Font Mono"
            }) `
            -Force

        # -Depth 100 keeps the whole settings tree intact when saving back
        $Settings |
            ConvertTo-Json -Depth 100 |
            Set-Content $SettingsPath

        Write-Host "[OK] Windows Terminal configured" -ForegroundColor Green

    }
    catch {

        Write-Host "[WARN] Failed to configure Windows Terminal" -ForegroundColor Yellow
        Write-Host $_.Exception.Message -ForegroundColor DarkYellow

    }
}

# ------------------------------------
# Komorebi Autostart
# ------------------------------------

try {

    $Startup =
    "$HOME\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"

    # WScript.Shell (Windows Script Host COM) is used to create a .lnk shortcut.
    # This ensures Komorebi launches automatically at login
    $Shell = New-Object -ComObject WScript.Shell

    $Shortcut =
        $Shell.CreateShortcut("$Startup\Komorebi.lnk")

    $Shortcut.TargetPath = "conhost.exe"

    # Launch hidden via conhost --headless so no console window shows
    $Shortcut.Arguments =
        "--headless pwsh.exe -ExecutionPolicy Bypass -File `"$HOME\dotfiles\windows\scripts\komorebi\start-komorebi.ps1`""

    $Shortcut.WorkingDirectory =
        "$HOME\dotfiles"

    # 7 = minimized, a fallback so the window stays hidden if conhost doesn't
    $Shortcut.WindowStyle = 7

    $Shortcut.Save()

    Write-Host "[OK] Komorebi autostart configured" -ForegroundColor Green
}
catch {

    Write-Host "[WARN] Failed to configure Komorebi autostart" `
        -ForegroundColor Yellow

    Write-Host "Manually create a shortcut in:" `
        -ForegroundColor Cyan

    Write-Host "$HOME\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"
}

# ------------------------------------
# whkd Autostart
# ------------------------------------

try {

    $Startup =
    "$HOME\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"

    $WhkdPath =
        (Get-Command whkd.exe -ErrorAction SilentlyContinue).Source

    # Fall back to the bare name (resolved via PATH) if lookup failed
    if (-not $WhkdPath) {
        $WhkdPath = "whkd.exe"
    }

    $Shell = New-Object -ComObject WScript.Shell

    $Shortcut =
        $Shell.CreateShortcut("$Startup\whkd.lnk")

    # Launch hidden via conhost --headless so no console window shows
    $Shortcut.TargetPath = "conhost.exe"

    $Shortcut.Arguments =
        "--headless `"$WhkdPath`""

    $Shortcut.WorkingDirectory =
        "$HOME\dotfiles"

    # 7 = minimized, a fallback so the window stays hidden if conhost doesn't
    $Shortcut.WindowStyle = 7

    $Shortcut.Save()

    Write-Host "[OK] whkd autostart configured" -ForegroundColor Green
}
catch {

    Write-Host "[WARN] Failed to configure whkd autostart" `
        -ForegroundColor Yellow

    Write-Host "Manually create a shortcut in:" `
        -ForegroundColor Cyan

    Write-Host "$HOME\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup"
}

# ------------------------------------
# Git Configuration
# ------------------------------------

if (Test-Path "$HOME\.gitconfig") {

    try {
        git config --global user.name 2>$null | Out-Null
    }
    catch {

        Write-Host "[WARN] Invalid .gitconfig detected, removing it..." `
            -ForegroundColor Yellow

        Remove-Item "$HOME\.gitconfig" -Force
    }
}

$GitName = git config --global user.name 2>$null
$GitEmail = git config --global user.email 2>$null

if ([string]::IsNullOrWhiteSpace($GitName) -or
    [string]::IsNullOrWhiteSpace($GitEmail)) {

    Write-Host ""
    Write-Host "Git configuration" -ForegroundColor Cyan

    $GitName = Read-Host "Git user name"
    $GitEmail = Read-Host "Git email"

    git config --global user.name "$GitName"
    git config --global user.email "$GitEmail"
    git config --global init.defaultBranch main

    Write-Host "[OK] Git configured" -ForegroundColor Green
}
else {
    Write-Host "[OK] Git already configured" -ForegroundColor Green
}

# ------------------------------------
# Done
# ------------------------------------

Show-Done "Aviel's Dots are ready."
Write-Host "$ColorBlue         Restart Windows Terminal and PowerShell.$ColorReset"
Write-Host ""