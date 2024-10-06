
param(
    [ValidateSet("CurrentUser", "Machine")]
    $Scope = "Machine",

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