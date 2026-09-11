using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;

namespace DeckX.Audio.Interop;

/// <summary>
/// Service to query, install, and manage the DeckX Virtual Audio Driver from C# / WPF.
/// </summary>
public class DeckXAudioDriverService
{
    public const string HardwareIdDeckX = @"ROOT\DeckXVirtualAudio";
    public const string HardwareIdFallback = @"ROOT\VirtualAudioDriver";
    public const string DefaultCaptureName = "DeckX Virtual Microphone";
    public const string DefaultRenderName = "DeckX Audio Cable";

    /// <summary>
    /// Checks whether the DeckX Virtual Audio driver is currently installed and registered in Windows.
    /// </summary>
    public bool IsDriverInstalled()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            
            // Check capture (microphone)
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            bool hasMic = captureDevices.Any(d => 
                d.FriendlyName.Contains("DeckX", StringComparison.OrdinalIgnoreCase) ||
                d.FriendlyName.Contains("Virtual Mic", StringComparison.OrdinalIgnoreCase));

            // Check render (playback / cable)
            var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            bool hasCable = renderDevices.Any(d => 
                d.FriendlyName.Contains("DeckX", StringComparison.OrdinalIgnoreCase) ||
                d.FriendlyName.Contains("Virtual Audio", StringComparison.OrdinalIgnoreCase));

            return hasMic || hasCable;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Finds the active MMDevice endpoint for DeckX Virtual Microphone (Capture).
    /// </summary>
    public MMDevice? GetVirtualMicrophoneDevice()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            return devices.FirstOrDefault(d => 
                d.FriendlyName.Contains("DeckX Virtual Microphone", StringComparison.OrdinalIgnoreCase))
                ?? devices.FirstOrDefault(d => d.FriendlyName.Contains("Virtual Mic", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Finds the active MMDevice endpoint for DeckX Audio Cable (Render / Playback).
    /// </summary>
    public MMDevice? GetAudioCableDevice()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            return devices.FirstOrDefault(d => 
                d.FriendlyName.Contains("DeckX Audio Cable", StringComparison.OrdinalIgnoreCase))
                ?? devices.FirstOrDefault(d => d.FriendlyName.Contains("Virtual Audio", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Installs the DeckX Virtual Audio Driver using devcon.exe with elevation.
    /// </summary>
    public async Task<bool> InstallDriverAsync(string driverDirectory)
    {
        return await Task.Run(() =>
        {
            string devconPath = Path.Combine(driverDirectory, "devcon.exe");
            string infPath = File.Exists(Path.Combine(driverDirectory, "DeckXVirtualAudio.inf"))
                ? Path.Combine(driverDirectory, "DeckXVirtualAudio.inf")
                : Path.Combine(driverDirectory, "VirtualAudioDriver.inf");

            if (!File.Exists(devconPath) || !File.Exists(infPath))
            {
                throw new FileNotFoundException($"Driver installation files missing in {driverDirectory}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = devconPath,
                Arguments = $"install \"{infPath}\" {HardwareIdDeckX}",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;
            process.WaitForExit(30000);
            return process.ExitCode == 0;
        });
    }

    /// <summary>
    /// Removes the virtual audio device using devcon.exe.
    /// </summary>
    public async Task<bool> UninstallDriverAsync(string devconPath)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(devconPath)) return false;

            var psi = new ProcessStartInfo
            {
                FileName = devconPath,
                Arguments = $"remove {HardwareIdDeckX}",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit(15000);

            // Also clean fallback ID if present
            var psi2 = new ProcessStartInfo
            {
                FileName = devconPath,
                Arguments = $"remove {HardwareIdFallback}",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using var proc2 = Process.Start(psi2);
            proc2?.WaitForExit(15000);

            return true;
        });
    }
}
