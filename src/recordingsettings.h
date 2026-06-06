#ifndef RECORDINGSETTINGS_H
#define RECORDINGSETTINGS_H

#include <QString>
#include <QRect>

struct RecordingSettings {
    // Output
    QString outputPath;
    QString filename = "recording.mp4";

    // Recording area
    QRect recordingRegion;
    bool recordFullScreen = true;

    // Video settings
    int frameRate = 30;
    QString codec = "h264";
    int bitrate = 5000; // kbps
    int quality = 85; // 0-100

    // Audio settings
    bool recordAudio = true;
    QString audioDevice;
    int audioSampleRate = 44100;

    // Advanced
    bool recordCursor = true;
    bool enableWatermark = false;
    QString watermarkText = "SimpleScreenRecorder";
    QString watermarkPosition = "bottom-right";

    // Performance
    bool useHardwareAcceleration = true;
    int maxMemoryUsage = 512; // MB

    QString getFullOutputPath() const;
    bool validate(QString &errorMessage);
};

#endif // RECORDINGSETTINGS_H
