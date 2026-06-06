# SimpleScreenRecorder

A powerful and user-friendly screen recording application for Windows, built with C++ and Qt.

## Features

✨ **Core Features:**
- Full screen or custom region recording
- High-quality video encoding
- Audio recording support
- Cursor recording
- Real-time preview

🎥 **Video Codecs:**
- H.264 (AVC) - Best compatibility
- H.265 (HEVC) - Better compression
- VP9 - Royalty-free, high quality
- FFV1 - Lossless compression

🔧 **Advanced Features:**
- Adjustable frame rate (1-120 FPS)
- Quality control (1-100%)
- Watermark support
- Audio device selection
- Hardware acceleration
- Pause/Resume recording

## System Requirements

### Windows
- Windows 7 or later
- Visual Studio 2019 or later (for building)
- CMake 3.20+
- Qt 6.0+
- FFmpeg development libraries

### Required Libraries

```bash
# vcpkg (recommended for Windows)
vcpkg install qt6:x64-windows
vcpkg install ffmpeg:x64-windows
```

Or manually:
- libavcodec-dev
- libavformat-dev
- libavutil-dev
- libswscale-dev

## Installation

### Building from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/SimpleScreenRecorder.git
   cd SimpleScreenRecorder
   ```

2. **Create build directory**
   ```bash
   mkdir build
   cd build
   ```

3. **Configure with CMake**
   ```bash
   cmake .. -G "Visual Studio 17 2022" -A x64
   ```

4. **Build**
   ```bash
   cmake --build . --config Release
   ```

5. **Run**
   ```bash
   Release\SimpleScreenRecorder.exe
   ```

## Usage

### Basic Recording
1. Launch SimpleScreenRecorder
2. Click **"Start Recording"** to capture the entire screen
3. Click **"Stop Recording"** to finish
4. Recording is saved to `Videos` folder by default

### Custom Region Recording
1. Click **"Select Region"** button
2. Click and drag to draw your recording area
3. Release mouse to confirm region
4. Click **"Start Recording"**

### Advanced Settings

#### Video Quality
- Adjust the quality slider (1-100%)
- Higher quality = larger file size

#### Codec Selection
- **H.264**: Best for general use, widely compatible
- **H.265**: Better compression, requires newer players
- **VP9**: Open-source, excellent quality
- **FFV1**: Lossless, largest file sizes

#### Frame Rate
- Adjust FPS spinbox (1-120)
- 30 FPS: Standard for most recordings
- 60 FPS: Smooth motion, larger files
- 120 FPS: High-speed, very large files

#### Audio Recording
- Enable/Disable audio recording
- Select audio device (Microphone, System Audio, etc.)
- Audio is encoded as PCM WAV

#### Cursor Recording
- Enable cursor overlay in recordings
- Red crosshair marks cursor position

#### Watermark
- Add text watermark to recordings
- Watermark appears in bottom-right corner
- Semi-transparent for visibility

### Output Settings
- Default location: `C:\Users\[Username]\Videos`
- Filenames: `recording_YYYY-MM-DD_HH-MM-SS.mp4`
- Can open output folder via UI button

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ESC | Cancel region selection |
| Space | Pause/Resume recording |

## Performance Tips

1. **Lower resolution** for better performance
2. **Reduce frame rate** if encoding is slow
3. **Use H.264** for faster encoding
4. **Enable hardware acceleration** when available
5. **Close unnecessary applications** while recording

## Troubleshooting

### Audio not recording
- Check that audio device is properly selected
- Verify audio device is not in use by another application
- Ensure "Record Audio" checkbox is enabled

### Slow encoding
- Lower frame rate
- Use H.264 codec
- Reduce recording resolution
- Close other applications

### File corruption
- Ensure enough disk space before recording
- Don't move files while recording is in progress
- Use supported file paths (avoid special characters)

### FFmpeg not found
- Install FFmpeg development libraries
- Add FFmpeg bin directory to PATH
- Rebuild the application

## Development

### Project Structure
```
SimpleScreenRecorder/
├── CMakeLists.txt           # Build configuration
├── README.md                # This file
└── src/
    ├── main.cpp             # Application entry point
    ├── mainwindow.*          # Main UI window
    ├── screenrecorder.*      # Screen capture logic
    ├── videoencoder.*        # FFmpeg video encoding
    ├── audiorecorder.*       # Audio recording
    ├── regionselectiondialog.* # Region selection UI
    └── recordingsettings.*   # Settings management
```

### Building for Development

```bash
# Debug build with symbols
cmake .. -DCMAKE_BUILD_TYPE=Debug
cmake --build . --config Debug

# Run with debugger
gdb ./SimpleScreenRecorder
```

### Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## License

This project is licensed under the MIT License - see LICENSE file for details.

## Acknowledgments

- Qt Framework (https://www.qt.io)
- FFmpeg (https://ffmpeg.org)
- SimpleScreenRecorder Linux original (https://github.com/MaartenBaert/ssr)

## Support

For issues, questions, or suggestions, please open an issue on GitHub.

## Roadmap

- [ ] Support for multiple displays
- [ ] Webcam overlay option
- [ ] Screenshot functionality
- [ ] Video editing tools
- [ ] Streaming to platforms (Twitch, YouTube)
- [ ] Configuration presets
- [ ] Scheduling recordings
- [ ] Post-processing filters
- [ ] Linux support
- [ ] macOS support
