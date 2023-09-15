set MOD_NAME=SRPlaylistManager

set BUILT_DLL=".\%MOD_NAME%\bin\Debug\net6.0\%MOD_NAME%.dll"
set LIB_DLL_DIR=".\%MOD_NAME%\bin\Debug\net6.0\libs"
set SYNTHRIDERS_MODS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods"

copy %BUILT_DLL% %SYNTHRIDERS_MODS_DIR%
copy %LIB_DLL_DIR%\* %SYNTHRIDERS_MODS_DIR%
