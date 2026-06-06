#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QPushButton>
#include <QLabel>
#include <QSpinBox>
#include <QComboBox>
#include <QCheckBox>
#include <QSlider>
#include <QTimer>
#include <memory>

class ScreenRecorder;
class RecordingSettings;

class MainWindow : public QMainWindow {
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private slots:
    void onStartRecording();
    void onStopRecording();
    void onPauseRecording();
    void onSelectRegion();
    void onOpenSettings();
    void onRecordingStateChanged(bool isRecording);
    void onRecordingFinished(const QString &filePath);
    void onRecordingError(const QString &errorMsg);
    void onQualityChanged(int value);
    void updateTimer();

private:
    void setupUI();
    void createMenuBar();
    void updateStatusLabel();
    void enableControls(bool enable);

    // UI Components
    QPushButton *startBtn;
    QPushButton *stopBtn;
    QPushButton *pauseBtn;
    QPushButton *regionSelectBtn;
    QPushButton *settingsBtn;
    QPushButton *openOutputBtn;
    
    QLabel *statusLabel;
    QLabel *timeLabel;
    QLabel *resolutionLabel;
    QLabel *qualityLabel;
    
    QSpinBox *fpsSpinBox;
    QComboBox *codecCombo;
    QComboBox *audioSourceCombo;
    QCheckBox *recordAudioCheckBox;
    QCheckBox *recordCursorCheckBox;
    QCheckBox *watermarkCheckBox;
    QSlider *qualitySlider;
    
    QLabel *selectedRegionLabel;

    // Recording core
    std::unique_ptr<ScreenRecorder> recorder;
    std::unique_ptr<RecordingSettings> settings;

    // Timer
    QTimer *timerUpdate;

    // State
    bool isRecording = false;
    bool isPaused = false;
    int elapsedSeconds = 0;
};

#endif // MAINWINDOW_H
