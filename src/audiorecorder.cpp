#include "audiorecorder.h"
#include <QAudioRecorder>
#include <QAudioInput>
#include <QMediaDevices>

AudioRecorder::AudioRecorder(QObject *parent)
    : QObject(parent), recorder(std::make_unique<QAudioRecorder>()) {
    
    connect(recorder.get(), QOverload<QMediaRecorder::Error>::of(&QAudioRecorder::error),
            this, [this](QMediaRecorder::Error err) {
        lastError = recorder->errorString();
        emit recordingError(lastError);
    });

    connect(recorder.get(), &QAudioRecorder::recorderStateChanged,
            this, [this](QMediaRecorder::RecorderState state) {
        if (state == QMediaRecorder::RecordingState) {
            emit recordingStarted();
        } else {
            emit recordingStopped();
        }
    });
}

AudioRecorder::~AudioRecorder() = default;

bool AudioRecorder::startRecording(const QString &outputPath) {
    if (!recorder) {
        return false;
    }

    recorder->setOutputLocation(QUrl::fromLocalFile(outputPath));
    
    // Configure audio settings
    QAudioEncoderSettings settings;
    settings.setCodec("audio/PCM");
    settings.setSampleRate(44100);
    settings.setChannelCount(2);
    settings.setBitRate(192000);
    
    recorder->setAudioInput(QMediaDevices::defaultAudioInput());
    recorder->setEncoderSettings(settings);

    recorder->record();
    return recorder->recorderState() == QMediaRecorder::RecordingState;
}

void AudioRecorder::stopRecording() {
    if (recorder && recorder->recorderState() == QMediaRecorder::RecordingState) {
        recorder->stop();
    }
}

void AudioRecorder::pauseRecording() {
    if (recorder && recorder->recorderState() == QMediaRecorder::RecordingState) {
        recorder->pause();
    }
}

void AudioRecorder::resumeRecording() {
    if (recorder && recorder->recorderState() == QMediaRecorder::PausedState) {
        recorder->record();
    }
}

bool AudioRecorder::isRecording() const {
    return recorder && recorder->recorderState() == QMediaRecorder::RecordingState;
}
