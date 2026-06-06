using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleScreenRecorder;

public partial class MainForm : Form
{
    private ScreenRecorder? _recorder;
    private bool _isRecording = false;
    private bool _isPaused = false;
    private int _elapsedSeconds = 0;
    private System.Windows.Forms.Timer? _timerUpdate;

    public MainForm()
    {
        InitializeComponent();
        SetupUI();
        _recorder = new ScreenRecorder();
    }

    private void SetupUI()
    {
        this.Text = "Simple Screen Recorder";
        this.Size = new Size(650, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Icon = SystemIcons.Application;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(15),
            AutoScroll = true
        };

        // Status Group
        var statusGroup = CreateStatusPanel();
        mainLayout.Controls.Add(statusGroup, 0, 0);
        mainLayout.SetRowSpan(statusGroup, 1);

        // Control Buttons
        var buttonPanel = CreateButtonPanel();
        mainLayout.Controls.Add(buttonPanel, 0, 1);

        // Region Selection
        var regionPanel = CreateRegionPanel();
        mainLayout.Controls.Add(regionPanel, 0, 2);

        // Video Settings
        var videoPanel = CreateVideoPanel();
        mainLayout.Controls.Add(videoPanel, 0, 3);

        // Audio Settings
        var audioPanel = CreateAudioPanel();
        mainLayout.Controls.Add(audioPanel, 0, 4);

        // Advanced Settings
        var advancedPanel = CreateAdvancedPanel();
        mainLayout.Controls.Add(advancedPanel, 0, 5);

        // Output Panel
        var outputPanel = CreateOutputPanel();
        mainLayout.Controls.Add(outputPanel, 0, 6);

        this.Controls.Add(mainLayout);

        // Timer
        _timerUpdate = new System.Windows.Forms.Timer();
        _timerUpdate.Interval = 1000;
        _timerUpdate.Tick += (s, e) => UpdateTimer();
    }

    private GroupBox CreateStatusPanel()
    {
        var group = new GroupBox { Text = "Recording Status", Dock = DockStyle.Fill, Height = 100 };
        var layout = new VerticalFlowLayout { AutoSize = true };

        var statusLabel = new Label { Text = "Status: Ready", AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold) };
        var timeLabel = new Label { Text = "Time: 00:00:00", AutoSize = true };
        var resolutionLabel = new Label { Text = "Resolution: 1920x1080", AutoSize = true };

        layout.Controls.Add(statusLabel);
        layout.Controls.Add(timeLabel);
        layout.Controls.Add(resolutionLabel);

        group.Controls.Add(layout);
        this.Tag = new { StatusLabel = statusLabel, TimeLabel = timeLabel, ResolutionLabel = resolutionLabel };

        return group;
    }

    private Panel CreateButtonPanel()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 50, AutoSize = true };
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };

        var startBtn = new Button { Text = "Start Recording", Width = 120, Height = 40 };
        var stopBtn = new Button { Text = "Stop", Width = 80, Height = 40, Enabled = false };
        var pauseBtn = new Button { Text = "Pause", Width = 80, Height = 40, Enabled = false };

        startBtn.Click += (s, e) => OnStartRecording();
        stopBtn.Click += (s, e) => OnStopRecording();
        pauseBtn.Click += (s, e) => OnPauseRecording();

        layout.Controls.Add(startBtn);
        layout.Controls.Add(pauseBtn);
        layout.Controls.Add(stopBtn);

        panel.Controls.Add(layout);
        return panel;
    }

    private GroupBox CreateRegionPanel()
    {
        var group = new GroupBox { Text = "Recording Region", Dock = DockStyle.Fill, Height = 80 };
        var layout = new VerticalFlowLayout { AutoSize = true };

        var selectBtn = new Button { Text = "Select Region", Width = 120, Height = 35 };
        var regionLabel = new Label { Text = "Selected: Full Screen", AutoSize = true };

        selectBtn.Click += (s, e) => OnSelectRegion();

        layout.Controls.Add(selectBtn);
        layout.Controls.Add(regionLabel);

        group.Controls.Add(layout);
        return group;
    }

    private GroupBox CreateVideoPanel()
    {
        var group = new GroupBox { Text = "Video Settings", Dock = DockStyle.Fill, Height = 150 };
        var layout = new VerticalFlowLayout { AutoSize = true };

        // FPS
        var fpsLayout = new FlowLayoutPanel { AutoSize = true };
        fpsLayout.Controls.Add(new Label { Text = "FPS:", AutoSize = true, Width = 50 });
        var fpsSpinBox = new NumericUpDown { Value = 30, Minimum = 1, Maximum = 120, Width = 60 };
        fpsLayout.Controls.Add(fpsSpinBox);
        layout.Controls.Add(fpsLayout);

        // Codec
        var codecLayout = new FlowLayoutPanel { AutoSize = true };
        codecLayout.Controls.Add(new Label { Text = "Codec:", AutoSize = true, Width = 50 });
        var codecCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        codecCombo.Items.AddRange("H.264", "H.265", "VP9");
        codecCombo.SelectedIndex = 0;
        codecLayout.Controls.Add(codecCombo);
        layout.Controls.Add(codecLayout);

        // Quality
        var qualityLayout = new FlowLayoutPanel { AutoSize = true };
        qualityLayout.Controls.Add(new Label { Text = "Quality:", AutoSize = true, Width = 50 });
        var qualitySlider = new TrackBar { Minimum = 1, Maximum = 100, Value = 85, Width = 200 };
        var qualityLabel = new Label { Text = "85%", AutoSize = true, Width = 40 };
        qualitySlider.ValueChanged += (s, e) => qualityLabel.Text = $"{qualitySlider.Value}%";
        qualityLayout.Controls.Add(qualitySlider);
        qualityLayout.Controls.Add(qualityLabel);
        layout.Controls.Add(qualityLayout);

        // Checkboxes
        var recordCursorCheck = new CheckBox { Text = "Record Cursor", Checked = true, AutoSize = true };
        layout.Controls.Add(recordCursorCheck);

        group.Controls.Add(layout);
        return group;
    }

    private GroupBox CreateAudioPanel()
    {
        var group = new GroupBox { Text = "Audio Settings", Dock = DockStyle.Fill, Height = 80 };
        var layout = new VerticalFlowLayout { AutoSize = true };

        var recordAudioCheck = new CheckBox { Text = "Record Audio", Checked = true, AutoSize = true };
        
        var deviceLayout = new FlowLayoutPanel { AutoSize = true };
        deviceLayout.Controls.Add(new Label { Text = "Device:", AutoSize = true, Width = 50 });
        var deviceCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        deviceCombo.Items.AddRange("Default", "Microphone", "System Audio");
        deviceCombo.SelectedIndex = 0;
        deviceLayout.Controls.Add(deviceCombo);

        layout.Controls.Add(recordAudioCheck);
        layout.Controls.Add(deviceLayout);

        group.Controls.Add(layout);
        return group;
    }

    private GroupBox CreateAdvancedPanel()
    {
        var group = new GroupBox { Text = "Advanced Options", Dock = DockStyle.Fill, Height = 80 };
        var layout = new VerticalFlowLayout { AutoSize = true };

        var watermarkCheck = new CheckBox { Text = "Enable Watermark", AutoSize = true };
        var hwAccelCheck = new CheckBox { Text = "Hardware Acceleration", Checked = true, AutoSize = true };

        layout.Controls.Add(watermarkCheck);
        layout.Controls.Add(hwAccelCheck);

        group.Controls.Add(layout);
        return group;
    }

    private Panel CreateOutputPanel()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 50, AutoSize = true };
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };

        var settingsBtn = new Button { Text = "Output Settings", Width = 120, Height = 35 };
        var openBtn = new Button { Text = "Open Folder", Width = 100, Height = 35 };

        layout.Controls.Add(settingsBtn);
        layout.Controls.Add(openBtn);

        panel.Controls.Add(layout);
        return panel;
    }

    private void OnStartRecording()
    {
        if (_recorder == null) return;

        _isRecording = true;
        _isPaused = false;
        _elapsedSeconds = 0;
        _timerUpdate?.Start();

        if (_recorder.StartRecording())
        {
            MessageBox.Show("Recording started!");
        }
        else
        {
            MessageBox.Show("Failed to start recording!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _isRecording = false;
        }
    }

    private void OnStopRecording()
    {
        if (_recorder == null || !_isRecording) return;

        _timerUpdate?.Stop();
        _recorder.StopRecording();
        _isRecording = false;
        _isPaused = false;

        MessageBox.Show("Recording saved!");
    }

    private void OnPauseRecording()
    {
        if (_recorder == null || !_isRecording) return;

        if (_isPaused)
        {
            _recorder.Resume();
            _isPaused = false;
        }
        else
        {
            _recorder.Pause();
            _isPaused = true;
        }
    }

    private void OnSelectRegion()
    {
        var dialog = new RegionSelectDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (_recorder != null)
            {
                _recorder.SetRegion(dialog.SelectedRegion);
            }
        }
    }

    private void UpdateTimer()
    {
        _elapsedSeconds++;
        int hours = _elapsedSeconds / 3600;
        int minutes = (_elapsedSeconds % 3600) / 60;
        int seconds = _elapsedSeconds % 60;

        // Update time label (implement as needed)
    }

    private void InitializeComponent()
    {
        // Auto-generated
    }
}

// Simple vertical flow layout
public class VerticalFlowLayout : FlowLayoutPanel
{
    public VerticalFlowLayout()
    {
        this.FlowDirection = FlowDirection.TopDown;
        this.WrapContents = false;
        this.Dock = DockStyle.Fill;
        this.AutoSize = true;
    }
}
