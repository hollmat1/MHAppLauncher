
param(
    [ValidateSet("CurrentUser", "Machine")]
    $Scope = "Machine",

    [String]
    $WrapperCmd,

    [String]
    $AppLauncherCmd = "AppLauncher.exe"

)

$FolderName="AppLauncher"
$ShortcutName="App Launcher.lnk"

if($Scope -eq "Machine") {
    $InstallPath="C:\Program Files\$FolderName\"
}
else {
    $InstallPath="$($env:USERPROFILE)\AppData\Local\Microsoft\WindowsApps\$FolderName\"
}

if(-not [string]::isnullorempty($WrapperCmd)) {
    $Target="$($InstallPath)$WrapperCmd"
    $Arguments="-c $($InstallPath)$AppLauncherCmd"
}
else {
   $Target="$($InstallPath)$AppLauncherCmd"
}

Write-Host "Creating Shortcut to $InstallPath (Scope = $Scope)"

$ws = New-Object -ComObject WScript.Shell; 
$desktopPath = [Environment]::GetFolderPath("Desktop")
$s = $ws.CreateShortcut("$desktopPath\$ShortcutName")
$s.TargetPath = $Target 
$s.Arguments = $Arguments
$s.Save()

