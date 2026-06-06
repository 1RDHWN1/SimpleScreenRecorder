#include "recordingsettings.h"
#include <QStandardPaths>
#include <QDir>

QString RecordingSettings::getFullOutputPath() const {
    if (outputPath.isEmpty()) {
        // Use default videos directory
        QString videosPath = QStandardPaths::writableLocation(QStandardPaths::MoviesLocation);
        return QDir(videosPath).filePath(filename);
    }
    return QDir(outputPath).filePath(filename);
}

bool RecordingSettings::validate(QString &errorMessage) {
    if (filename.isEmpty()) {
        errorMessage = "Filename cannot be empty";
        return false;
    }

    if (frameRate < 1 || frameRate > 120) {
        errorMessage = "Frame rate must be between 1 and 120 FPS";
        return false;
    }

    if (quality < 1 || quality > 100) {
        errorMessage = "Quality must be between 1 and 100";
        return false;
    }

    if (recordingRegion.width() <= 0 || recordingRegion.height() <= 0) {
        errorMessage = "Invalid recording region";
        return false;
    }

    if (recordAudio && audioDevice.isEmpty()) {
        errorMessage = "No audio device selected";
        return false;
    }

    return true;
}
