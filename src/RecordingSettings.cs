using System;

namespace SimpleScreenRecorder;

public class RecordingSettings
{
    public string Codec { get; set; } = "h264";
    public int Quality { get; set; } = 85;
    public int FPS { get; set; } = 30;
    public bool RecordAudio { get; set; } = true;
    public string? AudioDevice { get; set; }
    public bool RecordCursor { get; set; } = true;
    public bool UseWatermark { get; set; } = false;
    public string? WatermarkText { get; set; }
    public float WatermarkOpacity { get; set; } = 0.7f;
    public bool IsFullScreen { get; set; } = true;
    public int RegionX { get; set; } = 0;
    public int RegionY { get; set; } = 0;
    public int RegionWidth { get; set; } = 1920;
    public int RegionHeight { get; set; } = 1080;

    public string GetCodecName() => Codec switch
    {
        "h264" => "libx264",
        "h265" => "libx265",
        "vp9" => "libvpx-vp9",
        _ => "libx264"
    };

    public string GetCodecLabel() => Codec switch
    {
        "h264" => "H.264 (Recommended)",
        "h265" => "H.265 (Best Compression)",
        "vp9" => "VP9 (Open Source)",
        _ => "H.264"
    };

    public string GetCRFValue()
    {
        // Convert 1-100 quality to 0-51 CRF (lower = better)
        // 85 = CRF 18 (good balance)
        // 50 = CRF 28 (medium)
        // 100 = CRF 0 (lossless)
        return ((101 - Quality) * 51 / 100).ToString();
    }

    public string GetFFmpegCodecArgs()
    {
        return Codec switch
        {
            "h265" => $"-c:v {GetCodecName()} -crf {GetCRFValue()} -pix_fmt yuv420p",
            "vp9" => $"-c:v {GetCodecName()} -b:v 2M -pix_fmt yuv420p",
            _ => $"-c:v {GetCodecName()} -crf {GetCRFValue()} -pix_fmt yuv420p"
        };
    }
}
