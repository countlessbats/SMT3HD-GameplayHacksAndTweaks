@echo off
setlocal enabledelayedexpansion
title sevrL_bats SMT3 HD Mod Pack Installer
echo.
echo  ============================================
echo   sevrL_bats Mod Pack for SMT3 Nocturne HD
echo   Gameplay Hacks and Tweaks V2.3.4
echo  ============================================
echo.

:: ── Check we're in the right folder ──
if not exist "smt3hd.exe" (
    echo  ERROR: smt3hd.exe not found!
    echo  Please extract this zip into your SMT3 HD game folder:
    echo    Steam\steamapps\common\smt3hd\
    echo.
    echo  Then double-click install_modpack.bat again.
    echo.
    pause
    exit /b 1
)

:: ── Check MelonLoader ──
if exist "MelonLoader\" (
    echo  [OK] MelonLoader detected.
) else (
    echo  MelonLoader not found. Installing MelonLoader v0.6.1...
    echo  Downloading from GitHub...
    echo.
    powershell -Command "& { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://github.com/LavaGang/MelonLoader/releases/download/v0.6.1/MelonLoader.x64.zip' -OutFile 'MelonLoader_temp.zip' }"
    if not exist "MelonLoader_temp.zip" (
        echo  ERROR: Download failed. Check your internet connection.
        echo  Install MelonLoader manually from:
        echo    https://github.com/LavaGang/MelonLoader/releases
        echo.
        pause
        exit /b 1
    )
    echo  Extracting MelonLoader...
    powershell -Command "& { Expand-Archive -Path 'MelonLoader_temp.zip' -DestinationPath '.' -Force }"
    del "MelonLoader_temp.zip" >nul 2>&1
    if exist "MelonLoader\" (
        echo  [OK] MelonLoader installed successfully.
    ) else (
        echo  ERROR: MelonLoader extraction failed.
        pause
        exit /b 1
    )
)

echo.
echo  ============================================
echo   Choose which mods to install:
echo  ============================================
echo.
echo   Each mod is independent. Install any combination.
echo   Type Y or N for each, then press Enter.
echo.

if not exist "Mods" mkdir Mods

:: ── Prey Eyes 2 ──
echo  --------------------------------------------------
echo   PREY EYES 2 (v2.5.5)
echo   Color-coded targeting reticle + affinity board.
echo   GREEN=weak, RED=null, YELLOW=resist, ?=unknown.
echo   Ailment resistances shown above the board.
echo   Cathedral of Shadows fusion board included.
echo   Press START in battle for BuffView.
echo  --------------------------------------------------
set /p INSTALL_PE="  Install Prey Eyes 2? (Y/N): "
if /i "!INSTALL_PE!"=="Y" (
    copy /y "_pack\PreyEyes2.dll" "Mods\PreyEyes2.dll" >nul
    xcopy /s /e /i /y "_pack\icons" "Mods\icons" >nul
    echo  [OK] Prey Eyes 2 installed.
) else (
    echo  [ ] Prey Eyes 2 skipped.
)
echo.

:: ── SuspendSafe ──
echo  --------------------------------------------------
echo   SUSPENDSAFE
echo   Press R3 to quicksave into the suspend slot
echo   without being kicked back to the title screen.
echo  --------------------------------------------------
set /p INSTALL_SS="  Install SuspendSafe? (Y/N): "
if /i "!INSTALL_SS!"=="Y" (
    copy /y "_pack\SuspendSafe.dll" "Mods\SuspendSafe.dll" >nul
    echo  [OK] SuspendSafe installed.
) else (
    echo  [ ] SuspendSafe skipped.
)
echo.

:: ── KeptSuspense ──
echo  --------------------------------------------------
echo   KEPTSUSPENSE
echo   Keep your Suspend save file after loading it.
echo   No more losing your quicksave on resume.
echo  --------------------------------------------------
set /p INSTALL_KS="  Install KeptSuspense? (Y/N): "
if /i "!INSTALL_KS!"=="Y" (
    copy /y "_pack\KeptSuspense.dll" "Mods\KeptSuspense.dll" >nul
    echo  [OK] KeptSuspense installed.
) else (
    echo  [ ] KeptSuspense skipped.
)
echo.

:: ── QuickPass ──
echo  --------------------------------------------------
echo   QUICKPASS
echo   R1 = quick-pass a demon's turn.
echo   L1 = quick-attempt escape.
echo  --------------------------------------------------
set /p INSTALL_QP="  Install QuickPass? (Y/N): "
if /i "!INSTALL_QP!"=="Y" (
    copy /y "_pack\QuickPass.dll" "Mods\QuickPass.dll" >nul
    echo  [OK] QuickPass installed.
) else (
    echo  [ ] QuickPass skipped.
)
echo.

:: ── MoonKing ──
echo  --------------------------------------------------
echo   MOONKING
echo   D-pad Down toggles Full/New Moon phase.
echo   No more waiting on mystic chests.
echo  --------------------------------------------------
set /p INSTALL_MK="  Install MoonKing? (Y/N): "
if /i "!INSTALL_MK!"=="Y" (
    copy /y "_pack\MoonKing.dll" "Mods\MoonKing.dll" >nul
    echo  [OK] MoonKing installed.
) else (
    echo  [ ] MoonKing skipped.
)
echo.

:: ── SafePassage ──
echo  --------------------------------------------------
echo   SAFEPASSAGE
echo   D-pad Up toggles random encounters on/off.
echo  --------------------------------------------------
set /p INSTALL_SP="  Install SafePassage? (Y/N): "
if /i "!INSTALL_SP!"=="Y" (
    copy /y "_pack\SafePassage.dll" "Mods\SafePassage.dll" >nul
    echo  [OK] SafePassage installed.
) else (
    echo  [ ] SafePassage skipped.
)
echo.

:: ── SkipIntro ──
echo  --------------------------------------------------
echo   SKIPINTRO
echo   Skip Atlus logos, intro video, and
echo   "Press Start" screen. Straight to title.
echo  --------------------------------------------------
set /p INSTALL_SI="  Install SkipIntro? (Y/N): "
if /i "!INSTALL_SI!"=="Y" (
    copy /y "_pack\SkipIntro.dll" "Mods\SkipIntro.dll" >nul
    echo  [OK] SkipIntro installed.
) else (
    echo  [ ] SkipIntro skipped.
)

:: ── Done ──
echo.
echo  ============================================
echo   Installation complete!
echo   Launch the game normally through Steam.
echo  ============================================
echo.
echo   To uninstall a mod, delete its .dll from
echo   the Mods folder. To disable temporarily,
echo   rename .dll to .dll.disabled
echo.
echo  All mods are the work of sevrL_bats - thanks!
echo.
pause
