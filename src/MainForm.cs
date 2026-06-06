using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace SimpleScreenRecorder;

public class MainForm : Form
{
    private Label statusLabel = new();
    private Label timeLabel = new();
    private Button startBtn = new();
    private Button stopBtn = new();
    private Button pauseBtn = new();
    
    // Recording settings
    private NumericUpDown fpsSpinner = new();
    private TrackBar qualitySlider = new();
    private Label qualityLabel = new();
    private ComboBox codecCombo = new();
    private ComboBox audioDeviceCombo = new();
    private CheckBox recordAudioCheck = new();
    private CheckBox recordCursorCheck = new();
    private CheckBox fullScreenCheck = new();
    private Label regionLabel = new();
    private Button selectRegionBtn = new();
    
    // Watermark settings
    private CheckBox watermarkCheck = new();
    private TextBox watermarkText = new();
    
    // Recording state
    private Timer? recordingTimer;
    private int elapsedSeconds = 0;
    private bool isPaused = false;
    private Process? ffmpegProcess;
    private string outputDir;
    private RecordingSettings settings = new();

    public MainForm()
    {
        outputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            "SimpleScreenRecorder");
        InitializeUI();
        Directory.CreateDirectory(outputDir);
    }

    private void InitializeUI()
    {
        this.Text = "🎥 Simple Screen Recorder - Full Features";
        this.Size = new Size(700, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = Color.WhiteSmoke;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 20,
            Padding = new Padding(15),
            AutoScroll = true
        };
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int row = 0;

        // Status Section
        var statusHeader = new Label
        {
            Text = "Status",
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        mainPanel.Controls.Add(statusHeader, 0, row);
        mainPanel.SetColumnSpan(statusHeader, 2);
        row++;

        statusLabel.Text = "🟢 Ready";
        statusLabel.Font = new Font("Arial", 11, FontStyle.Bold);
        statusLabel.ForeColor = Color.Green;
        mainPanel.Controls.Add(new Label { Text = "Status:", AutoSize = true }, 0, row);
        mainPanel.Controls.Add(statusLabel, 1, row);
        row++;

        timeLabel.Text = "00:00:00";
        mainPanel.Controls.Add(new Label { Text = "Time:", AutoSize = true }, 0, row);
        mainPanel.Controls.Add(timeLabel, 1, row);
        row++;

        // Recording Mode Section
        var modeHeader = new Label
        {
            Text = "Recording Mode",
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        mainPanel.Controls.Add(modeHeader, 0, row);
        mainPanel.SetColumnSpan(modeHeader, 2);
        row++;

        fullScreenCheck.Text = "Full Screen";
        fullScreenCheck.Checked = true;
        fullScreenCheck.CheckedChanged += (s, e) =>
        {
            selectRegionBtn.Enabled = !fullScreenCheck.Checked;
            regionLabel.Text = fullScreenCheck.Checked ? "Full Screen" : "Custom Region";
        };
        mainPanel.Controls.Add(fullScreenCheck, 0, row);
        mainPanel.SetColumnSpan(fullScreenCheck, 2);
        row++;

        mainPanel.Controls.Add(new Label { Text = "Region:", AutoSize = true }, 0, row);
        regionLabel.Text = "Full Screen";
        regionLabel.ForeColor = Color.DarkGreen;
        regionLabel.Font = new Font("Arial", 9, FontStyle.Italic);
        mainPanel.Controls.Add(regionLabel, 1, row);
        row++;

        selectRegionBtn.Text = "Select Area";
        selectRegionBtn.Enabled = false;
        selectRegionBtn.Click += SelectRegionBtn_Click;
        mainPanel.Controls.Add(selectRegionBtn, 1, row);
        row++;

        // Codec Section
        var codecHeader = new Label
        {
            Text = "Video Settings",
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        mainPanel.Controls.Add(codecHeader, 0, row);
        mainPanel.SetColumnSpan(codecHeader, 2);
        row++;

        codecCombo.Items.AddRange("H.264 (Recommended)", "H.265 (Best Compression)", "VP9 (Open Source)");
        codecCombo.SelectedIndex = 0;
        codecCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        mainPanel.Controls.Add(new Label { Text = "Codec:", AutoSize = true }, 0, row);
        mainPanel.Controls.Add(codecCombo, 1, row);
        row++;

        mainPanel.Controls.Add(new Label { Text = "FPS:", AutoSize = true }, 0, row);
        fpsSpinner.Minimum = 1;
        fpsSpinner.Maximum = 120;
        fpsSpinner.Value = 30;
        fpsSpinner.Width = 60;
        mainPanel.Controls.Add(fpsSpinner, 1, row);
        row++;

        qualitySlider.Minimum = 1;
        qualitySlider.Maximum = 100;
        qualitySlider.Value = 85;
        qualitySlider.Width = 200;
        qualityLabel.Text = "85%";
        qualityLabel.Width = 40;
        qualitySlider.ValueChanged += (s, e) => qualityLabel.Text = $"{qualitySlider.Value}%";
        
        mainPanel.Controls.Add(new Label { Text = "Quality:", AutoSize = true }, 0, row);
        var qualityPanel = new Panel { Height = 30 };
        qualityPanel.Controls.Add(qualitySlider);
        qualityPanel.Controls.Add(qualityLabel);
        mainPanel.Controls.Add(qualityPanel, 1, row);
        row++;

        // Audio Section
        var audioHeader = new Label
        {
            Text = "Audio Settings",
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        mainPanel.Controls.Add(audioHeader, 0, row);
        mainPanel.SetColumnSpan(audioHeader, 2);
        row++;

        recordAudioCheck.Text = "Record Audio";
        recordAudioCheck.Checked = true;
        recordAudioCheck.CheckedChanged += (s, e) => audioDeviceCombo.Enabled = recordAudioCheck.Checked;
        mainPanel.Controls.Add(recordAudioCheck, 0, row);
        mainPanel.SetColumnSpan(recordAudioCheck, 2);
        row++;

        audioDeviceCombo.Items.AddRange("Microphone", "Speaker (Stereo Mix)", "System Default");
        audioDeviceCombo.SelectedIndex = 0;
        audioDeviceCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        mainPanel.Controls.Add(new Label { Text = "Device:", AutoSize = true }, 0, row);
        mainPanel.Controls.Add(audioDeviceCombo, 1, row);
        row++;

        // Effects Section
        var effectsHeader = new Label
        {
            Text = "Effects & Options",
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        mainPanel.Controls.Add(effectsHeader, 0, row);
        mainPanel.SetColumnSpan(effectsHeader, 2);
        row++;

        recordCursorCheck.Text = "Record Cursor";
        recordCursorCheck.Checked = true;
        mainPanel.Controls.Add(recordCursorCheck, 0, row);
        mainPanel.SetColumnSpan(recordCursorCheck, 2);
        row++;

        watermarkCheck.Text = "Add Watermark";
        watermarkCheck.CheckedChanged += (s, e) => watermarkText.Enabled = watermarkCheck.Checked;
        mainPanel.Controls.Add(watermarkCheck, 0, row);
        mainPanel.SetColumnSpan(watermarkCheck, 2);
        row++;

        watermarkText.PlaceholderText = "Watermark text (e.g., 'MyCompany.com')";
        watermarkText.Enabled = false;
        watermarkText.Width = 250;
        mainPanel.Controls.Add(new Label { Text = "Text:", AutoSize = true }, 0, row);
        mainPanel.Controls.Add(watermarkText, 1, row);
        row++;

        // Buttons Section
        mainPanel.Controls.Add(new Label { Text = "" }, 0, row);
        row++;

        startBtn.Text = "▶ Start Recording";
        startBtn.Height = 45;
        startBtn.BackColor = Color.LimeGreen;
        startBtn.ForeColor = Color.White;
        startBtn.Font = new Font("Arial", 11, FontStyle.Bold);
        startBtn.Click += StartBtn_Click;
        mainPanel.Controls.Add(startBtn, 0, row);

        pauseBtn.Text = "⏸ Pause";
        pauseBtn.Height = 45;
        pauseBtn.Enabled = false;
        pauseBtn.Click += PauseBtn_Click;
        mainPanel.Controls.Add(pauseBtn, 1, row);
        row++;

        stopBtn.Text = "⏹ Stop Recording";
        stopBtn.Height = 45;
        stopBtn.Enabled = false;
        stopBtn.BackColor = Color.Red;
        stopBtn.ForeColor = Color.White;
        stopBtn.Font = new Font("Arial", 11, FontStyle.Bold);
        stopBtn.Click += StopBtn_Click;
        mainPanel.Controls.Add(stopBtn, 0, row);
        mainPanel.SetColumnSpan(stopBtn, 2);
        row++;

        this.Controls.Add(mainPanel);
    }

    private void SelectRegionBtn_Click(object? sender, EventArgs e)
    {
        var selector = new RegionSelector();
        if (selector.ShowDialog() == DialogResult.OK)
        {
            var region = selector.GetSelectedRegion();
            settings.IsFullScreen = false;
            settings.RegionX = region.X;
            settings.RegionY = region.Y;
            settings.RegionWidth = region.Width;
            settings.RegionHeight = region.Height;
            regionLabel.Text = $"{region.Width}×{region.Height} @ ({region.X},{region.Y})";
            regionLabel.ForeColor = Color.DarkRed;
        }
    }

    private void StartBtn_Click(object? sender, EventArgs e)
    {
        // Update settings from UI
        settings.Codec = codecCombo.SelectedIndex switch
        {
            0 => "h264",
            1 => "h265",
            2 => "vp9",
            _ => "h264"
        };
        settings.Quality = (int)qualitySlider.Value;
        settings.FPS = (int)fpsSpinner.Value;
        settings.RecordAudio = recordAudioCheck.Checked;
        settings.RecordCursor = recordCursorCheck.Checked;
        settings.IsFullScreen = fullScreenCheck.Checked;
        settings.UseWatermark = watermarkCheck.Checked;
        settings.WatermarkText = watermarkText.Text;

        elapsedSeconds = 0;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string outputFile = Path.Combine(outputDir, $"recording_{timestamp}.mp4");

        try
        {
            // Build FFmpeg command
            string ffmpegCmd = BuildFFmpegCommand(outputFile);
            
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegCmd,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            ffmpegProcess = Process.Start(psi);
            
            statusLabel.Text = "🔴 Recording...";
            statusLabel.ForeColor = Color.Red;
            startBtn.Enabled = false;
            stopBtn.Enabled = true;
            pauseBtn.Enabled = true;
            
            // Disable settings during recording
            codecCombo.Enabled = false;
            fpsSpinner.Enabled = false;
            qualitySlider.Enabled = false;
            recordAudioCheck.Enabled = false;
            audioDeviceCombo.Enabled = false;
            recordCursorCheck.Enabled = false;
            watermarkCheck.Enabled = false;
            watermarkText.Enabled = false;
            fullScreenCheck.Enabled = false;
            selectRegionBtn.Enabled = false;

            recordingTimer = new Timer();
            recordingTimer.Interval = 1000;
            recordingTimer.Tick += (s, a) =>
            {
                elapsedSeconds++;
                int h = elapsedSeconds / 3600;
                int m = (elapsedSeconds % 3600) / 60;
                int s_val = elapsedSeconds % 60;
                timeLabel.Text = $"{h:D2}:{m:D2}:{s_val:D2}";
            };
            recordingTimer.Start();

            MessageBox.Show($"Recording started!\n\nSaving to:\n{outputFile}", "Recording Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting FFmpeg:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ResetUI();
        }
    }

    private string BuildFFmpegCommand(string outputFile)
    {
        var cmd = new System.Text.StringBuilder();

        // Input device
        if (settings.IsFullScreen)
        {
            cmd.Append($"-f gdigrab -framerate {settings.FPS} -i desktop");
        }
        else
        {
            cmd.Append($"-f gdigrab -framerate {settings.FPS} ");
            cmd.Append($"-offset_x {settings.RegionX} -offset_y {settings.RegionY} ");
            cmd.Append($"-video_size {settings.RegionWidth}x{settings.RegionHeight} ");
            cmd.Append($"-i desktop");
        }

        // Audio input
        if (settings.RecordAudio)
        {
            cmd.Append(" -f dshow -i audio=\"Microphone\"");
        }

        // Video codec
        cmd.Append($" {settings.GetFFmpegCodecArgs()}");

        // Frame rate
        cmd.Append($" -r {settings.FPS}");

        // Draw cursor
        if (settings.RecordCursor)
        {
            cmd.Append(" -vf \"format=bgra,hwupload\" -draw_mouse 1");
        }

        // Audio codec if recording audio
        if (settings.RecordAudio)
        {
            cmd.Append(" -c:a aac -b:a 128k");
        }

        // Watermark
        if (settings.UseWatermark && !string.IsNullOrEmpty(settings.WatermarkText))
        {
            string watermarkEscaped = settings.WatermarkText.Replace("'", "\\'");
            cmd.Append($" -vf \"drawtext=text='{watermarkEscaped}':fontfile='C\\:/Windows/Fonts/arial.ttf':");
            cmd.Append($"fontsize=24:fontcolor=white@0.7:x=10:y=10\"");
        }

        // Output file
        cmd.Append($" -y \"{outputFile}\"");

        return cmd.ToString();
    }

    private void PauseBtn_Click(object? sender, EventArgs e)
    {
        if (ffmpegProcess == null || ffmpegProcess.HasExited) return;

        if (isPaused)
        {
            try
            {
                ffmpegProcess.StandardInput.WriteLine("");
                isPaused = false;
                pauseBtn.Text = "⏸ Pause";
                statusLabel.Text = "🔴 Recording...";
            }
            catch { }
        }
        else
        {
            try
            {
                // FFmpeg pause via stdin: send 'p' for pause
                ffmpegProcess.StandardInput.WriteLine("p");
                isPaused = true;
                pauseBtn.Text = "▶ Resume";
                statusLabel.Text = "🟡 Paused";
            }
            catch { }
        }
    }

    private void StopBtn_Click(object? sender, EventArgs e)
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
        {
            try
            {
                ffmpegProcess.StandardInput.WriteLine("q");
                ffmpegProcess.WaitForExit(5000);
            }
            catch { }
            finally
            {
                ffmpegProcess?.Dispose();
            }
        }

        recordingTimer?.Stop();
        recordingTimer?.Dispose();

        statusLabel.Text = "🟠 Stopped";
        statusLabel.ForeColor = Color.Orange;
        timeLabel.Text = "00:00:00";
        elapsedSeconds = 0;

        ResetUI();
        MessageBox.Show("Recording stopped!\n\nVideo saved to your Videos folder.", "Recording Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ResetUI()
    {
        startBtn.Enabled = true;
        stopBtn.Enabled = false;
        pauseBtn.Enabled = false;
        isPaused = false;
        pauseBtn.Text = "⏸ Pause";
        
        codecCombo.Enabled = true;
        fpsSpinner.Enabled = true;
        qualitySlider.Enabled = true;
        recordAudioCheck.Enabled = true;
        audioDeviceCombo.Enabled = true;
        recordCursorCheck.Enabled = true;
        watermarkCheck.Enabled = true;
        fullScreenCheck.Enabled = true;
        selectRegionBtn.Enabled = !fullScreenCheck.Checked;
    }
}
