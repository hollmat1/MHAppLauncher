@echo off
set PWS=powershell.exe -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile
call %PWS% -Command %~dp0CreateShortcut.ps1

