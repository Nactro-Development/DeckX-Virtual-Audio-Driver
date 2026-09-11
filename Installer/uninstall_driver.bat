@echo off
setlocal
cd /d "%~dp0"

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting Administrator privileges...
    powershell -Command "Start-Process cmd -ArgumentList '/k cd /d """"%~dp0"""" && call """"%~dp0uninstall_driver.bat"""" elevated' -Verb RunAs"
    exit /b
)

echo ========================================================
echo     Uninstalling DeckX Virtual Audio Driver...
echo ========================================================

devcon.exe remove ROOT\DeckXVirtualAudio
devcon.exe remove ROOT\VirtualAudioDriver

echo ========================================================
echo     Restarting Windows Audio Endpoint Builder...
echo ========================================================
powershell.exe -Command "Restart-Service -Name AudioEndpointBuilder -Force"

echo ========================================================
echo     Driver Uninstalled Successfully!
echo ========================================================
echo.
pause
exit /b 0
