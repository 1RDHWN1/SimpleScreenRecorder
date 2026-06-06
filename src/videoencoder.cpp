#include "videoencoder.h"

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libavutil/avutil.h>
#include <libavutil/pixdesc.h>
#include <libswscale/swscale.h>
}

#include <QPixmap>
#include <QImage>
#include <QString>

VideoEncoder::VideoEncoder() = default;

VideoEncoder::~VideoEncoder() {
    cleanupCodec();
}

bool VideoEncoder::initialize(const QString &outputPath, int width, int height, int fps,
                             Codec codec, int quality) {
    this->width = width;
    this->height = height;
    this->fps = fps;
    this->codec = codec;
    this->quality = quality;

    // Create output context
    avformat_alloc_output_context2(&formatCtx, nullptr, nullptr, outputPath.toStdString().c_str());
    if (!formatCtx) {
        return false;
    }

    // Select codec
    const AVCodec *selectedCodec = nullptr;
    switch (codec) {
        case H264:
            selectedCodec = avcodec_find_encoder_by_name("libx264");
            break;
        case H265:
            selectedCodec = avcodec_find_encoder_by_name("libx265");
            break;
        case VP9:
            selectedCodec = avcodec_find_encoder_by_name("libvpx-vp9");
            break;
        case FFV1:
            selectedCodec = avcodec_find_encoder_by_name("ffv1");
            break;
    }

    if (!selectedCodec) {
        avformat_free_context(formatCtx);
        return false;
    }

    // Create codec context
    codecCtx = avcodec_alloc_context3(selectedCodec);
    if (!codecCtx) {
        avformat_free_context(formatCtx);
        return false;
    }

    // Configure codec
    codecCtx->width = width;
    codecCtx->height = height;
    codecCtx->pix_fmt = AV_PIX_FMT_YUV420P;
    codecCtx->time_base = {1, fps};
    codecCtx->framerate = {fps, 1};

    // Quality settings
    if (codec == H264 || codec == H265) {
        codecCtx->bit_rate = (quality / 100.0) * 5000000; // Variable bitrate based on quality
        codecCtx->rc_max_rate = codecCtx->bit_rate * 1.5;
        codecCtx->rc_buffer_size = codecCtx->bit_rate;
    }

    // Open codec
    if (avcodec_open2(codecCtx, selectedCodec, nullptr) < 0) {
        avcodec_free_context(&codecCtx);
        avformat_free_context(formatCtx);
        return false;
    }

    // Add stream
    AVStream *stream = avformat_new_stream(formatCtx, selectedCodec);
    if (!stream) {
        avcodec_free_context(&codecCtx);
        avformat_free_context(formatCtx);
        return false;
    }

    avcodec_parameters_from_context(stream->codecpar, codecCtx);
    stream->time_base = codecCtx->time_base;

    // Open output file
    if (!(formatCtx->oformat->flags & AVFMT_NOFILE)) {
        if (avio_open(&formatCtx->pb, outputPath.toStdString().c_str(), AVIO_FLAG_WRITE) < 0) {
            avcodec_free_context(&codecCtx);
            avformat_free_context(formatCtx);
            return false;
        }
    }

    // Write header
    if (avformat_write_header(formatCtx, nullptr) < 0) {
        avcodec_free_context(&codecCtx);
        avformat_free_context(formatCtx);
        return false;
    }

    // Allocate frame
    frame = av_frame_alloc();
    frame->format = codecCtx->pix_fmt;
    frame->width = width;
    frame->height = height;

    if (av_frame_get_buffer(frame, 32) < 0) {
        av_frame_free(&frame);
        avcodec_free_context(&codecCtx);
        avformat_free_context(formatCtx);
        return false;
    }

    // Allocate packet
    packet = av_packet_alloc();

    // Create scale context for QPixmap to YUV conversion
    swsCtx = sws_getContext(width, height, AV_PIX_FMT_RGB32,
                           width, height, AV_PIX_FMT_YUV420P,
                           SWS_BILINEAR, nullptr, nullptr, nullptr);

    return true;
}

bool VideoEncoder::encodeFrame(const QPixmap &pixmap, int64_t timestamp) {
    if (!codecCtx || !frame || !packet) {
        return false;
    }

    // Convert QPixmap to AVFrame
    AVFrame *rgbFrame = pixmapToAVFrame(pixmap);
    if (!rgbFrame) {
        return false;
    }

    // Scale from RGB to YUV420P
    const uint8_t *srcData[1] = {rgbFrame->data[0]};
    int srcLinesize[1] = {rgbFrame->linesize[0]};

    sws_scale(swsCtx, srcData, srcLinesize, 0, height,
             frame->data, frame->linesize);

    frame->pts = frameCount;
    frameCount++;

    // Encode frame
    int ret = avcodec_send_frame(codecCtx, frame);
    if (ret < 0) {
        av_frame_free(&rgbFrame);
        return false;
    }

    // Receive packets
    while (ret >= 0) {
        ret = avcodec_receive_packet(codecCtx, packet);
        if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) {
            break;
        }
        if (ret < 0) {
            av_frame_free(&rgbFrame);
            return false;
        }

        // Write packet
        packet->stream_index = 0;
        av_packet_rescale_ts(packet, codecCtx->time_base, formatCtx->streams[0]->time_base);
        av_interleaved_write_frame(formatCtx, packet);
        av_packet_unref(packet);
    }

    av_frame_free(&rgbFrame);
    return true;
}

bool VideoEncoder::finalize() {
    if (!codecCtx) {
        return false;
    }

    // Flush encoder
    avcodec_send_frame(codecCtx, nullptr);

    while (avcodec_receive_packet(codecCtx, packet) >= 0) {
        packet->stream_index = 0;
        av_packet_rescale_ts(packet, codecCtx->time_base, formatCtx->streams[0]->time_base);
        av_interleaved_write_frame(formatCtx, packet);
        av_packet_unref(packet);
    }

    // Write trailer
    av_write_trailer(formatCtx);

    cleanupCodec();
    return true;
}

void VideoEncoder::cleanupCodec() {
    if (frame) {
        av_frame_free(&frame);
    }
    if (packet) {
        av_packet_free(&packet);
    }
    if (swsCtx) {
        sws_freeContext(swsCtx);
        swsCtx = nullptr;
    }
    if (codecCtx) {
        avcodec_free_context(&codecCtx);
    }
    if (formatCtx) {
        if (!(formatCtx->oformat->flags & AVFMT_NOFILE)) {
            avio_closep(&formatCtx->pb);
        }
        avformat_free_context(formatCtx);
        formatCtx = nullptr;
    }
}

AVFrame *VideoEncoder::pixmapToAVFrame(const QPixmap &pixmap) {
    QImage image = pixmap.toImage();
    image = image.convertToFormat(QImage::Format_RGB32);

    AVFrame *rgbFrame = av_frame_alloc();
    if (!rgbFrame) {
        return nullptr;
    }

    rgbFrame->format = AV_PIX_FMT_RGB32;
    rgbFrame->width = width;
    rgbFrame->height = height;

    if (av_frame_get_buffer(rgbFrame, 32) < 0) {
        av_frame_free(&rgbFrame);
        return nullptr;
    }

    // Copy image data
    for (int y = 0; y < height; ++y) {
        memcpy(rgbFrame->data[0] + y * rgbFrame->linesize[0],
              image.scanLine(y),
              width * 4);
    }

    return rgbFrame;
}
