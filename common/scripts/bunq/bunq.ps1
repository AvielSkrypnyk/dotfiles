param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$CliArgs
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$binaryPath = Join-Path $scriptRoot "dist\win-x64\bunq.exe"
$projectPath = Join-Path $scriptRoot "bunq-suite\Bunq.Cli\Bunq.Cli.csproj"

if (Test-Path $binaryPath) {
    & $binaryPath @CliArgs
    exit $LASTEXITCODE
}

dotnet run --project $projectPath -- @CliArgs
exit $LASTEXITCODE
