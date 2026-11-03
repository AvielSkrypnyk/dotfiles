# Switch the Starship prompt style (Windows)
# Usage: starship-style [name]

param(
    [string]$Style
)

Set-StrictMode -Version Latest

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

    $item = Get-Item $Target

    if (-not $item.LinkType) {
        return
    }

    Split-Path $item.Target -LeafBase
}

function Choose-Style {
    if ($Style) {
        return
    }

    Show-Styles

    $activeStyle = Get-ActiveStyle

    if ($activeStyle) {
        Write-Host ""
        Write-Host "Current style: $activeStyle"
    }

    Write-Host ""

    # Update the script-level parameter so the other functions see the selection.
    $script:Style = Read-Host "Style (Enter = default)"

    if (-not $script:Style) {
        $script:Style = "default"
    }
}

function Validate-Style {
    $source = Join-Path $StylesDir "$Style.toml"

    if (-not (Test-Path $source)) {
        throw "Unknown style: $Style"
    }
}

function Apply-Style {
    $source = Join-Path $StylesDir "$Style.toml"

    New-Item `
        -ItemType Directory `
        -Path (Split-Path $Target) `
        -Force | Out-Null

    Remove-Item $Target -Force -ErrorAction SilentlyContinue

    # Hard links need no administrator rights and keep edits shared with the style file.
    New-Item `
        -ItemType HardLink `
        -Path $Target `
        -Target $source | Out-Null

    Write-Host "Starship style: $Style"
}

function Main {
    if (-not $Style) {
        Choose-Style
    }

    if (-not $Style) {
        $Style = "default"
    }

    Validate-Style
    Apply-Style
}

Main