@echo off

set ROOT=%~dp0

set BUILD_DIR="%ROOT%\Intermediate\Win64"

cmake --version >nul 2>&1

if %ERRORLEVEL%==0 (
    set CMAKE_BIN=cmake
) else (
    set CMAKE_BIN=%ROOT%Tools\cmake-4.0.0-rc3-windows-x86_64\bin\cmake.exe
)

echo Using cmake at %CMAKE_BIN%

@echo on
%CMAKE_BIN% -B %BUILD_DIR% -G "Visual Studio 17 2022" -DUSE_LIVEPP=OFF -DENABLE_ASAN=OFF
@echo off

if /I NOT "%GITHUB_ACTIONS%"=="true" (
    pause
)