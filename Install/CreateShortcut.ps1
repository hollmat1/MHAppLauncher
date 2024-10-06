
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
    $ShortcutPath="$([Environment]::GetFolderPath("CommonStartMenu"))\Programs\$FolderName"
    if(-not (Test-Path $ShortcutPath)) {
        [void](mkdir $ShortcutPath)
    }
}
else {
    $InstallPath="$($env:USERPROFILE)\AppData\Local\Microsoft\WindowsApps\$FolderName\"
    $ShortcutPath=[Environment]::GetFolderPath("Desktop")
}

if(-not [string]::isnullorempty($WrapperCmd)) {
    $Target="$($InstallPath)$WrapperCmd"
    $Arguments="-c $($InstallPath)$AppLauncherCmd"
}
else {
   $Target="$($InstallPath)$AppLauncherCmd"
}

Write-Host "Creating Shortcut in $ShortcutPath to $InstallPath (Scope = $Scope)"

$ws = New-Object -ComObject WScript.Shell; 
$s = $ws.CreateShortcut("$ShortcutPath\$ShortcutName")
$s.TargetPath = $Target 
$s.Arguments = $Arguments
$s.Save()

