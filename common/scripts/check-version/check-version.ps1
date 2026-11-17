$VersionFile = "$HOME\dotfiles\VERSION"
$RemoteUrl = "https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/VERSION"

$LocalVersion = (Get-Content $VersionFile -Raw).Trim()

try {
    $RemoteVersion = (Invoke-RestMethod $RemoteUrl).Trim()
}
catch {
    "$LocalVersion (offline)"
    exit 0
}

if ($LocalVersion -eq $RemoteVersion) {
    "$LocalVersion ✓"
}
else {
    "$LocalVersion -> $RemoteVersion"
}