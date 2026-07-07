# ------------------------------------
# Shared bootstrap helpers
# Colors, banner and status output.
# Change branding/theme here once.
# ------------------------------------

# Catppuccin Macchiato palette
$ColorPeach = "$([char]27)[38;2;245;169;127m"
$ColorBlue  = "$([char]27)[38;2;138;173;244m"
$ColorGreen = "$([char]27)[38;2;166;218;149m"
$ColorText  = "$([char]27)[38;2;202;211;245m"
$ColorReset = "$([char]27)[0m"

$AuthorName = "Aviel Skrypnyk"

# Directory of this library (banner.txt lives alongside)
$HelpersDir = $PSScriptRoot

function Show-Banner {
    param([string]$Subtitle)

    # No banner file when helpers were fetched over the network; skip the art
    $BannerFile = if ($HelpersDir) { Join-Path $HelpersDir "banner.txt" }

    Clear-Host
    Write-Host ""

    if ($BannerFile -and (Test-Path $BannerFile)) {
        Get-Content $BannerFile | ForEach-Object {
            Write-Host "$ColorPeach$_$ColorReset"
        }
    }

    Write-Host ""
    Write-Host "$ColorBlue         $Subtitle$ColorReset"
    Write-Host "$ColorText            by $AuthorName$ColorReset"
    Write-Host ""
    Write-Host "$ColorPeach  ---------------------------------------------------$ColorReset"
    Write-Host ""
}

function Show-Loading {
    param([string]$Message)

    Write-Host "$ColorPeach::$ColorReset $Message"
}

function Invoke-WithSpinner {
    param(
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][scriptblock]$Action,
        [object[]]$ArgumentList
    )

    $Frames = '|', '/', '-', '\'

    # Run the work in the background so we can animate while it executes
    $Job = Start-Job -ScriptBlock $Action -ArgumentList $ArgumentList
    $Index = 0

    while ($Job.State -eq 'Running') {
        $Frame = $Frames[$Index % $Frames.Length]
        # \r rewrites the same line each tick to animate in place
        Write-Host -NoNewline "`r$ColorPeach $Frame$ColorReset $Message"
        Start-Sleep -Milliseconds 120
        $Index++
    }

    $Ok = $Job.State -eq 'Completed'
    Remove-Job $Job -Force

    if ($Ok) {
        Write-Host "`r$ColorGreen [OK]$ColorReset $Message"
    }
    else {
        Write-Host "`r$ColorPeach [!!]$ColorReset $Message"
    }
}

function Show-Done {
    param([string]$Message)

    Write-Host ""
    Write-Host "$ColorPeach  ---------------------------------------------------$ColorReset"
    Write-Host ""
    Write-Host "$ColorGreen  [DONE]$ColorReset $Message"
    Write-Host ""
}
