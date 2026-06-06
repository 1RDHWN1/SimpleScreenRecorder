# Building SimpleScreenRecorder on Windows

## Prerequisites

### Visual Studio
- Visual Studio 2019 or 2022 (Community Edition is fine)
- C++ desktop development workload

### Tools
- CMake 3.20+
- Git
- vcpkg (optional but recommended)

## Option 1: Building with vcpkg (Recommended)

### 1. Setup vcpkg

```bash
# Clone vcpkg
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg

# Bootstrap vcpkg
.\bootstrap-vcpkg.bat

# Add vcpkg to PATH (optional, for convenience)
# System Properties -> Environment Variables -> Add vcpkg folder to PATH
```

### 2. Install dependencies

```bash
# From vcpkg directory
.\vcpkg install qt6:x64-windows
.\vcpkg install ffmpeg:x64-windows

# Optional: Install ffmpeg with additional codecs
.\vcpkg install ffmpeg[ffmpeg]:x64-windows
```

### 3. Clone and build SimpleScreenRecorder

```bash
git clone https://github.com/yourusername/SimpleScreenRecorder.git
cd SimpleScreenRecorder
mkdir build
cd build

# Configure with vcpkg integration
cmake .. -G "Visual Studio 17 2022" -A x64 -DCMAKE_TOOLCHAIN_FILE=[vcpkg]/scripts/buildsystems/vcpkg.cmake

# Build
cmake --build . --config Release -j 8
```

## Option 2: Manual Installation

### 1. Install Qt6

Download Qt installer from https://www.qt.io/download-open-source

```bash
# Set Qt path environment variable
set Qt6_DIR=C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6
```

### 2. Install FFmpeg

#### Method A: Download pre-built binaries
- Download from: https://ffmpeg.org/download.html
- Extract to: `C:\ffmpeg`
- Add to PATH

#### Method B: Build from source
```bash
# This is complex, use pre-built binaries instead
```

### 3. Build SimpleScreenRecorder

```bash
mkdir build
cd build

cmake .. -G "Visual Studio 17 2022" -A x64 -DQt6_DIR=C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6

cmake --build . --config Release
```

## Troubleshooting Build Issues

### CMake not found
```bash
# Add CMake to PATH
set PATH=%PATH%;C:\Program Files\CMake\bin
```

### Qt not found
```bash
# Manually specify Qt path
cmake .. -DQt6_DIR=C:\Qt\6.6.0\msvc2022_64\lib\cmake\Qt6
```

### FFmpeg libraries not found
```bash
# Copy FFmpeg libraries to build directory or system path
# Or specify FFmpeg path
cmake .. -DFFMPEG_PATH=C:\ffmpeg
```

### Missing include files
```bash
# Update FFmpeg includes
# Ensure libavcodec, libavformat, libavutil, libswscale headers are installed
```

### Runtime dependencies missing

After building, you need to copy runtime dependencies:

```bash
# Copy FFmpeg DLLs
copy C:\ffmpeg\bin\*.dll build\Release\

# Copy Qt DLLs (if not using static build)
copy C:\Qt\6.6.0\msvc2022_64\bin\Qt6*.dll build\Release\
```

Or add directories to PATH:
```bash
set PATH=%PATH%;C:\ffmpeg\bin;C:\Qt\6.6.0\msvc2022_64\bin
```

## Static Build

For a standalone executable without external DLL dependencies:

```bash
cmake .. -G "Visual Studio 17 2022" -A x64 \
  -DBUILD_SHARED_LIBS=OFF \
  -DQt6_DIR=[Qt6 static build]

cmake --build . --config Release
```

## Clean Build

If you encounter issues, try a clean build:

```bash
# Remove build directory
rmdir /s /q build
mkdir build
cd build

# Reconfigure and build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release -j 8
```

## Development Setup

### Using Visual Studio IDE

1. Open Visual Studio
2. File → Open → Folder
3. Select SimpleScreenRecorder directory
4. Visual Studio will auto-detect CMakeLists.txt

### Using VS Code

1. Install extensions:
   - C/C++ Extension Pack
   - CMake Tools

2. Open folder in VS Code
3. Select build configuration (Release/Debug)
4. Press F5 to build and debug

### Debug Build

```bash
cmake .. -DCMAKE_BUILD_TYPE=Debug
cmake --build . --config Debug -j 8
```

## Unit Testing (Optional)

```bash
# Install Google Test
vcpkg install gtest:x64-windows

# Configure with tests
cmake .. -DBUILD_TESTS=ON

# Run tests
ctest
```

## Performance Testing

```bash
# Build with optimization flags
cmake .. -DCMAKE_CXX_FLAGS="/O2 /arch:AVX2"
```

## Known Issues

1. **FFmpeg version compatibility**
   - Requires FFmpeg 4.4 or later
   - Older versions may have API incompatibilities

2. **Qt version compatibility**
   - Tested with Qt 6.2+
   - Qt 5.x may require code modifications

3. **Audio codec issues**
   - Some audio devices may not support all codecs
   - Use PCM WAV as fallback

## Support

For build issues, check:
1. CMAKE version: `cmake --version`
2. Visual Studio installed correctly
3. Qt configuration: Check CMAKE error messages
4. FFmpeg libraries in PATH

For additional help, open an issue on GitHub with:
- OS version and build tools versions
- Full CMake output
- Error messages
