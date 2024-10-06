
param(
    [ValidateSet("CurrentUser", "Machine")]
    $Scope = "Machine",

    $SourcePath = "$PSScriptRoot\..\bin\x64\Release"
)

$FolderName="AppLauncher"
$ShortcutName="App Launcher.lnk"

if($Scope -eq "Machine") {
    $InstallPath="C:\Program Files\$FolderName\"
}
else {
    $InstallPath="$($env:USERPROFILE)\AppData\Local\Microsoft\WindowsApps\$FolderName\"
}

Write-Host "Copying $SourcePath  to $InstallPath (Scope = $Scope)"
copy $SourcePath\*.* $InstallPath -Force -Recurse

$InstallPath = $InstallPath.TrimEnd("\")

if($Scope -eq "Machine") {
    $path = [environment]::GetEnvironmentVariable("path", "Machine")
    if(-not $path.Contains($InstallPath) ) {
        [environment]::SetEnvironmentVariable("path", "$path;$($InstallPath)", "Machine")
    }
}
else {
    $path = [environment]::GetEnvironmentVariable("path", "User")
    if(-not $path.Contains($InstallPath) ) {
        [environment]::SetEnvironmentVariable("path", "$path;$($InstallPath.TrimEnd("\"))", "User")
    }

}
