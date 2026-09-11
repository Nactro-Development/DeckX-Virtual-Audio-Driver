# DeckX Virtual Audio Driver

[![Build and Sign](https://github.com/Nactro-Development/DeckX-Virtual-Audio-Driver/actions/workflows/build-and-sign.yml/badge.svg)](https://github.com/Nactro-Development/DeckX-Virtual-Audio-Driver/actions/workflows/build-and-sign.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A high-performance, low-latency Windows Virtual Audio Driver developed by **Nactro Development** for **DeckX Control Studio**. 

This driver provides virtual audio endpoints (**DeckX Virtual Microphone** and **DeckX Audio Cable**) enabling Windows applications (Discord, OBS Studio, Zoom, games) to capture audio injected from DeckX soundboards, microphone chains, or audio effects without requiring commercial third-party software.

---

## 🎯 Features

- **Low Latency**: Sub-10ms loopback latency between playback (cable) and capture (mic) pins.
- **Modern Windows Support**: Designed for Windows 10 (1903+) and Windows 11 (21H2, 22H2, 23H2, 24H2+).
- **High Audio Fidelity**: Supports 48,000 Hz / 44,100 Hz (16-bit and 24-bit PCM stereo/mono).
- **Branded Endpoints**: Automatically registered in Windows Audio as:
  - `DeckX Virtual Microphone` (Recording / Capture)
  - `DeckX Audio Cable` (Playback / Render)
- **Production Code Signing**: Automated CI pipeline integration with **SignPath Foundation** for genuine Authenticode signing (no "Test Signing" mode watermark or warning needed).
- **C# / WPF Ready**: Includes `.NET 8` Interop service (`DeckX.Audio.Interop`) for easy integration into WPF / WinUI desktop apps.

---

## 📁 Repository Structure

```
DeckX-Virtual-Audio-Driver/
├── .github/workflows/
│   └── build-and-sign.yml        # GitHub Actions CI with SignPath code signing
├── Source/                       # C/C++ WDK Kernel-Mode Driver Source
│   ├── Main/                     # Adapter, WDF setup, and INF template
│   ├── Filters/                  # Mic Array & Speaker topology filters
│   └── Utilities/                # WaveRT circular buffer & tone generation
├── Interop/
│   └── DeckX.Audio.Interop/      # C# / .NET 8 library for WPF client integration
├── Installer/
│   ├── deckx_driver_setup.iss    # Inno Setup script for automated driver install
│   ├── devcon.exe                # Microsoft Device Console (x64)
│   ├── install_driver.bat        # 1-click elevated install script
│   └── uninstall_driver.bat      # 1-click elevated uninstall script
├── LICENSE                       # MIT License
└── README.md
```

---

## 🚀 Building from Source

### Prerequisites
1. **Visual Studio 2022** with:
   - "Desktop development with C++"
   - "MSVC v143 - VS 2022 C++ x64/x86 build tools"
2. **Windows 11 Driver Kit (WDK)** matching your Windows SDK version:
   - [Download the Windows WDK](https://learn.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk)

### Build via Command Line
Run the included build script:
```cmd
build.bat release x64
```
Artifacts will be produced under `Package\x64\Release\package\`.

---

## 🔐 Production Signing via SignPath Foundation

To deploy kernel drivers to standard Windows PCs without requiring users to run `bcdedit /set testsigning on`, binaries must be signed with a trusted Authenticode certificate.

This repository is configured to build and sign via the [SignPath Foundation Open Source Program](https://signpath.io/solutions/open-source-community):

### 1. Requirements for SignPath Approval
- The GitHub repository must be public.
- The project must use an OSI-approved license (`MIT` included).
- Builds must be performed on public GitHub Actions runners from the source repository.

### 2. Configure GitHub Secrets & Variables
Once approved by SignPath, add the following to your GitHub repository settings (**Settings -> Secrets and variables -> Actions**):

| Name | Type | Description |
|---|---|---|
| `SIGNPATH_API_TOKEN` | Secret | Your SignPath organization API token |
| `SIGNPATH_ORGANIZATION_ID` | Variable | Your SignPath Organization ID GUID |
| `SIGNPATH_PROJECT_SLUG` | Variable | The slug of your SignPath project (e.g. `deckx-virtual-audio-driver`) |

When you push a commit or tag to `main`/`master`, the `.github/workflows/build-and-sign.yml` workflow compiles the driver, submits it to SignPath, receives the signed binaries, and packages the signed installer.

---

## 💻 C# / WPF Integration (`DeckX.Audio.Interop`)

Add a project reference to `DeckX.Audio.Interop.csproj` in your WPF app:

### 1. Checking Driver Status
```csharp
using DeckX.Audio.Interop;

var driverService = new DeckXAudioDriverService();

if (!driverService.IsDriverInstalled())
{
    // Guide user to install the driver
    await driverService.InstallDriverAsync(@"Assets\Driver");
}
```

### 2. Injecting Soundboard Audio into the Virtual Mic
```csharp
using DeckX.Audio.Interop;

var audioRouter = new DeckXAudioRoutingService();

// Play soundboard effect directly to the virtual microphone
await audioRouter.PlayFileToVirtualMicAsync(@"C:\Audio\airhorn.mp3");
```

---

## 📦 Installer Packaging (Inno Setup)

To create a standalone setup executable that installs and configures the driver silently on end-user PCs:
1. Open `Installer\deckx_driver_setup.iss` in [Inno Setup 6](https://jrsoftware.org/isdl.php).
2. Click **Build -> Compile**.
3. Output executable will be generated at `Installer\Output\DeckX_Virtual_Audio_Driver_Setup.exe`.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.
Based on the open-source Sysvad/Virtual-Audio-Driver project.

---

## 🛡️ Code Signing Policy

Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org).

### Team Roles & Governance
- **Project Lead & Author**: [Nactro Development](https://github.com/Nactro-Development)
- **Reviewers & Approvers**: Core Maintainers of Nactro Development
- **Build & Release Pipeline**: Fully automated via public GitHub Actions runners from tagged releases.

### 🔒 Privacy & Data Security
DeckX Virtual Audio Driver processes audio exclusively within the local Windows kernel and user-mode memory on the user's computer. It does not collect, record, track, transmit, or store any personal information, telemetry, or audio content to external servers.
