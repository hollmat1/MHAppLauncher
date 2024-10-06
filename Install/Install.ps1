
param(
    [ValidateSet("CurrentUser", "Machine")]
    $Scope = "CurrentUser",

    $SourcePath = "$PSScriptRoot\..\bin\x64\Release"
)

if($Scope -eq "Machine") {
    $InstallPath="C:\Program Files\AppLauncher\"
}
else {
    $InstallPath="$($env:USERPROFILE)\AppData\Local\Microsoft\WindowsApps\"
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
