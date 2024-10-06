

$useWrapper = $false
$InstallPath="C:\Program Files\AppLauncher\"
$ShortcutName="App Launcher.lnk"
if($useWrapper) {
    $Target="$($InstallPath)runas.exe"
    $Arguments="-c $($InstallPath)AppLauncher.exe"
}
else {
   $Target="$($InstallPath)AppLauncher.exe"
}

$ws = New-Object -ComObject WScript.Shell; 
$desktopPath = [Environment]::GetFolderPath("Desktop")
$s = $ws.CreateShortcut("$desktopPath\$ShortcutName")
$s.TargetPath = $Target 
$s.Arguments = $Arguments
$s.Save()

