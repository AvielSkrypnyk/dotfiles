# Switch the Starship prompt style (Windows)
# Usage: starship-style [name]

param(
    [string]$Style
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$StylesDir = "$HOME\dotfiles\common\.config\starship"
$Target    = "$HOME\.config\starship.toml"

function Show-Styles {
    Write-Host "Available styles:"

    Get-ChildItem $StylesDir -Filter '*.toml' |
        Sort-Object BaseName |
        ForEach-Object {
            Write-Host "  $($_.BaseName)"
        }
}

function Get-ActiveStyle {
    if (-not (Test-Path $Target)) {
        return
    }

    $targetHash = (Get-FileHash -LiteralPath $Target).Hash

    Get-ChildItem $StylesDir -Filter *.toml |
        Where-Object {
            (Get-FileHash -LiteralPath $_.FullName).Hash -eq $targetHash
        } |
        Select-Object -First 1 -ExpandProperty BaseName
}

function Choose-Style {
    if ($Style) {
        return $true
    }

    Show-Styles
    Write-Host ""

    $activeStyle = Get-ActiveStyle

    if ($activeStyle) {
        Write-Host "Current style: $activeStyle"
    }

    Write-Host ""

    $script:Style = Read-Host "Style (Enter = default)"

    if (-not $script:Style) {
        $script:Style = "default"
    }

    return $true
}

function Handle-UnknownStyle {
    if (-not (Test-Path -LiteralPath $Target) -or (Get-ActiveStyle)) {
        return $true
    }

    Write-Host "Current style: unknown" -ForegroundColor Yellow
    Write-Host "[WARN] Current configuration does not match a known style." -ForegroundColor Yellow
    Write-Host "[WARN] Switching styles may overwrite custom changes." -ForegroundColor Yellow
    Write-Host ""

    $answer = Read-Host "Back up and switch styles? (Y/n)"

    if ($answer -notmatch '^[Nn]$') {
        $backup = "$Target.backup"

        if (Test-Path -LiteralPath $backup) {
            $backup = "$Target.backup.$([guid]::NewGuid().ToString('N'))"
        }
        Copy-Item -LiteralPath $Target -Destination $backup

        Write-Host "[OK] Backup created: $backup" `
            -ForegroundColor Green
    }
    else {
        Write-Host "Cancelled; configuration unchanged."
        return $false
    }

    return $true
}

function Validate-Style {
    $source = Join-Path $StylesDir "$Style.toml"

    if (-not (Test-Path $source)) {
        throw "Unknown style: $Style"
    }

    if (Test-Path -LiteralPath $Target -PathType Container) {
        throw "Cannot replace $Target because it is a directory."
    }
}

function Apply-Style {
    $source = Join-Path $StylesDir "$Style.toml"

    New-Item `
        -ItemType Directory `
        -Path (Split-Path $Target) `
        -Force | Out-Null

    if (Test-Path -LiteralPath $Target) {
        Remove-Item -LiteralPath $Target -Force
    }

    # Hard links need no administrator rights and keep edits shared with the style file.
    New-Item `
        -ItemType HardLink `
        -Path $Target `
        -Target $source | Out-Null

    Write-Host "Starship style: $Style"
}

function Main {
    if (-not (Choose-Style)) {
        return
    }

    Validate-Style
    if (-not (Handle-UnknownStyle)) {
        return
    }
    Apply-Style
}

Main