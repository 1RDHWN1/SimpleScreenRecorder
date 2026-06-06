# SimpleScreenRecorder - Quick Start Guide

## 📋 Daftar Isi
1. [Instalasi](#instalasi)
2. [Panduan Penggunaan](#panduan-penggunaan)
3. [Tips Performa](#tips-performa)
4. [FAQ](#faq)

## Instalasi

### Prasyarat Sistem
- **OS**: Windows 7 atau lebih baru
- **RAM**: 4GB minimum (8GB recommended)
- **Storage**: 50GB free space untuk high-quality recordings
- **CPU**: Multi-core processor recommended

### Cara Install

#### 1. Download
```bash
git clone https://github.com/yourusername/SimpleScreenRecorder.git
cd SimpleScreenRecorder
```

#### 2. Install Dependencies (vcpkg recommended)
```bash
# Clone vcpkg
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg
.\bootstrap-vcpkg.bat

# Install packages
.\vcpkg install qt6:x64-windows
.\vcpkg install ffmpeg:x64-windows
```

#### 3. Build
```bash
mkdir build
cd build
cmake .. -G "Visual Studio 17 2022" -A x64 -DCMAKE_TOOLCHAIN_FILE=[path-to-vcpkg]/scripts/buildsystems/vcpkg.cmake
cmake --build . --config Release
```

#### 4. Run
```bash
.\Release\SimpleScreenRecorder.exe
```

## Panduan Penggunaan

### Interface Utama

```
┌─────────────────────────────────────────┐
│ Simple Screen Recorder                  │
├─────────────────────────────────────────┤
│ Recording Status                        │
│ ├─ Status: Ready                        │
│ ├─ Time: 00:00:00                       │
│ └─ Resolution: Full Screen              │
├─────────────────────────────────────────┤
│ Recording Controls                      │
│ ├─ [Start] [Pause] [Stop]               │
├─────────────────────────────────────────┤
│ Recording Region                        │
│ ├─ [Select Region]                      │
│ └─ Selected: Full Screen                │
├─────────────────────────────────────────┤
│ Video Settings                          │
│ ├─ FPS: 30                              │
│ ├─ Codec: H.264                         │
│ ├─ Quality: ████████░ 85%               │
│ ├─ ☑ Record Cursor                      │
│ └─ ☐ Enable Watermark                   │
├─────────────────────────────────────────┤
│ Audio Settings                          │
│ ├─ ☑ Record Audio                       │
│ └─ Audio Device: Default                │
├─────────────────────────────────────────┤
│ Output                                  │
│ ├─ [Output Settings] [Open Folder]      │
└─────────────────────────────────────────┘
```

### Skenario Penggunaan

#### Skenario 1: Rekam Tutorial Layar Penuh
1. Buka SimpleScreenRecorder
2. Settings default sudah OK (30 FPS, H.264, 85%)
3. Klik **"Start Recording"**
4. Lakukan aktivitas yang ingin direkam
5. Klik **"Stop Recording"**
6. File akan tersimpan di folder Videos

#### Skenario 2: Rekam Game dengan Region Custom
1. Klik **"Select Region"**
2. Klik & drag untuk pilih area game
3. Release mouse untuk confirm
4. Ubah FPS ke 60 untuk smooth motion
5. Klik **"Start Recording"**
6. Mulai main game
7. Klik **"Stop Recording"**

#### Skenario 3: Rekam dengan Watermark
1. Centang checkbox **"Enable Watermark"**
2. Adjust Quality slider ke 90%+
3. Codec: pilih VP9 untuk better quality
4. Klik **"Start Recording"**
5. Rekam seperti biasa

### Pengaturan Rekomendasi

#### Untuk Tutorial/Presentasi
```
FPS: 30
Codec: H.264
Quality: 85%
Audio: Enabled
Cursor: Enabled
Watermark: Disabled
```

#### Untuk Gaming/Action
```
FPS: 60
Codec: H.264
Quality: 95%
Audio: Enabled
Cursor: Enabled
Watermark: Disabled
```

#### Untuk Streaming
```
FPS: 30-60
Codec: H.264
Quality: 70-80%
Audio: Enabled
Cursor: Enabled
Watermark: Enabled (Optional)
```

#### Untuk Archival/Backup
```
FPS: 30
Codec: FFV1 (Lossless)
Quality: 100%
Audio: Enabled
Cursor: Disabled
Watermark: Disabled
```

## Tips Performa

### Meningkatkan Kualitas
- ✅ Gunakan SSD untuk output (lebih cepat)
- ✅ Tingkatkan Quality slider ke 90+%
- ✅ Gunakan codec H.265 untuk file lebih kecil
- ✅ Reduce resolution jika H.264/H.265 slow

### Mengurangi Beban CPU
- ✅ Reduce FPS (30 instead of 60)
- ✅ Gunakan H.264 (faster than H.265/VP9)
- ✅ Record region instead of full screen
- ✅ Close unnecessary apps

### Mengoptimalkan Disk Usage
- ✅ Gunakan codec H.265 atau VP9
- ✅ Lower quality setting (70-80%)
- ✅ Reduce resolution
- ✅ Record at lower FPS

### Mencegah Audio Sync Issues
- ✅ Use default audio device
- ✅ Avoid changing audio device during recording
- ✅ Ensure minimum 2GB free RAM
- ✅ Close audio-related apps

## FAQ

### Q: File terlalu besar, gimana?
**A:** Coba:
1. Reduce quality ke 70-80%
2. Use codec H.265 atau VP9
3. Reduce FPS ke 30
4. Record region yang lebih kecil

### Q: Audio tidak terekam
**A:** Pastikan:
1. "Record Audio" checkbox ✓ dicentang
2. Audio Device bukan "(None)"
3. Volume tidak muted
4. Device tidak digunakan app lain

### Q: Encoding lambat/lag
**A:**
1. Close unnecessary apps
2. Use H.264 codec (faster)
3. Reduce FPS
4. Reduce resolution
5. Lower quality

### Q: Video jerky/stuttering
**A:**
1. Lower FPS (dari 60 ke 30)
2. Reduce resolution
3. Close background apps
4. Check disk space (perlu >5GB)

### Q: Watermark gak terlihat
**A:**
1. Check "Enable Watermark" is checked
2. Watermark di bottom-right corner
3. Increase quality (watermark lebih jelas)

### Q: Output format apa yang supported?
**A:**
- Primary: MP4 (H.264, H.265)
- Alternative: MKV (VP9, FFV1)
- Audio: WAV (PCM)

### Q: Bisa pause recording?
**A:** Ya! Klik tombol Pause untuk pause, Resume untuk lanjut

### Q: Bisa customize output folder?
**A:** Klik "Output Settings" untuk ubah lokasi default

### Q: Minimum hardware requirement?
**A:**
- CPU: Dual-core minimum (quad-core recommended)
- RAM: 4GB (8GB+ recommended)
- Storage: 50GB free untuk high-quality
- GPU: Optional (untuk acceleration)

### Q: Support Linux/Mac?
**A:** Roadmap include Linux & macOS support (planned for v2.0)

## Shortcut Keys

| Key | Action |
|-----|--------|
| `Space` | Pause/Resume recording |
| `ESC` | Cancel region selection |
| `Ctrl+Q` | Exit application |

## Keyboard Navigation

- `Tab`: Navigate between UI elements
- `Enter`: Activate button
- `Space`: Toggle checkbox

## Command Line Arguments

```bash
SimpleScreenRecorder.exe [options]

Options:
  --region X,Y,W,H      Auto-select region on startup
  --fps N               Set FPS (1-120)
  --codec CODEC         Set codec (h264/h265/vp9/ffv1)
  --quality N           Set quality (1-100)
  --output PATH         Set output directory
  --help                Show this help message
```

## Contoh penggunaan:
```bash
# Record 1920x1080 region at 60fps
SimpleScreenRecorder.exe --region 0,0,1920,1080 --fps 60

# Record with H.265 codec at 95% quality
SimpleScreenRecorder.exe --codec h265 --quality 95

# Custom output directory
SimpleScreenRecorder.exe --output "D:\MyRecordings"
```

## Troubleshooting Checklist

- [ ] Windows 7 atau lebih baru?
- [ ] Minimum 4GB RAM tersedia?
- [ ] Cukup disk space (5GB+)?
- [ ] FFmpeg libraries installed?
- [ ] Qt6 libraries installed?
- [ ] Visual C++ Redistributable installed?
- [ ] Admin privileges (jika diperlukan)?

## Support & Reporting Issues

Jika menemukan bug atau punya saran:

1. Check existing issues di GitHub
2. Provide:
   - Windows version
   - Recording settings
   - Error message (jika ada)
   - Steps to reproduce
3. Create issue dengan detail sesuai template

## Performance Benchmarks

Typical performance on mid-range hardware (Intel i5, 8GB RAM, SSD):

| Setting | FPS | Quality | File Size/min | CPU Usage |
|---------|-----|---------|---------------|-----------|
| Full HD 30fps H.264 | 30 | 85% | 40-50MB | 25-35% |
| Full HD 60fps H.264 | 60 | 85% | 80-100MB | 40-50% |
| Full HD 30fps H.265 | 30 | 85% | 25-35MB | 40-50% |
| 1280x720 30fps H.264 | 30 | 85% | 20-25MB | 15-25% |

---

**Happy Recording! 🎥**

Untuk dokumentasi lengkap, lihat README.md dan BUILDING.md
