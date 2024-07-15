@echo off

call %~dp0Binaries\ThirdParty\premake5.exe vs2022 --file=Source/UOEngine.Solution.lua
pause
exit /B %ERRORLEVEL%