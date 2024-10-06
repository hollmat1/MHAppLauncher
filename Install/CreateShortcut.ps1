
param(
    [ValidateSet("CurrentUser", "Machine")]
    $Scope = "Machine",

    [Switch]
    $useWrapper = $true
)


if($Scope -eq "Machine") {
    $InstallPath="C:\Program Files\AppLauncher\"
}
else {
    $InstallPath="$($env:USERPROFILE)\AppData\Local\Microsoft\WindowsApps\"
}
$ShortcutName="App Launcher.lnk"
if($useWrapper) {
    $Target="$($InstallPath)runas.exe"
    $Arguments="-c $($InstallPath)AppLauncher.exe"
}
else {
   $Target="$($InstallPath)AppLauncher.exe"
}

Write-Host "Creating Shortcut to $InstallPath (Scope = $Scope)"

$ws = New-Object -ComObject WScript.Shell; 
$desktopPath = [Environment]::GetFolderPath("Desktop")
$s = $ws.CreateShortcut("$desktopPath\$ShortcutName")
$s.TargetPath = $Target 
$s.Arguments = $Arguments
$s.Save()

