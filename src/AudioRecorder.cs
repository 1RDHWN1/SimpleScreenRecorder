using NAudio.Wave;
using System;
using System.IO;

namespace SimpleScreenRecorder;

public class AudioRecorder : IDisposable
{
    private IWaveIn? _waveInDevice;
    private WaveFileWriter? _waveWriter;
    private string _outputPath;

    public event EventHandler<EventArgs>? RecordingStarted;
    public event EventHandler<EventArgs>? RecordingStopped;
    public event EventHandler<ErrorEventArgs>? RecordingError;

    public AudioRecorder()
    {
        _outputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Videos),
            "SimpleScreenRecorder");
    }

    public bool StartRecording(string filename)
    {
        try
        {
            // Get default audio input device
            int deviceNumber = 0;

            _waveInDevice = new WaveInEvent
            {
                DeviceNumber = deviceNumber
            };

            string outputFile = Path.Combine(_outputPath, filename);

            _waveWriter = new WaveFileWriter(outputFile, _waveInDevice.WaveFormat);

            _waveInDevice.DataAvailable += (s, e) =>
            {
                if (_waveWriter != null)
                {
                    _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                }
            };

            _waveInDevice.RecordingStopped += (s, e) =>
            {
                _waveWriter?.Dispose();
                _waveInDevice?.Dispose();
                RecordingStopped?.Invoke(this, EventArgs.Empty);
            };

            _waveInDevice.StartRecording();
            RecordingStarted?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, new ErrorEventArgs(ex));
            return false;
        }
    }

    public void StopRecording()
    {
        try
        {
            _waveInDevice?.StopRecording();
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    public void Pause()
    {
        try
        {
            _waveInDevice?.StopRecording();
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    public void Resume()
    {
        try
        {
            _waveInDevice?.StartRecording();
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    public bool IsRecording => _waveInDevice?.RecordingState == RecordingState.Recording;

    public void Dispose()
    {
        StopRecording();
        _waveWriter?.Dispose();
        _waveInDevice?.Dispose();
        GC.SuppressFinalize(this);
    }
}
