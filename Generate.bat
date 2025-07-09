
set ROOT=%~dp0

set BUILD_DIR="%ROOT%\Intermediate\Win64"

set CMAKE_BIN=%ROOT%Tools\cmake-4.0.0-rc3-windows-x86_64\bin\cmake.exe

%CMAKE_BIN% -B %BUILD_DIR% -G "Visual Studio 17 2022" -DUSE_LIVEPP=ON

pause