#include "screenrecorder.h"
#include "videoencoder.h"
#include "audiorecorder.h"

#include <QScreen>
#include <QGuiApplication>
#include <QPixmap>
#include <QCursor>
#include <QImage>
#include <QPainter>
#include <QDateTime>
#include <chrono>
#include <thread>

ScreenRecorder::ScreenRecorder(QObject *parent)
    : QObject(parent), videoEncoder(std::make_unique<VideoEncoder>()),
      audioRecorder(std::make_unique<AudioRecorder>(this)) {
    // Initialize with full screen by default
    QScreen *screen = QGuiApplication::primaryScreen();
    selectedRegion = screen->geometry();
}

ScreenRecorder::~ScreenRecorder() {
    stopRecording();
    if (captureThread.joinable()) {
        captureThread.join();
    }
}

void ScreenRecorder::setOutputPath(const QString &path) {
    outputPath = path;
}

void ScreenRecorder::setRegion(const QRect &rect) {
    selectedRegion = rect;
}

void ScreenRecorder::setFrameRate(int fps) {
    frameRate = fps;
}

void ScreenRecorder::setCodec(const QString &codec) {
    this->codec = codec;
}

void ScreenRecorder::setQuality(int quality) {
    this->quality = qBound(1, quality, 100);
}

void ScreenRecorder::setRecordAudio(bool enable) {
    recordAudio = enable;
}

void ScreenRecorder::setRecordCursor(bool enable) {
    recordCursor = enable;
}

void ScreenRecorder::setWatermark(bool enable, const QString &text) {
    enableWatermark = enable;
    watermarkText = text.isEmpty() ? "SimpleScreenRecorder" : text;
}

bool ScreenRecorder::startRecording() {
    if (state != Idle) {
        return false;
    }

    // Initialize video encoder
    if (!videoEncoder->initialize(outputPath, selectedRegion.width(), 
                                 selectedRegion.height(), frameRate, 
                                 VideoEncoder::H264, quality)) {
        emit recordingError("Failed to initialize video encoder");
        return false;
    }

    // Start audio recording if enabled
    if (recordAudio) {
        QString audioPath = outputPath + ".wav";
        if (!audioRecorder->startRecording(audioPath)) {
            emit recordingError("Failed to start audio recording");
            return false;
        }
    }

    // Initialize recording
    recordingStartTime = QDateTime::currentMSecsSinceEpoch();
    frameCount = 0;
    state = Recording;
    shouldExit = false;
    isPaused = false;

    // Start capture thread
    captureThread = std::thread(&ScreenRecorder::captureLoop, this);

    emit stateChanged(Recording);
    return true;
}

void ScreenRecorder::stopRecording() {
    if (state == Idle) {
        return;
    }

    state = Stopped;
    shouldExit = true;

    if (captureThread.joinable()) {
        captureThread.join();
    }

    // Stop audio recording
    if (recordAudio) {
        audioRecorder->stopRecording();
    }

    // Finalize video encoding
    if (!videoEncoder->finalize()) {
        emit recordingError("Failed to finalize video encoding");
    } else {
        emit recordingFinished(outputPath);
    }

    state = Idle;
    emit stateChanged(Idle);
}

void ScreenRecorder::pauseRecording() {
    if (state == Recording) {
        isPaused = true;
        state = Paused;
        audioRecorder->pauseRecording();
        emit stateChanged(Paused);
    }
}

void ScreenRecorder::resumeRecording() {
    if (state == Paused) {
        isPaused = false;
        state = Recording;
        audioRecorder->resumeRecording();
        emit stateChanged(Recording);
    }
}

ScreenRecorder::RecordingState ScreenRecorder::getState() const {
    return state;
}

QRect ScreenRecorder::getSelectedRegion() const {
    return selectedRegion;
}

QSize ScreenRecorder::getResolution() const {
    return selectedRegion.size();
}

void ScreenRecorder::captureLoop() {
    int frameDelay = 1000 / frameRate; // milliseconds

    while (!shouldExit) {
        auto frameStart = std::chrono::high_resolution_clock::now();

        if (!isPaused) {
            // Capture screen
            QPixmap screenshot = captureScreen();
            
            if (recordCursor) {
                screenshot = drawCursor(screenshot);
            }

            if (enableWatermark) {
                addWatermark(screenshot);
            }

            // Calculate timestamp
            int64_t currentTime = QDateTime::currentMSecsSinceEpoch();
            int64_t timestamp = currentTime - recordingStartTime;

            // Encode frame
            if (videoEncoder->encodeFrame(screenshot, timestamp)) {
                frameCount++;
                emit framesCaptured(frameCount);
            }
        }

        // Frame rate control
        auto frameEnd = std::chrono::high_resolution_clock::now();
        auto elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(frameEnd - frameStart);
        
        if (elapsed.count() < frameDelay) {
            std::this_thread::sleep_for(
                std::chrono::milliseconds(frameDelay - elapsed.count()));
        }
    }
}

void ScreenRecorder::addWatermark(QPixmap &pixmap) {
    QPainter painter(&pixmap);
    painter.setRenderHint(QPainter::Antialiasing);

    // Set font and color
    QFont font = painter.font();
    font.setPointSize(14);
    font.setBold(true);
    painter.setFont(font);

    // Draw semi-transparent background
    QFontMetrics fm(font);
    int textWidth = fm.horizontalAdvance(watermarkText);
    int textHeight = fm.height();
    int padding = 5;

    QRect bgRect(pixmap.width() - textWidth - 2 * padding - 10,
                 pixmap.height() - textHeight - 2 * padding - 10,
                 textWidth + 2 * padding,
                 textHeight + 2 * padding);

    painter.setOpacity(0.3);
    painter.fillRect(bgRect, Qt::black);
    painter.setOpacity(1.0);

    // Draw text
    painter.setPen(Qt::white);
    painter.drawText(bgRect.adjusted(padding, padding, -padding, -padding),
                    Qt::AlignCenter, watermarkText);
    painter.end();
}

QPixmap ScreenRecorder::captureScreen() {
    QScreen *screen = QGuiApplication::primaryScreen();
    return screen->grabWindow(0, selectedRegion.x(), selectedRegion.y(),
                             selectedRegion.width(), selectedRegion.height());
}

QPixmap ScreenRecorder::drawCursor(const QPixmap &pixmap) {
    QPixmap result = pixmap;
    
    QCursor cursor;
    QPoint cursorPos = QCursor::pos();
    
    // Adjust cursor position to recording region
    cursorPos -= selectedRegion.topLeft();

    // Check if cursor is within recording region
    if (result.rect().contains(cursorPos)) {
        QPainter painter(&result);
        
        // Draw cursor shape (simple crosshair for now)
        painter.setPen(QPen(Qt::red, 2));
        int size = 15;
        
        // Horizontal line
        painter.drawLine(cursorPos.x() - size, cursorPos.y(),
                        cursorPos.x() + size, cursorPos.y());
        
        // Vertical line
        painter.drawLine(cursorPos.x(), cursorPos.y() - size,
                        cursorPos.x(), cursorPos.y() + size);
        
        painter.end();
    }

    return result;
}
