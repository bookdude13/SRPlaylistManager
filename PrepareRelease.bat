
set BUILT_VERSION=1.2.0
set MOD_NAME=SRPlaylistManager

set RELEASE_BUILD_DIR=.\%MOD_NAME%\bin\Release
set MAIN_DLL=%RELEASE_BUILD_DIR%\%MOD_NAME%.dll
set LIB_DLL_DIR=%RELEASE_BUILD_DIR%\libs

set OUTPUT_DIR=.\build\%MOD_NAME%_v%BUILT_VERSION%
mkdir %OUTPUT_DIR%

set OUTPUT_MODS_DIR=%OUTPUT_DIR%\Mods
mkdir %OUTPUT_MODS_DIR%

copy %MAIN_DLL% %OUTPUT_MODS_DIR%
copy %LIB_DLL_DIR%\* %OUTPUT_MODS_DIR%

powershell Compress-Archive %OUTPUT_MODS_DIR% %OUTPUT_DIR%\%MOD_NAME%_v%BUILT_VERSION%.zip
