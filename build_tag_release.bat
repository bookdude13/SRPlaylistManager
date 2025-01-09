@echo off

IF %1.==. goto :Usage

set MOD_NAME="SRPlaylistManager"
set BUILD_SCRIPT=.\SRModCore\build.py
set VERSION=%1

echo "Building release..."
python.exe %BUILD_SCRIPT% --clean --tag -n "%MOD_NAME%" -c Release %VERSION% build_files.txt || goto :ERROR

echo "Done"
goto :EOF

:ERROR
echo "Error occurred in build script! Error code: %errorlevel%"

:Usage
echo "Usage: ./build_tag_release.bat <version>"
