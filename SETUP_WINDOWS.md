# SimpleScreenRecorder - Windows Setup Guide (Simplified)

## ⚡ Quick Setup (5 menit)

### Step 1: Download & Install Qt6
1. Visit: https://www.qt.io/download-open-source
2. Download Qt 6.6.0 or latest
3. Install to: `C:\Qt\6.6.0\msvc2022_64`
   (Remember this path!)

### Step 2: Download FFmpeg
Option A (Binary):
```powershell
# Download pre-built: https://ffmpeg.org/download.html
# Extract to: C:\ffmpeg
# Add to PATH: setx PATH "%PATH%;C:\ffmpeg\bin"
```

Option B (Binary from Full): 
```powershell
# Download "FFmpeg builds" from BtbN
# Extract to C:\ffmpeg
```

### Step 3: Setup Environment
```powershell
# Set Qt path
$env:Qt6_DIR = "C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6"

# Optional: Add FFmpeg to PATH for later use
$env:FFMPEG_PATH = "C:\ffmpeg"
```

### Step 4: Build
```powershell
cd C:\SimpleScreenRecorder
mkdir build
cd build

# Configure (update paths if different)
cmake .. -G "Visual Studio 17 2022" -A x64 `
  -DQt6_DIR="C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6" `
  -DFFMPEG_PATH="C:\ffmpeg"

# Build
cmake --build . --config Release -j 8

# Run
.\Release\SimpleScreenRecorder.exe
```

## 🔧 If you have different paths:

### Qt not at C:\Qt\?
```powershell
# Find Qt location
dir C:\
# Then use: -DQt6_DIR="C:\[YourPath]\msvc2022_64\lib\cmake\Qt6"
```

### FFmpeg not at C:\ffmpeg\?
```powershell
# If you have FFmpeg elsewhere:
cmake .. -DFFMPEG_PATH="C:\YourFFmpegPath"
```

### Visual Studio version different?
```powershell
# For VS 2019:
cmake .. -G "Visual Studio 16 2019" -A x64

# For VS 2022:
cmake .. -G "Visual Studio 17 2022" -A x64
```

## 📋 Complete Step-by-Step

```
1. Install Qt6
   └─ Download from qt.io
   └─ Choose MSVC 2022 64-bit
   └─ Install location: C:\Qt\6.6.0\msvc2022_64

2. Download FFmpeg
   └─ From ffmpeg.org/download.html
   └─ Extract to C:\ffmpeg
   └─ Should have: ffmpeg\bin, ffmpeg\lib, ffmpeg\include

3. Build
   ├─ Open PowerShell
   ├─ cd C:\SimpleScreenRecorder
   ├─ mkdir build && cd build
   ├─ cmake .. -G "Visual Studio 17 2022" -A x64
   │   -DQt6_DIR="C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6"
   │   -DFFMPEG_PATH="C:\ffmpeg"
   ├─ cmake --build . --config Release -j 8
   └─ .\Release\SimpleScreenRecorder.exe
```

## ✅ Verification

After install, verify:
```powershell
# Check Qt
ls "C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6"

# Check FFmpeg
ls "C:\ffmpeg\bin"
ls "C:\ffmpeg\lib"
ls "C:\ffmpeg\include"
```

Should see:
- Qt: Multiple Qt6* folders
- FFmpeg: DLLs in bin, LIB files in lib, headers in include

## ❌ Common Issues & Fixes

### "Qt6 not found"
```powershell
# Check Qt6_DIR path is correct:
ls "C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6"
# Should show: Qt6 folder with version info

# Rebuild with correct path:
rm -r build
mkdir build
cd build
cmake .. -DQt6_DIR="C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6"
```

### "FFmpeg not found"
```powershell
# Option 1: Provide FFmpeg path
cmake .. -DFFMPEG_PATH="C:\ffmpeg"

# Option 2: Add to environment first
$env:FFMPEG_PATH = "C:\ffmpeg"
cmake ..
```

### Build fails with linker errors
```powershell
# Make sure FFmpeg has lib files:
ls C:\ffmpeg\lib
# Should show: avcodec.lib, avformat.lib, etc.

# If not, download FFmpeg dev libraries instead
```

### DLL not found at runtime
```powershell
# Copy FFmpeg DLLs to build directory:
copy C:\ffmpeg\bin\*.dll build\Release\

# Or add to PATH:
$env:PATH += ";C:\ffmpeg\bin"
```

## 📦 Download Links

**Qt6 Official**: https://www.qt.io/download-open-source
- Version: 6.6.0 or latest
- Component: MSVC 2022 64-bit

**FFmpeg Binary**:
- https://ffmpeg.org/download.html
- Or: https://github.com/BtbN/FFmpeg-Builds/releases

## 🎯 Default Build Locations

After successful build, you'll have:
```
C:\SimpleScreenRecorder\
├── build\
│   └── Release\
│       └── SimpleScreenRecorder.exe  ← This is your app!
│
├── src\ (source files)
└── CMakeLists.txt
```

## 🚀 Run the Application

```powershell
# From build directory:
.\Release\SimpleScreenRecorder.exe

# Or from project root:
.\build\Release\SimpleScreenRecorder.exe
```

## 💡 Tips

1. **First time builds are slow** - CMake needs to analyze everything
2. **Subsequent builds are faster** - Only changed files recompile
3. **Clean build if issues**: `rm -r build && mkdir build && cd build`
4. **64-bit only**: This build is for x64 Windows

## 📞 Still having issues?

Check:
1. Qt installation path is correct
2. FFmpeg has lib\ folder with .lib files
3. Visual Studio build tools installed
4. CMake version >= 3.20 (`cmake --version`)
5. PowerShell running as Admin (if needed)

---

**Next: Start with the standard build steps above!**
