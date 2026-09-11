$ErrorActionPreference = "Continue"

$PKEY_FriendlyName = "{a45c254e-df1c-4efd-8020-67d146a850e0},2"
$PKEY_DeviceDesc   = "{b3f8fa53-0004-438e-9003-51a46e139bfc},6"
$PKEY_IconPath     = "{259abffc-50a7-47ce-af08-68c9a7d73366},12"

function Apply-DynamicBranding {
    param(
        [string]$Type,
        [string]$TargetName,
        [string]$TargetDesc,
        [string]$TargetIcon
    )

    $basePath = "SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\$Type"
    $baseKey = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey($basePath)
    if ($null -eq $baseKey) { return }

    $subKeys = $baseKey.GetSubKeyNames()
    $baseKey.Close()

    foreach ($guid in $subKeys) {
        $propsPath = "$basePath\$guid\Properties"
        try {
            $regKey = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey($propsPath, [Microsoft.Win32.RegistryKeyPermissionCheck]::ReadWriteSubTree, [System.Security.AccessControl.RegistryRights]::QueryValues -bor [System.Security.AccessControl.RegistryRights]::SetValue)
            if ($null -ne $regKey) {
                $name = [string]$regKey.GetValue($PKEY_FriendlyName)
                $desc = [string]$regKey.GetValue($PKEY_DeviceDesc)

                $isTarget = ($name -like "*MTT*" -or $desc -like "*MTT*" -or 
                             $name -like "*Virtual Audio Driver*" -or $desc -like "*Virtual Audio Driver*" -or
                             $name -like "*Virtual Mic Driver*" -or $desc -like "*Virtual Mic Driver*" -or
                             $name -like "*DeckX*")

                if ($isTarget) {
                    Write-Host "[Found $Type] $guid - Currently: '$name' ('$desc')" -ForegroundColor Yellow
                    $regKey.SetValue($PKEY_FriendlyName, $TargetName)
                    $regKey.SetValue($PKEY_DeviceDesc, $TargetDesc)
                    $regKey.SetValue($PKEY_IconPath, $TargetIcon)
                    Write-Host "  -> Successfully branded to '$TargetName' ('$TargetDesc')" -ForegroundColor Green
                }
                $regKey.Close()
            }
        } catch {
            Write-Warning "Skipped $propsPath : $($_.Exception.Message)"
        }
    }
}

Write-Host "Applying DeckX branding to audio endpoints..." -ForegroundColor Cyan
Apply-DynamicBranding -Type "Capture" -TargetName "DeckX Virtual Microphone" -TargetDesc "DeckX Virtual Audio" -TargetIcon "%windir%\system32\mmres.dll,-3014"
Apply-DynamicBranding -Type "Render"  -TargetName "DeckX Audio Cable"        -TargetDesc "DeckX Virtual Audio" -TargetIcon "%windir%\system32\mmres.dll,-3012"

Write-Host "Restarting Windows Audio Endpoint Builder to refresh cache..." -ForegroundColor Cyan
try {
    Restart-Service -Name "AudioEndpointBuilder" -Force -ErrorAction SilentlyContinue
} catch {
    Write-Warning "Could not restart AudioEndpointBuilder automatically."
}

Write-Host "=== DeckX Audio Device Branding Completed Successfully ===" -ForegroundColor Green
