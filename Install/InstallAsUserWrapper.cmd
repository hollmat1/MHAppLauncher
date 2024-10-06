@echo off
SET SCOPE=CurrentUser
IF NOT  "%1"=="" (
	SET SCOPE=%1
)
set PWS=powershell.exe -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile
ECHO Installing to %scope% scope.
call %PWS% -Command %~dp0Install.ps1 -Scope %SCOPE%
call %PWS% -Command %~dp0CreateShortcut.ps1 -Scope %SCOPE% -Wrapper "runas.exe"

