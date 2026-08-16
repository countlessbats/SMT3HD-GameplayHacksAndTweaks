SMT3HD GAMEPLAY HACKS AND TWEAKS V2.3.7
for Shin Megami Tensei III: Nocturne HD Remaster
by sevrL_bats

====================
 WHAT IS THIS?
====================

This is a pick-and-choose MelonLoader mod pack for SMT3 HD.

Each mod is independent. The installer asks about each one separately, so you
can install only the pieces you want.

====================
 WHAT CHANGED IN V2.3.7
====================

- Prey Eyes 2 no longer rebuilds the affinity-board layout on every target-
  cursor draw or recreates persistent board/sprite assets after every battle.
  This fixes runaway RAM use, freezes, and eventual crashes during skill use.
- The F3 alternate layout and keyboard Up/Down bindings remain available.
- BuffView remains twice as large at 1920x1080 and scales with resolution.

====================
 INCLUDED MODS
====================

PREY EYES 2 v2.5.8
  Color-coded targeting reticle and affinity board.
  Shows ailment resistances above the board.
  Shows fusion-preview affinities in the Cathedral of Shadows.
  Press START in battle to toggle the resolution-scaled BuffView.

SUSPENDSAFE
  Press R3 to quicksave into the suspend slot without being kicked back to the
  title screen. In the alternate F3 layout, press F4 instead.

KEPTSUSPENSE
  Keep your suspend save file after loading it.

QUICKPASS
  R1 quick-passes a demon's turn.
  L1 quick-attempts escape.

MOONKING
  D-pad Down or keyboard Down Arrow toggles Full/New Moon phase. In the
  alternate F3 layout, R3 replaces D-pad Down.

SAFEPASSAGE
  D-pad Up or keyboard Up Arrow toggles random encounters on/off. In the
  alternate F3 layout, L3 replaces D-pad Up.

KEYBIND LAYOUT
  Press F3 to toggle between the default and alternate layouts. This works with
  any combination of SuspendSafe, MoonKing, and SafePassage. Press F3 again to
  restore the defaults. The layout resets when the game closes.

SKIPINTRO
  Skip Atlus logos, intro video, and the Press Start screen.

====================
 INSTALL
====================

1. Extract this entire zip into your SMT3 HD game folder:

     Steam\steamapps\common\smt3hd\

2. Double-click:

     install_modpack.bat

3. Answer Y or N for each mod.

The installer will install MelonLoader v0.6.1 if MelonLoader is not already
present.

====================
 UNINSTALL
====================

Delete the relevant DLL from:

  Mods\

To disable temporarily, rename the DLL from:

  ExampleMod.dll

to:

  ExampleMod.dll.disabled

To fully remove MelonLoader, delete the MelonLoader folder and the following
files from the game root:

  version.dll
  dobby.dll

====================
 SOURCE
====================

Source for the included mods is bundled under:

  source\

The suite is open source under the MIT license. See:

  LICENSE

For GitHub-style documentation, including build instructions for modders, see:

  README.md

To compile every included mod from source, run this from the extracted folder:

  powershell -ExecutionPolicy Bypass -File .\build_all.ps1 -GameDir "C:\Program Files (x86)\Steam\steamapps\common\smt3hd"

Requirements:

  - SMT3 HD installed on Windows.
  - MelonLoader installed for SMT3 HD.
  - The game launched at least once with MelonLoader so dependency assemblies
    exist under MelonLoader\net6\ and MelonLoader\Il2CppAssemblies\.
  - .NET SDK 6 or newer.

To compile one mod manually, run a command like:

  "C:\Program Files\dotnet\dotnet.exe" build ".\source\PreyEyes2\PreyEyes2.csproj" -c Release /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\smt3hd"

Each compiled DLL is written under that mod's source\<ModName>\bin\Release\net6.0\
folder. Copy the desired DLL into Mods\ to install it manually.

For review: the installer creates Mods\ if needed, installs MelonLoader v0.6.1
only if version.dll is not already present, asks Y/N for each mod, copies the
selected DLLs from _pack\ to Mods\, and copies Prey Eyes 2 icons from _pack\icons\
to Mods\icons\ when Prey Eyes 2 is selected. It does not download payloads at
install time. FramerateUnlock / the FPS delimiter is not included.

====================

All mods are the work of sevrL_bats - thanks!
