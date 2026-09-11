; Inno Setup Script for DeckX Virtual Audio Driver
; Copyright (c) 2026 Nactro Development

#define MyAppName "DeckX Virtual Audio Driver"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Nactro Development"
#define MyAppURL "https://github.com/Nactro-Development/DeckX-Virtual-Audio-Driver"

[Setup]
AppId={{D8A1E92C-3F55-46B9-A2BE-C677C38B0F21}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Nactro\DeckX Virtual Audio Driver
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=DeckX_Virtual_Audio_Driver_Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "devcon.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "VirtualAudioDriver.sys"; DestDir: "{app}"; Flags: ignoreversion
Source: "virtualaudiodriver.cat"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Source\Main\VirtualAudioDriver.inx"; DestDir: "{app}"; DestName: "DeckXVirtualAudio.inf"; Flags: ignoreversion
Source: "apply_deckx_branding.ps1"; DestDir: "{app}"; Flags: ignoreversion

[Run]
; 1. Stage and install the driver onto root-enumerated devnode
Filename: "{app}\devcon.exe"; Parameters: "install ""{app}\DeckXVirtualAudio.inf"" ROOT\DeckXVirtualAudio"; StatusMsg: "Installing DeckX Virtual Audio Driver..."; Flags: runhidden waituntilterminated
; 2. Apply branding to MMDevice Audio endpoints
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -WindowStyle Hidden -NoProfile -File ""{app}\apply_deckx_branding.ps1"""; StatusMsg: "Configuring audio endpoints..."; Flags: runhidden waituntilterminated

[UninstallRun]
; Cleanly remove root devnodes when uninstalled
Filename: "{app}\devcon.exe"; Parameters: "remove ROOT\DeckXVirtualAudio"; Flags: runhidden waituntilterminated
Filename: "{app}\devcon.exe"; Parameters: "remove ROOT\VirtualAudioDriver"; Flags: runhidden waituntilterminated
