# SimpleScreenRecorder - Architecture Guide

## 🏗️ System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                    SimpleScreenRecorder Application              │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    MainWindow (UI Layer)                 │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │   │
│  │  │ Controls     │  │ Settings     │  │ Status       │   │   │
│  │  │ - Start      │  │ - FPS        │  │ - Recording  │   │   │
│  │  │ - Stop       │  │ - Codec      │  │ - Time       │   │   │
│  │  │ - Pause      │  │ - Quality    │  │ - Resolution │   │   │
│  │  │ - Region     │  │ - Audio      │  │              │   │   │
│  │  └──────────────┘  └──────────────┘  └──────────────┘   │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              ↕                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              Recording Engine (Core Layer)              │   │
│  │                                                         │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │           ScreenRecorder (Main Controller)       │  │   │
│  │  │  - Manages recording state                       │  │   │
│  │  │  - Coordinates screen capture                    │  │   │
│  │  │  - Handles pause/resume                          │  │   │
│  │  │  - Adds watermark & cursor overlay               │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │           ↓                              ↓              │   │
│  │  ┌──────────────────────┐  ┌──────────────────────┐    │   │
│  │  │  VideoEncoder        │  │  AudioRecorder       │    │   │
│  │  │  (FFmpeg)            │  │  (Qt Multimedia)     │    │   │
│  │  │ ┌────────────────┐   │  │ ┌────────────────┐   │    │   │
│  │  │ │ • H.264        │   │  │ │ • Device Sel   │   │    │   │
│  │  │ │ • H.265        │   │  │ │ • PCM WAV      │   │    │   │
│  │  │ │ • VP9          │   │  │ │ • 44.1kHz      │   │    │   │
│  │  │ │ • FFV1         │   │  │ │ • 2 channels   │   │    │   │
│  │  │ └────────────────┘   │  │ └────────────────┘   │    │   │
│  │  └──────────────────────┘  └──────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              ↓                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │            Platform Layer (Windows APIs)               │   │
│  │                                                         │   │
│  │  ┌──────────────────┐  ┌──────────────────────────┐   │   │
│  │  │ GDI (Screen)     │  │ WinMM/WASAPI (Audio)     │   │   │
│  │  │ - GetDC()        │  │ - Device enumeration     │   │   │
│  │  │ - BitBlt()       │  │ - Audio capture          │   │   │
│  │  │ - StretchBlt()   │  │ - Audio mixing           │   │   │
│  │  └──────────────────┘  └──────────────────────────┘   │   │
│  │                                                         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              ↓                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │         Output Files                                   │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ recording_YYYY-MM-DD_HH-MM-SS.mp4               │  │   │
│  │  │ (H.264/H.265/VP9 + AAC audio)                   │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

## 🔄 Data Flow Diagram

```
                    [User Interaction]
                          ↓
    ┌─────────────────────────────────────────┐
    │  MainWindow                             │
    │  ┌─────────────────────────────────┐   │
    │  │ UI Controls (Buttons, Sliders)  │   │
    │  └────────────┬────────────────────┘   │
    │               ↓                        │
    │  ┌─────────────────────────────────┐   │
    │  │ Settings Validation             │   │
    │  └────────────┬────────────────────┘   │
    │               ↓                        │
    │  ┌─────────────────────────────────┐   │
    │  │ ScreenRecorder.startRecording() │   │
    │  └────────────┬────────────────────┘   │
    └───────────────┼────────────────────────┘
                    ↓
    ┌─────────────────────────────────────────┐
    │  ScreenRecorder                         │
    │  ┌─────────────────────────────────┐   │
    │  │ Create VideoEncoder             │   │
    │  │ Create AudioRecorder            │   │
    │  └────────────┬────────────────────┘   │
    │               ↓                        │
    │  ┌─────────────────────────────────┐   │
    │  │ Start Capture Thread            │   │
    │  └────────────┬────────────────────┘   │
    │               ↓                        │
    │  ┌─────────────────────────────────┐   │
    │  │ captureLoop()                   │   │
    │  │ - Sleep(1000/FPS)               │   │
    │  │ - captureScreen()               │   │
    │  │ - drawCursor() (optional)       │   │
    │  │ - addWatermark() (optional)     │   │
    │  │ - encodeFrame()                 │   │
    │  └────────────┬────────────────────┘   │
    │               ↓                        │
    │  ┌─────────────────────────────────┐   │
    │  │ stopRecording()                 │   │
    │  │ - Signal encoder to finalize    │   │
    │  │ - Stop audio recorder           │   │
    │  │ - Wait for encoding to complete │   │
    │  └────────────┬────────────────────┘   │
    └───────────────┼────────────────────────┘
                    ↓
    ┌─────────────────────────────────────────┐
    │  VideoEncoder (FFmpeg)                  │
    │  Each Frame:                            │
    │  - QPixmap → RGB32                      │
    │  - RGB32 → YUV420P (sws_scale)          │
    │  - Encode to H.264/H.265/VP9            │
    │  - Write to MP4/MKV container           │
    └───────────────┬────────────────────────┘
                    ↓
    ┌─────────────────────────────────────────┐
    │  AudioRecorder (Qt Multimedia)          │
    │  - Capture from device                  │
    │  - Buffer audio frames                  │
    │  - Write to WAV file                    │
    └───────────────┬────────────────────────┘
                    ↓
    ┌─────────────────────────────────────────┐
    │  Output File                            │
    │  recording_YYYY-MM-DD_HH-MM-SS.mp4      │
    └─────────────────────────────────────────┘
```

## 📦 Module Dependencies

```
main.cpp
    ↓
MainWindow
    ├─→ ScreenRecorder
    │       ├─→ VideoEncoder
    │       │       └─→ FFmpeg libs
    │       │
    │       └─→ AudioRecorder
    │               └─→ Qt Multimedia
    │
    ├─→ RegionSelectionDialog
    │       └─→ Qt Core/GUI
    │
    └─→ RecordingSettings
            └─→ Qt Core
```

## 🧵 Threading Model

```
Main Thread (Qt Event Loop)
    ↓
    ├─→ [UI Events]
    │   - Button clicks
    │   - Slider changes
    │   - Menu selections
    │   ↓
    │   MainWindow event handlers
    │   ↓
    │   Update UI elements
    │
    └─→ [Recording Thread]  (ScreenRecorder::captureLoop)
        Spawned when recording starts
        ├─→ Loop at FPS rate
        ├─→ Capture screen (non-blocking)
        ├─→ Process image
        ├─→ Send to VideoEncoder
        └─→ Signals back to Main Thread
            (framesCaptured, stateChanged)

Audio Thread (Qt Multimedia)
    - Runs asynchronously
    - Buffers audio samples
    - No synchronization needed (handled internally)
```

## 🎬 Recording State Machine

```
                    ┌──────────┐
                    │  Start   │
                    └────┬─────┘
                         ↓
          ┌──────────────────────────────┐
          │         IDLE                 │
          │  (Application ready)         │
          └─────────────┬────────────────┘
                        │ startRecording()
                        ↓
          ┌──────────────────────────────┐
          │      INITIALIZING            │
          │  (Setup encoder/audio)       │
          └─────────────┬────────────────┘
                        │
                        ↓
          ┌──────────────────────────────┐
    ┌────→│      RECORDING               │←───┐
    │     │  (Capture running)           │    │
    │     └────┬─────────┬────────────────┘    │
    │          │         │ pauseRecording()    │
    │          │         ↓                     │
    │          │    ┌────────────────────┐    │
    │          │    │   PAUSED           │    │
    │          │    │ (Capture paused)   │────┘
    │          │    └────────────────────┘
    │          │ stopRecording()
    │          ↓
    │     ┌──────────────────────────────┐
    └─────│      FINALIZING              │
          │  (Encoding, file write)      │
          └────┬─────────────────────────┘
               │
               ↓
          ┌──────────────────────────────┐
          │      STOPPED                 │
          │  (File saved, cleanup)       │
          └─────────────┬────────────────┘
                        │
                        ↓ reset
          ┌──────────────────────────────┐
          │         IDLE                 │
          │  (Ready for next recording)  │
          └──────────────────────────────┘
```

## 💾 Memory Management

```
During Recording:

┌─────────────────────────────┐
│  Main Thread Stack          │
│  ├─ MainWindow (heap)       │
│  │   └─ ~50 MB              │
│  └─ UI Controls             │
└─────────────────────────────┘
             ↓
┌─────────────────────────────┐
│  Recording Thread Heap      │
│  ├─ ScreenRecorder          │
│  │  └─ ~100 MB              │
│  ├─ Frame Buffer (QPixmap)  │
│  │  └─ ~50-200 MB           │
│  └─ Processing buffers      │
└─────────────────────────────┘
             ↓
┌─────────────────────────────┐
│  VideoEncoder Buffers       │
│  ├─ AVFrame                 │
│  │  └─ ~20 MB               │
│  ├─ Codec context           │
│  │  └─ ~10 MB               │
│  └─ Encoding buffers        │
│     └─ ~30 MB               │
└─────────────────────────────┘
             ↓
┌─────────────────────────────┐
│  AudioRecorder Buffers      │
│  ├─ Audio frames            │
│  │  └─ ~5-10 MB             │
│  └─ WAV buffer              │
│     └─ ~10 MB               │
└─────────────────────────────┘

Total typical: ~300-400 MB
Peak: ~500+ MB during encoding
```

## 🔐 Error Handling Strategy

```
User Action
    ↓
Validation
    ├─ Valid → Proceed
    │   ↓
    │   Execute
    │   ├─ Success → Update UI
    │   └─ Error → Show Dialog
    │
    └─ Invalid → Show Error Message
        ↓
        Display reason
        ↓
        Suggest fix
```

## 📊 Performance Characteristics

### CPU Usage
- **Idle**: < 1%
- **Recording (30fps 1080p H.264)**: 20-35%
- **Recording (60fps 1080p H.264)**: 40-60%
- **Recording (30fps 1080p H.265)**: 30-50%

### Memory Usage
- **Idle**: ~100 MB
- **Recording**: 300-500 MB
- **Recording + Encoding**: 400-600 MB

### Disk I/O
- **H.264 1080p 30fps**: ~40-50 MB/min
- **H.264 1080p 60fps**: ~80-100 MB/min
- **H.265 1080p 30fps**: ~25-35 MB/min
- **VP9 1080p 30fps**: ~30-40 MB/min

---

For implementation details, see individual source files in `/src` directory.
