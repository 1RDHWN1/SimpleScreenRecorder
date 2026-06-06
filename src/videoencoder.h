#ifndef VIDEOENCODER_H
#define VIDEOENCODER_H

#include <QString>
#include <QPixmap>
#include <memory>

// Forward declarations for FFmpeg
struct AVCodecContext;
struct AVFormatContext;
struct AVFrame;
struct AVPacket;
struct SwsContext;

class VideoEncoder {
public:
    enum Codec {
        H264,
        H265,
        VP9,
        FFV1
    };

    VideoEncoder();
    ~VideoEncoder();

    bool initialize(const QString &outputPath, int width, int height, int fps, 
                   Codec codec, int quality);
    bool encodeFrame(const QPixmap &pixmap, int64_t timestamp);
    bool finalize();

    int getWidth() const { return width; }
    int getHeight() const { return height; }

private:
    void cleanupCodec();
    AVFrame* pixmapToAVFrame(const QPixmap &pixmap);

    AVCodecContext *codecCtx = nullptr;
    AVFormatContext *formatCtx = nullptr;
    SwsContext *swsCtx = nullptr;
    AVFrame *frame = nullptr;
    AVPacket *packet = nullptr;

    int width = 0;
    int height = 0;
    int fps = 30;
    Codec codec;
    int quality = 85;
    int frameCount = 0;
};

#endif // VIDEOENCODER_H
