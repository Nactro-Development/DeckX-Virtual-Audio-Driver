@echo off
setlocal
cd /d "%~dp0"

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting Administrator privileges...
    powershell -Command "Start-Process cmd -ArgumentList '/k cd /d """"%~dp0"""" && call """"%~dp0install_driver.bat"""" elevated' -Verb RunAs"
    exit /b
)

echo ========================================================
echo     Installing DeckX Virtual Audio Driver...
echo ========================================================

if exist "DeckXVirtualAudio.inf" (
    set INF_NAME=DeckXVirtualAudio.inf
) else (
    set INF_NAME=VirtualAudioDriver.inf
)

devcon.exe install "%INF_NAME%" ROOT\DeckXVirtualAudio

echo ========================================================
echo     Applying DeckX Virtual Microphone Branding...
echo ========================================================

powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%~dp0apply_deckx_branding.ps1"

echo ========================================================
echo     Installation and Branding Complete!
echo ========================================================
echo.
echo Press any key to close this window.
pause >nul
exit /b 0
