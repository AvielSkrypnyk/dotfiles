@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "DIST_BIN=%SCRIPT_DIR%dist\win-x64\bunq.exe"

if exist "%DIST_BIN%" (
  "%DIST_BIN%" %*
  exit /b %ERRORLEVEL%
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%bunq.ps1" %*
exit /b %ERRORLEVEL%
