# SMT3HD Gameplay Hacks and Tweaks

A pick-and-choose MelonLoader mod suite for **Shin Megami Tensei III: Nocturne HD Remaster**.

The packaged installer asks about each mod independently, so players can install only the pieces they want. Version `2.3.4` replaces the old bundled Prey Eyes 2 build with current Prey Eyes 2 `2.5.5` and removes the old FPS delimiter. Use the Graphics Configurator mod for FPS unlocking instead.

## Included Mods

| Mod | Function |
| --- | --- |
| Prey Eyes 2 | Adds a color-coded targeting reticle, affinity board, ailment resistance display, Cathedral of Shadows fusion-preview affinity display, and START-toggle BuffView. |
| SuspendSafe | Press R3 to write a suspend/quicksave without returning to the title screen. |
| KeptSuspense | Keeps a suspend save available after loading it instead of consuming it permanently. |
| QuickPass | Adds controller shortcuts in battle: R1 passes the current demon's turn, L1 attempts escape. |
| MoonKing | Press D-pad Down to toggle between Full Moon and New Moon. |
| SafePassage | Press D-pad Up to toggle random encounters on and off. |
| SkipIntro | Skips the Atlus logos, intro movie, and Press Start screen. |

## Install

1. Download the release zip.
2. Extract the entire zip into the SMT3 HD game folder, usually:

   ```text
   Steam\steamapps\common\smt3hd\
   ```

3. Run:

   ```bat
   install_modpack.bat
   ```

4. Answer `Y` or `N` for each mod.

The installer installs MelonLoader `0.6.1` if MelonLoader is not already present.

## Uninstall

Delete the relevant DLL from:

```text
Mods\
```

To disable a mod temporarily, rename its DLL from `ExampleMod.dll` to `ExampleMod.dll.disabled`.

To fully remove MelonLoader, delete the `MelonLoader` folder plus these files from the game root:

```text
version.dll
dobby.dll
```

## Build From Source

Requirements:

- SMT3 HD installed on Windows.
- MelonLoader installed for SMT3 HD.
- The game launched at least once with MelonLoader so dependency assemblies exist under `MelonLoader\net6\` and `MelonLoader\Il2CppAssemblies\`.
- .NET SDK 6 or newer.

Build every included mod from the repository root:

```powershell
.\build_all.ps1 -GameDir "C:\Program Files (x86)\Steam\steamapps\common\smt3hd"
```

Build one mod manually:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" build ".\source\PreyEyes2\PreyEyes2.csproj" -c Release /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\smt3hd"
```

Each compiled DLL is written under that mod's `source\<ModName>\bin\Release\net6.0\` folder. To install a compiled DLL manually, copy it into the game's `Mods\` folder.

## Repository Layout

```text
_pack\                 Prebuilt DLLs and bundled runtime assets used by install_modpack.bat
source\                Source code for each included mod
install_modpack.bat    Pick-each-one installer
build_all.ps1          Convenience script for compiling every included mod
README.txt             Plain-text release instructions
README.md              GitHub documentation
LICENSE                MIT license for this suite
```

## Notes For Modders

The smaller gameplay tweaks are intentionally self-contained MelonLoader mods. Prey Eyes 2 is larger and carries its own detailed notes in `source\PreyEyes2\README.md`.

The old `FramerateUnlock` mod is not included in this suite anymore. Graphics Configurator is the preferred FPS-unlock path.

## License

MIT. See [LICENSE](LICENSE).
