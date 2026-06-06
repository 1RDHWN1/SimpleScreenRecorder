using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SimpleScreenRecorder;

public class ScreenRecorder
{
    private Thread? _captureThread;
    private bool _isRecording = false;
    private bool _isPaused = false;
    private Rectangle _recordingRegion;
    private string _outputPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Videos),
        "SimpleScreenRecorder");
    
    // Settings
    public int FrameRate { get; set; } = 30;
    public string Codec { get; set; } = "libx264";
    public int Quality { get; set; } = 85;
    public bool RecordAudio { get; set; } = true;
    public bool RecordCursor { get; set; } = true;

    public event EventHandler<RecordingEventArgs>? RecordingStarted;
    public event EventHandler<RecordingEventArgs>? RecordingStopped;
    public event EventHandler<ErrorEventArgs>? RecordingError;

    public ScreenRecorder()
    {
        // Get primary screen size
        _recordingRegion = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
        
        // Create output directory
        if (!Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);
    }

    public void SetRegion(Rectangle region)
    {
        _recordingRegion = region;
    }

    public bool StartRecording()
    {
        if (_isRecording) return false;

        _isRecording = true;
        _isPaused = false;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string outputFile = Path.Combine(_outputPath, $"recording_{timestamp}.mp4");

        _captureThread = new Thread(() => CaptureLoop(outputFile))
        {
            IsBackground = true
        };
        _captureThread.Start();

        RecordingStarted?.Invoke(this, new RecordingEventArgs { Message = "Recording started" });
        return true;
    }

    public void StopRecording()
    {
        if (!_isRecording) return;

        _isRecording = false;
        _captureThread?.Join();

        RecordingStopped?.Invoke(this, new RecordingEventArgs { Message = "Recording stopped" });
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Resume()
    {
        _isPaused = false;
    }

    private void CaptureLoop(string outputFile)
    {
        int frameDelay = 1000 / FrameRate;
        var ffmpegProcess = StartFFmpeg(outputFile);

        try
        {
            while (_isRecording)
            {
                if (!_isPaused)
                {
                    var screenshot = CaptureScreen();
                    
                    if (RecordCursor)
                        screenshot = DrawCursor(screenshot);

                    // Send to FFmpeg stdin
                    if (ffmpegProcess?.StandardInput?.BaseStream != null)
                    {
                        using var ms = new MemoryStream();
                        screenshot.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        ffmpegProcess.StandardInput.BaseStream.Write(ms.ToArray(), 0, (int)ms.Length);
                        ffmpegProcess.StandardInput.BaseStream.Flush();
                    }

                    screenshot.Dispose();
                }

                Thread.Sleep(frameDelay);
            }

            // Flush and close FFmpeg
            ffmpegProcess?.StandardInput?.Close();
            ffmpegProcess?.WaitForExit();
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    private Process? StartFFmpeg(string outputFile)
    {
        try
        {
            string ffmpegPath = "ffmpeg"; // Assumes ffmpeg in PATH

            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -f bmp_pipe -i - -pix_fmt yuv420p -c:v {Codec} -crf {101 - Quality} -r {FrameRate} \"{outputFile}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            return Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, new ErrorEventArgs(ex));
            return null;
        }
    }

    private Bitmap CaptureScreen()
    {
        var bitmap = new Bitmap(_recordingRegion.Width, _recordingRegion.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(_recordingRegion.Location, Point.Empty, _recordingRegion.Size);
        return bitmap;
    }

    private Bitmap DrawCursor(Bitmap bitmap)
    {
        var g = Graphics.FromImage(bitmap);
        var cursorPos = Cursor.Position;
        
        // Adjust cursor position to recording region
        cursorPos.X -= _recordingRegion.X;
        cursorPos.Y -= _recordingRegion.Y;

        // Draw cursor crosshair
        if (bitmap.GetBounds(ref cursorPos).Width > 0)
        {
            using var pen = new Pen(Color.Red, 2);
            int size = 15;
            g.DrawLine(pen, cursorPos.X - size, cursorPos.Y, cursorPos.X + size, cursorPos.Y);
            g.DrawLine(pen, cursorPos.X, cursorPos.Y - size, cursorPos.X, cursorPos.Y + size);
        }

        g.Dispose();
        return bitmap;
    }
}

public class RecordingEventArgs : EventArgs
{
    public string Message { get; set; } = "";
}

// Region selection dialog
public class RegionSelectDialog : Form
{
    public Rectangle SelectedRegion { get; private set; }
    private Point _startPoint;
    private Point _endPoint;
    private bool _isSelecting = false;

    public RegionSelectDialog()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.WindowState = FormWindowState.Maximized;
        this.TopMost = true;
        this.BackColor = Color.Black;
        this.Opacity = 0.3;
        this.Cursor = Cursors.Cross;
        this.DoubleBuffered = true;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _startPoint = e.Location;
        _isSelecting = true;
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isSelecting)
        {
            _endPoint = e.Location;
            this.Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _endPoint = e.Location;
        _isSelecting = false;

        int x = Math.Min(_startPoint.X, _endPoint.X);
        int y = Math.Min(_startPoint.Y, _endPoint.Y);
        int w = Math.Abs(_endPoint.X - _startPoint.X);
        int h = Math.Abs(_endPoint.Y - _startPoint.Y);

        SelectedRegion = new Rectangle(x, y, w, h);

        if (w > 0 && h > 0)
            this.DialogResult = DialogResult.OK;

        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_isSelecting && _startPoint != _endPoint)
        {
            int x = Math.Min(_startPoint.X, _endPoint.X);
            int y = Math.Min(_startPoint.Y, _endPoint.Y);
            int w = Math.Abs(_endPoint.X - _startPoint.X);
            int h = Math.Abs(_endPoint.Y - _startPoint.Y);

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 150, 255)), x, y, w, h);
            e.Graphics.DrawRectangle(new Pen(Color.Cyan, 2), x, y, w, h);

            string sizeText = $"{w}x{h}";
            e.Graphics.DrawString(sizeText, new Font("Arial", 12, FontStyle.Bold), 
                Brushes.White, x + 5, y + 5);
        }

        base.OnPaint(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            this.DialogResult = DialogResult.Cancel;

        base.OnKeyDown(e);
    }
}
