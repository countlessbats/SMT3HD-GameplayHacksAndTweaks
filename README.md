# SMT3HD Gameplay Hacks and Tweaks

A pick-and-choose MelonLoader mod suite for **Shin Megami Tensei III: Nocturne HD Remaster**.

The packaged installer asks about each mod independently, so players can install only the pieces they want. Version `2.3.9` includes SafePassage `1.0.5`, which keeps alternate-layout L3 exclusive to encounter toggling without a native global-input hook, alongside Prey Eyes 2 `2.5.8`, keyboard arrow-key alternatives, and resolution-scaled BuffView.

## Included Mods

| Mod | Function |
| --- | --- |
| Prey Eyes 2 | Adds a color-coded targeting reticle, affinity board, ailment resistance display, Cathedral of Shadows fusion-preview affinity display, and resolution-scaled START-toggle BuffView. |
| SuspendSafe | Press R3 to write a suspend/quicksave without returning to the title screen (F4 in the alternate layout). |
| KeptSuspense | Keeps a suspend save available after loading it instead of consuming it permanently. |
| QuickPass | Adds controller shortcuts in battle: R1 passes the current demon's turn, L1 attempts escape. |
| MoonKing | Press D-pad Down or keyboard Down Arrow to toggle between Full Moon and New Moon (R3 or Down Arrow in the alternate layout). |
| SafePassage | Press D-pad Up or keyboard Up Arrow to toggle random encounters on and off (L3 or Up Arrow in the alternate layout). |
| SkipIntro | Skips the Atlus logos, intro movie, and Press Start screen. |

Press F3 to toggle the alternate layout: SafePassage moves from D-pad Up to L3, MoonKing moves from D-pad Down to R3, and SuspendSafe moves from R3 to F4. While SafePassage is installed and the alternate layout is active, L3 no longer invokes the game's screenshot UI-hide function. Press F3 again to restore the default layout and vanilla L3 behavior. This works with any subset of the three mods; the keyboard arrow bindings remain active in both layouts, and the layout resets when the game closes.

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

These are standard C#/.NET MelonLoader mods. The source projects reference only the local MelonLoader assemblies and the IL2CPP assembly stubs generated for the local SMT3 HD install. The repository does not require any private SDK.

1. Clone the repository:

   ```powershell
   git clone https://github.com/sevrlbats/SMT3HD-GameplayHacksAndTweaks.git
   cd SMT3HD-GameplayHacksAndTweaks
   ```

2. Confirm the game has generated the expected dependency folders:

   ```text
   C:\Program Files (x86)\Steam\steamapps\common\smt3hd\MelonLoader\net6\
   C:\Program Files (x86)\Steam\steamapps\common\smt3hd\MelonLoader\Il2CppAssemblies\
   ```

3. Build every included mod from the repository root:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\build_all.ps1 -GameDir "C:\Program Files (x86)\Steam\steamapps\common\smt3hd"
   ```

4. Or build one mod manually with `dotnet`:

   ```powershell
   & "C:\Program Files\dotnet\dotnet.exe" build ".\source\PreyEyes2\PreyEyes2.csproj" -c Release /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\smt3hd"
   ```

Each compiled DLL is written under that mod's `source\<ModName>\bin\Release\net6.0\` folder. To install a compiled DLL manually, copy it into the game's `Mods\` folder.

For example, after building Prey Eyes 2:

```text
source\PreyEyes2\bin\Release\net6.0\PreyEyes2.dll
```

copy that DLL to:

```text
C:\Program Files (x86)\Steam\steamapps\common\smt3hd\Mods\PreyEyes2.dll
```

## Notes For Malware Review

The release zip includes prebuilt DLLs in `_pack\` so normal players can install without compiling. The source for those DLLs is in `source\`.

`install_modpack.bat` performs these actions:

- Creates `Mods\` if needed.
- Installs MelonLoader `0.6.1` if `version.dll` is not already present.
- Prompts the user for each included mod.
- Copies the selected DLLs from `_pack\` into `Mods\`.
- Copies Prey Eyes 2 icon assets from `_pack\icons\` into `Mods\icons\` when Prey Eyes 2 is selected.

The installer does not download payloads at install time. The old `FramerateUnlock` / FPS delimiter mod is intentionally not included.

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
