#include "mainwindow.h"
#include "screenrecorder.h"
#include "recordingsettings.h"
#include "regionselectiondialog.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QGroupBox>
#include <QMenuBar>
#include <QMenu>
#include <QAction>
#include <QMessageBox>
#include <QFileDialog>
#include <QDesktopServices>
#include <QUrl>
#include <QTimer>
#include <QScreen>
#include <QGuiApplication>
#include <QDateTime>

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent), recorder(std::make_unique<ScreenRecorder>()),
      settings(std::make_unique<RecordingSettings>()),
      timerUpdate(new QTimer(this)) {
    
    setWindowTitle("Simple Screen Recorder");
    setWindowIcon(QIcon(":/icons/recorder.png"));
    setMinimumSize(600, 500);

    setupUI();
    createMenuBar();

    // Connect signals
    connect(recorder.get(), &ScreenRecorder::stateChanged, this, &MainWindow::onRecordingStateChanged);
    connect(recorder.get(), &ScreenRecorder::recordingFinished, this, &MainWindow::onRecordingFinished);
    connect(recorder.get(), &ScreenRecorder::recordingError, this, &MainWindow::onRecordingError);
    
    connect(startBtn, &QPushButton::clicked, this, &MainWindow::onStartRecording);
    connect(stopBtn, &QPushButton::clicked, this, &MainWindow::onStopRecording);
    connect(pauseBtn, &QPushButton::clicked, this, &MainWindow::onPauseRecording);
    connect(regionSelectBtn, &QPushButton::clicked, this, &MainWindow::onSelectRegion);
    connect(settingsBtn, &QPushButton::clicked, this, &MainWindow::onOpenSettings);
    connect(openOutputBtn, &QPushButton::clicked, this, [this]() {
        if (!settings->outputPath.isEmpty()) {
            QDesktopServices::openUrl(QUrl::fromLocalFile(settings->outputPath));
        }
    });
    
    connect(qualitySlider, QOverload<int>::of(&QSlider::valueChanged), this, &MainWindow::onQualityChanged);
    connect(timerUpdate, &QTimer::timeout, this, &MainWindow::updateTimer);

    updateStatusLabel();
}

MainWindow::~MainWindow() = default;

void MainWindow::setupUI() {
    QWidget *centralWidget = new QWidget(this);
    setCentralWidget(centralWidget);

    QVBoxLayout *mainLayout = new QVBoxLayout(centralWidget);
    mainLayout->setSpacing(15);
    mainLayout->setContentsMargins(15, 15, 15, 15);

    // Status Group
    QGroupBox *statusGroup = new QGroupBox("Recording Status", this);
    QVBoxLayout *statusLayout = new QVBoxLayout(statusGroup);
    
    statusLabel = new QLabel("Status: Ready", this);
    statusLabel->setStyleSheet("font-weight: bold; font-size: 12px;");
    timeLabel = new QLabel("Time: 00:00:00", this);
    resolutionLabel = new QLabel("Resolution: Full Screen", this);
    
    statusLayout->addWidget(statusLabel);
    statusLayout->addWidget(timeLabel);
    statusLayout->addWidget(resolutionLabel);
    
    mainLayout->addWidget(statusGroup);

    // Control Buttons
    QGroupBox *controlGroup = new QGroupBox("Recording Controls", this);
    QHBoxLayout *controlLayout = new QHBoxLayout(controlGroup);
    
    startBtn = new QPushButton("Start Recording", this);
    startBtn->setMinimumHeight(40);
    stopBtn = new QPushButton("Stop Recording", this);
    stopBtn->setMinimumHeight(40);
    stopBtn->setEnabled(false);
    pauseBtn = new QPushButton("Pause", this);
    pauseBtn->setMinimumHeight(40);
    pauseBtn->setEnabled(false);
    
    controlLayout->addWidget(startBtn);
    controlLayout->addWidget(pauseBtn);
    controlLayout->addWidget(stopBtn);
    
    mainLayout->addWidget(controlGroup);

    // Region Selection
    QGroupBox *regionGroup = new QGroupBox("Recording Region", this);
    QVBoxLayout *regionLayout = new QVBoxLayout(regionGroup);
    
    regionSelectBtn = new QPushButton("Select Region", this);
    regionSelectBtn->setMinimumHeight(35);
    selectedRegionLabel = new QLabel("Selected: Full Screen", this);
    
    regionLayout->addWidget(regionSelectBtn);
    regionLayout->addWidget(selectedRegionLabel);
    
    mainLayout->addWidget(regionGroup);

    // Video Settings
    QGroupBox *videoGroup = new QGroupBox("Video Settings", this);
    QVBoxLayout *videoLayout = new QVBoxLayout(videoGroup);
    
    // FPS
    QHBoxLayout *fpsLayout = new QHBoxLayout();
    fpsLayout->addWidget(new QLabel("FPS:"));
    fpsSpinBox = new QSpinBox(this);
    fpsSpinBox->setMinimum(1);
    fpsSpinBox->setMaximum(120);
    fpsSpinBox->setValue(30);
    fpsLayout->addWidget(fpsSpinBox);
    fpsLayout->addStretch();
    videoLayout->addLayout(fpsLayout);
    
    // Codec
    QHBoxLayout *codecLayout = new QHBoxLayout();
    codecLayout->addWidget(new QLabel("Codec:"));
    codecCombo = new QComboBox(this);
    codecCombo->addItems({"H.264", "H.265 (HEVC)", "VP9", "FFV1"});
    codecLayout->addWidget(codecCombo);
    codecLayout->addStretch();
    videoLayout->addLayout(codecLayout);
    
    // Quality Slider
    QHBoxLayout *qualityLayout = new QHBoxLayout();
    qualityLayout->addWidget(new QLabel("Quality:"));
    qualitySlider = new QSlider(Qt::Horizontal, this);
    qualitySlider->setMinimum(1);
    qualitySlider->setMaximum(100);
    qualitySlider->setValue(85);
    qualityLabel = new QLabel("85%", this);
    qualityLabel->setMinimumWidth(40);
    qualityLayout->addWidget(qualitySlider);
    qualityLayout->addWidget(qualityLabel);
    videoLayout->addLayout(qualityLayout);
    
    // Checkboxes
    recordCursorCheckBox = new QCheckBox("Record Cursor", this);
    recordCursorCheckBox->setChecked(true);
    watermarkCheckBox = new QCheckBox("Enable Watermark", this);
    
    videoLayout->addWidget(recordCursorCheckBox);
    videoLayout->addWidget(watermarkCheckBox);
    
    mainLayout->addWidget(videoGroup);

    // Audio Settings
    QGroupBox *audioGroup = new QGroupBox("Audio Settings", this);
    QVBoxLayout *audioLayout = new QVBoxLayout(audioGroup);
    
    recordAudioCheckBox = new QCheckBox("Record Audio", this);
    recordAudioCheckBox->setChecked(true);
    
    QHBoxLayout *audioDeviceLayout = new QHBoxLayout();
    audioDeviceLayout->addWidget(new QLabel("Audio Device:"));
    audioSourceCombo = new QComboBox(this);
    audioSourceCombo->addItems({"Default", "Microphone", "System Audio"});
    audioDeviceLayout->addWidget(audioSourceCombo);
    audioDeviceLayout->addStretch();
    
    audioLayout->addWidget(recordAudioCheckBox);
    audioLayout->addLayout(audioDeviceLayout);
    
    mainLayout->addWidget(audioGroup);

    // Output Settings
    QGroupBox *outputGroup = new QGroupBox("Output", this);
    QHBoxLayout *outputLayout = new QHBoxLayout(outputGroup);
    
    settingsBtn = new QPushButton("Output Settings", this);
    settingsBtn->setMinimumHeight(35);
    openOutputBtn = new QPushButton("Open Output Folder", this);
    openOutputBtn->setMinimumHeight(35);
    
    outputLayout->addWidget(settingsBtn);
    outputLayout->addWidget(openOutputBtn);
    
    mainLayout->addWidget(outputGroup);

    mainLayout->addStretch();
}

void MainWindow::createMenuBar() {
    QMenuBar *menuBar = new QMenuBar(this);
    setMenuBar(menuBar);

    // File Menu
    QMenu *fileMenu = menuBar->addMenu("&File");
    QAction *exitAction = fileMenu->addAction("E&xit");
    connect(exitAction, &QAction::triggered, this, &QWidget::close);

    // Help Menu
    QMenu *helpMenu = menuBar->addMenu("&Help");
    QAction *aboutAction = helpMenu->addAction("&About");
    connect(aboutAction, &QAction::triggered, this, [this]() {
        QMessageBox::about(this, "About SimpleScreenRecorder",
            "SimpleScreenRecorder v1.0\n\n"
            "A simple yet powerful screen recording application for Windows.\n\n"
            "Features:\n"
            "• Full screen or region recording\n"
            "• Multiple codec support (H.264, H.265, VP9, FFV1)\n"
            "• Audio recording\n"
            "• Watermark support\n"
            "• Advanced compression settings");
    });
}

void MainWindow::onStartRecording() {
    if (!settings->validate(QString())) {
        QMessageBox::warning(this, "Invalid Settings", "Please check your recording settings.");
        return;
    }

    // Set up recorder with current settings
    recorder->setFrameRate(fpsSpinBox->value());
    recorder->setQuality(qualitySlider->value());
    recorder->setRecordAudio(recordAudioCheckBox->isChecked());
    recorder->setRecordCursor(recordCursorCheckBox->isChecked());
    recorder->setWatermark(watermarkCheckBox->isChecked(), "SimpleScreenRecorder");

    // Generate output filename
    QString timestamp = QDateTime::currentDateTime().toString("yyyy-MM-dd_hh-mm-ss");
    QString filename = QString("recording_%1.mp4").arg(timestamp);
    settings->filename = filename;
    recorder->setOutputPath(settings->getFullOutputPath());

    // Start recording
    if (recorder->startRecording()) {
        isRecording = true;
        elapsedSeconds = 0;
        timerUpdate->start(1000);
        updateStatusLabel();
    } else {
        QMessageBox::critical(this, "Error", "Failed to start recording!");
    }
}

void MainWindow::onStopRecording() {
    if (isRecording) {
        timerUpdate->stop();
        recorder->stopRecording();
        isRecording = false;
        isPaused = false;
        updateStatusLabel();
    }
}

void MainWindow::onPauseRecording() {
    if (!isRecording || isPaused) return;

    if (isPaused) {
        recorder->resumeRecording();
        pauseBtn->setText("Pause");
        isPaused = false;
    } else {
        recorder->pauseRecording();
        pauseBtn->setText("Resume");
        isPaused = true;
    }
}

void MainWindow::onSelectRegion() {
    RegionSelectionDialog dialog(this);
    if (dialog.exec() == QDialog::Accepted) {
        QRect region = dialog.getSelectedRegion();
        recorder->setRegion(region);
        selectedRegionLabel->setText(
            QString("Selected: %1x%2 at (%3, %4)")
                .arg(region.width())
                .arg(region.height())
                .arg(region.x())
                .arg(region.y()));
        resolutionLabel->setText(QString("Resolution: %1x%2").arg(region.width()).arg(region.height()));
    }
}

void MainWindow::onOpenSettings() {
    // TODO: Implement advanced settings dialog
    QMessageBox::information(this, "Settings", "Advanced settings dialog coming soon!");
}

void MainWindow::onRecordingStateChanged(bool isRec) {
    enableControls(!isRec);
}

void MainWindow::onRecordingFinished(const QString &filePath) {
    QMessageBox::information(this, "Success", 
        QString("Recording saved to:\n%1").arg(filePath));
    updateStatusLabel();
}

void MainWindow::onRecordingError(const QString &errorMsg) {
    QMessageBox::critical(this, "Recording Error", errorMsg);
    isRecording = false;
    timerUpdate->stop();
    updateStatusLabel();
}

void MainWindow::onQualityChanged(int value) {
    qualityLabel->setText(QString("%1%").arg(value));
}

void MainWindow::updateTimer() {
    elapsedSeconds++;
    int hours = elapsedSeconds / 3600;
    int minutes = (elapsedSeconds % 3600) / 60;
    int seconds = elapsedSeconds % 60;
    
    timeLabel->setText(QString("Time: %1:%2:%3")
        .arg(hours, 2, 10, QChar('0'))
        .arg(minutes, 2, 10, QChar('0'))
        .arg(seconds, 2, 10, QChar('0')));
}

void MainWindow::updateStatusLabel() {
    if (isRecording) {
        if (isPaused) {
            statusLabel->setText("Status: Paused");
            statusLabel->setStyleSheet("color: orange; font-weight: bold;");
        } else {
            statusLabel->setText("Status: Recording");
            statusLabel->setStyleSheet("color: red; font-weight: bold;");
        }
    } else {
        statusLabel->setText("Status: Ready");
        statusLabel->setStyleSheet("color: green; font-weight: bold;");
        timeLabel->setText("Time: 00:00:00");
    }
}

void MainWindow::enableControls(bool enable) {
    startBtn->setEnabled(enable);
    regionSelectBtn->setEnabled(enable);
    fpsSpinBox->setEnabled(enable);
    codecCombo->setEnabled(enable);
    qualitySlider->setEnabled(enable);
    recordAudioCheckBox->setEnabled(enable);
    recordCursorCheckBox->setEnabled(enable);
    watermarkCheckBox->setEnabled(enable);
    audioSourceCombo->setEnabled(enable);
    pauseBtn->setEnabled(!enable);
    stopBtn->setEnabled(!enable);
}
