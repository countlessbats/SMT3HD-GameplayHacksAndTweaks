SMT3HD GAMEPLAY HACKS AND TWEAKS V2.3.4
for Shin Megami Tensei III: Nocturne HD Remaster
by sevrL_bats

====================
 WHAT IS THIS?
====================

This is a pick-and-choose MelonLoader mod pack for SMT3 HD.

Each mod is independent. The installer asks about each one separately, so you
can install only the pieces you want.

====================
 WHAT CHANGED IN V2.3.4
====================

- Replaced the old bundled Prey Eyes 2 build with current Prey Eyes 2 v2.5.5.
  This includes the current affinity board, reticle behavior, Cathedral display,
  START-toggle BuffView, packaged icon assets, and the post-KO board visibility
  fix.
- Removed FramerateUnlock. Use the Graphics Configurator mod for FPS unlocking
  instead.
- Preserved the automated/pick-each-one installer flow.

====================
 INCLUDED MODS
====================

PREY EYES 2 v2.5.5
  Color-coded targeting reticle and affinity board.
  Shows ailment resistances above the board.
  Shows fusion-preview affinities in the Cathedral of Shadows.
  Press START in battle to toggle BuffView.

SUSPENDSAFE
  Press R3 to quicksave into the suspend slot without being kicked back to the
  title screen.

KEPTSUSPENSE
  Keep your suspend save file after loading it.

QUICKPASS
  R1 quick-passes a demon's turn.
  L1 quick-attempts escape.

MOONKING
  D-pad Down toggles Full/New Moon phase.

SAFEPASSAGE
  D-pad Up toggles random encounters on/off.

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

Each compiled DLL is written under that mod's source\<ModName>\bin\Release\net6.0\
folder. Copy the desired DLL into Mods\ to install it manually.

====================

All mods are the work of sevrL_bats - thanks!
