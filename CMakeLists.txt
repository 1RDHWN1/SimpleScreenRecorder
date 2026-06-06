cmake_minimum_required(VERSION 3.20)
project(SimpleScreenRecorder)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

# Find Qt6
find_package(Qt6 REQUIRED COMPONENTS 
    Core 
    Gui 
    Widgets
    Multimedia
    OpenGL
)

# Find FFmpeg
find_package(PkgConfig QUIET)
if(PkgConfig_FOUND)
    pkg_check_modules(FFMPEG IMPORTED_TARGET libavcodec libavformat libavutil libswscale)
endif()

# Source files
set(SOURCES
    src/main.cpp
    src/mainwindow.cpp
    src/mainwindow.h
    src/screenrecorder.cpp
    src/screenrecorder.h
    src/audiorecorder.cpp
    src/audiorecorder.h
    src/videoencoder.cpp
    src/videoencoder.h
    src/regionselectiondialog.cpp
    src/regionselectiondialog.h
    src/recordingsettings.cpp
    src/recordingsettings.h
)

# Create executable
add_executable(SimpleScreenRecorder ${SOURCES})

# Link libraries
target_link_libraries(SimpleScreenRecorder
    Qt6::Core
    Qt6::Gui
    Qt6::Widgets
    Qt6::Multimedia
    Qt6::OpenGL
)

# Link FFmpeg if found
if(FFMPEG_FOUND)
    target_link_libraries(SimpleScreenRecorder ${FFMPEG_LIBRARIES})
    target_include_directories(SimpleScreenRecorder PRIVATE ${FFMPEG_INCLUDE_DIRS})
else()
    message(WARNING "FFmpeg not found. Install libavcodec-dev, libavformat-dev, libavutil-dev, libswscale-dev")
endif()

# Link Windows-specific libraries for screen capture
if(WIN32)
    target_link_libraries(SimpleScreenRecorder
        gdi32
        user32
        winmm
        ole32
        oleaut32
        dsound
        dxva2
        d3d11
        dxgi
    )
endif()
