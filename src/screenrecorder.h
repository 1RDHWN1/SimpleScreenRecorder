#ifndef SCREENRECORDER_H
#define SCREENRECORDER_H

#include <QObject>
#include <QPixmap>
#include <QRect>
#include <memory>
#include <thread>
#include <atomic>
#include <queue>

class VideoEncoder;
class AudioRecorder;

struct FrameData {
    QPixmap screenshot;
    int64_t timestamp; // milliseconds
};

class ScreenRecorder : public QObject {
    Q_OBJECT

public:
    enum RecordingState {
        Idle,
        Recording,
        Paused,
        Stopped
    };

    explicit ScreenRecorder(QObject *parent = nullptr);
    ~ScreenRecorder();

    void setOutputPath(const QString &path);
    void setRegion(const QRect &rect);
    void setFrameRate(int fps);
    void setCodec(const QString &codec);
    void setQuality(int quality);
    void setRecordAudio(bool enable);
    void setRecordCursor(bool enable);
    void setWatermark(bool enable, const QString &text = "");

    bool startRecording();
    void stopRecording();
    void pauseRecording();
    void resumeRecording();

    RecordingState getState() const;
    QRect getSelectedRegion() const;
    QSize getResolution() const;

signals:
    void stateChanged(RecordingState state);
    void recordingFinished(const QString &filePath);
    void recordingError(const QString &errorMessage);
    void framesCaptured(int count);

private:
    void captureLoop();
    void addWatermark(QPixmap &pixmap);
    QPixmap captureScreen();
    QPixmap drawCursor(const QPixmap &pixmap);

    std::unique_ptr<VideoEncoder> videoEncoder;
    std::unique_ptr<AudioRecorder> audioRecorder;

    std::thread captureThread;
    std::atomic<RecordingState> state{Idle};
    std::atomic<bool> shouldExit{false};
    std::atomic<bool> isPaused{false};

    QString outputPath;
    QRect selectedRegion;
    int frameRate = 30;
    QString codec = "h264";
    int quality = 85;
    bool recordAudio = true;
    bool recordCursor = true;
    bool enableWatermark = false;
    QString watermarkText;

    int64_t recordingStartTime = 0;
    int frameCount = 0;
};

#endif // SCREENRECORDER_H
