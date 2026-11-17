@echo off
setlocal

echo ==============================================================================
echo   Unity IL Localization Toolkit - Build System
echo ==============================================================================
echo.

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

set REPO_ROOT=%~dp0
set BIN_DIR=%REPO_ROOT%bin
if not exist "%BIN_DIR%" mkdir "%BIN_DIR%"

set STEAM_MANAGED=C:\Program Files (x86)\Steam\steamapps\common\Travelling at Night Demo\travelling_Data\Managed

echo [1/3] Compiling InteractiveDemo.exe (Standalone CLI Technology Showcase)...
"%CSC%" /nologo /t:exe /out:"%BIN_DIR%\InteractiveDemo.exe" "%REPO_ROOT%src\Core\KoreanParticleResolver.cs" "%REPO_ROOT%src\Core\TagAndSpriteParser.cs" "%REPO_ROOT%src\Core\TranslationEngine.cs" "%REPO_ROOT%src\Demo\InteractiveDemo.cs"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to compile InteractiveDemo.exe.
    exit /b 1
)
echo [PASS] InteractiveDemo.exe compiled successfully.

echo.
echo [2/3] Compiling AssemblyPatcher.exe (Mono.Cecil IL Injector)...
"%CSC%" /nologo /t:exe /r:"%REPO_ROOT%lib\Mono.Cecil.dll" /out:"%BIN_DIR%\AssemblyPatcher.exe" "%REPO_ROOT%src\Injector\*.cs"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to compile AssemblyPatcher.exe.
    exit /b 1
)
echo [PASS] AssemblyPatcher.exe compiled successfully.

echo.
echo [3/3] Compiling UnityTextHookPatch.dll (In-Game Runtime Patch Engine)...
if exist "%STEAM_MANAGED%\UnityEngine.CoreModule.dll" (
    "%CSC%" /nologo /t:library /r:"%STEAM_MANAGED%\netstandard.dll" /r:"%STEAM_MANAGED%\UnityEngine.CoreModule.dll" /r:"%STEAM_MANAGED%\UnityEngine.UI.dll" /r:"%STEAM_MANAGED%\UnityEngine.TextRenderingModule.dll" /r:"%STEAM_MANAGED%\Unity.TextMeshPro.dll.bak" /out:"%BIN_DIR%\UnityTextHookPatch.dll" "%REPO_ROOT%src\Core\*.cs" "%REPO_ROOT%src\Patch\*.cs"
    if errorlevel 1 (
        echo [ERROR] Failed to compile UnityTextHookPatch.dll.
        exit /b 1
    )
    echo [PASS] UnityTextHookPatch.dll compiled successfully.
) else (
    echo [INFO] Unity reference DLLs not found in default Steam path.
)

echo.
echo ==============================================================================
echo   Build finished! All binaries are located in bin\
echo   Try running: bin\InteractiveDemo.exe
echo ==============================================================================
echo.
exit /b 0
