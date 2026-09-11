using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace DeckX.Audio.Interop;

/// <summary>
/// Routes soundboard audio, microphone input, or media streams directly into the DeckX Virtual Audio Cable.
/// The kernel driver automatically mirrors this playback into DeckX Virtual Microphone for external apps (Discord, OBS, etc.).
/// </summary>
public class DeckXAudioRoutingService : IDisposable
{
    private readonly DeckXAudioDriverService _driverService;
    private WasapiOut? _wasapiOut;
    private float _outputVolume = 1.0f;
    private bool _disposed;

    public DeckXAudioRoutingService(DeckXAudioDriverService? driverService = null)
    {
        _driverService = driverService ?? new DeckXAudioDriverService();
    }

    /// <summary>
    /// Current output volume multiplier (0.0 to 1.0).
    /// </summary>
    public float Volume
    {
        get => _outputVolume;
        set => _outputVolume = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Plays an audio file (.wav, .mp3) directly into the DeckX virtual cable.
    /// External apps configured to listen to DeckX Virtual Microphone will hear this audio instantly.
    /// </summary>
    public async Task PlayFileToVirtualMicAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found", filePath);

        var cableDevice = _driverService.GetAudioCableDevice()
            ?? throw new InvalidOperationException("DeckX Audio Cable device is not active or installed.");

        await Task.Run(() =>
        {
            using var audioFileReader = new AudioFileReader(filePath);
            audioFileReader.Volume = _outputVolume;

            using var wasapiOut = new WasapiOut(cableDevice, AudioClientShareMode.Shared, useEventSync: true, latency: 20);
            wasapiOut.Init(audioFileReader);
            wasapiOut.Play();

            while (wasapiOut.PlaybackState == PlaybackState.Playing && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(50);
            }

            wasapiOut.Stop();
        }, cancellationToken);
    }

    /// <summary>
    /// Streams an in-memory WaveProvider (e.g. live mic feed with effects or mixed soundboard audio) to the virtual cable.
    /// </summary>
    public void StartStreaming(IWaveProvider waveProvider)
    {
        StopStreaming();

        var cableDevice = _driverService.GetAudioCableDevice()
            ?? throw new InvalidOperationException("DeckX Audio Cable device is not active or installed.");

        _wasapiOut = new WasapiOut(cableDevice, AudioClientShareMode.Shared, useEventSync: true, latency: 20);
        _wasapiOut.Init(waveProvider);
        _wasapiOut.Play();
    }

    /// <summary>
    /// Stops active streaming to the virtual cable.
    /// </summary>
    public void StopStreaming()
    {
        if (_wasapiOut != null)
        {
            try
            {
                _wasapiOut.Stop();
                _wasapiOut.Dispose();
            }
            finally
            {
                _wasapiOut = null;
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopStreaming();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
