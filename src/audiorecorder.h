#ifndef AUDIORECORDER_H
#define AUDIORECORDER_H

#include <QString>
#include <QObject>
#include <memory>

class QAudioRecorder;
class QAudioDecoder;

class AudioRecorder : public QObject {
    Q_OBJECT

public:
    explicit AudioRecorder(QObject *parent = nullptr);
    ~AudioRecorder();

    bool startRecording(const QString &outputPath);
    void stopRecording();
    void pauseRecording();
    void resumeRecording();

    QString getLastErrorMessage() const { return lastError; }
    bool isRecording() const;

signals:
    void recordingStarted();
    void recordingStopped();
    void recordingError(const QString &error);

private:
    std::unique_ptr<QAudioRecorder> recorder;
    QString lastError;
};

#endif // AUDIORECORDER_H
