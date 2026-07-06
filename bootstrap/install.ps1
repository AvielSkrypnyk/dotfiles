$Repo = "https://github.com/AvielSkrypnyk/dotfiles.git"
$Dotfiles = "$HOME\dotfiles"

Write-Host "Installing packages..." -ForegroundColor Cyan

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
    winget install `
        --id $Package `
        --silent `
        --accept-package-agreements `
        --accept-source-agreements
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

    $Shell = New-Object -ComObject WScript.Shell

    $Shortcut =
        $Shell.CreateShortcut("$Startup\Komorebi.lnk")

    $Shortcut.TargetPath = "pwsh.exe"

    $Shortcut.Arguments =
        "-ExecutionPolicy Bypass -File `"$HOME\dotfiles\windows\scripts\komorebi\start-komorebi.ps1`""

    $Shortcut.WorkingDirectory =
        "$HOME\dotfiles"

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

Write-Host ""
Write-Host "Bootstrap completed." -ForegroundColor Green
Write-Host "Restart Windows Terminal and PowerShell." -ForegroundColor Cyan