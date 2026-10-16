# GodMode — a cheat / sandbox mod for *Casualties: Unknown*

A [BepInEx 5](https://github.com/BepInEx/BepInEx) plugin that gives you god mode, flight, max skills, a full-status-cleanse heal, teleport, fullbright — and a big searchable **spawn-anything** window (items, creatures, monsters, plants, structures, terrain blocks), with the spawn point chosen by you.

[中文](README.md) · **English**

---

> ### ⚠️ Please read first
> - **Single-player only.** This is a cheat mod. Do not use it in multiplayer / co-op sessions.
> - **Any crash, glitch or save corruption caused by this mod is the mod's fault — not the game developer's.** Please don't report mod-caused problems to Orsoniks.
> - The game is in **beta**, and it *will* update. A game update can break any mod, including this one. See [Compatibility](#compatibility).
> - This repository contains **no game files**. It ships source code plus a compiled plugin that patches the game at runtime. You need to own the game.

---

## Features

| Key | Feature | What it does |
|:---:|---|---|
| `F1` | **God mode** | You cannot die. Health, blood, oxygen, hunger, thirst, stamina, temperature and every limb are held at full. Bleeding, infection, fractures, pain and dislocations are cleared continuously. |
| `F2` | **Flight / noclip** | Free flight through terrain. Hold `LeftShift` for 4× speed. Soft-clamped to the world border so you can't fly out and crash the game. |
| `F3` | **Max skills** | Strength / Resistance / Intelligence maxed. |
| `F4` | **Cleanse** | Full heal **plus** removal of every negative status effect. |
| `F5` | **Teleport** | Teleport to the mouse cursor. |
| `F6` | **Fullbright** | Full map lighting. |
| `F7` | **Starter kit** | Grants the starting loadout. |
| `F8` | **Spawn anything** | Opens the spawn window — see below. |

Every key is rebindable, and every feature can be toggled off in the config file.

### The spawn window (`F8`)

Spawns **anything in the game**, pulled live from the game's own registries — so a game update that adds new items or creatures shows up automatically, with no mod update needed.

- **Categories** — Everything / Items / Creatures / Plants / Structures / Blocks. Items are sub-categorised using the game's own item categories. Sub-categories are cycled with `◀ ▶` or `Tab`.
- **Search** — press `/` and type.
- **Spawn position** — three modes: at your feet, at the mouse, or **at a point you pick yourself**. In pick mode you click anywhere in the world; an on-screen marker shows the exact world coordinates, and you can fine-tune with the arrow keys (hold `Shift` for 0.25-tile steps). Every spawn position is clamped inside the world bounds.
- **Batch spawn** — "spawn all filtered", "random ×5", "random ×20", spread over a grid and queued a few per frame.
- **Real names** — display names come from the game's own localisation tables, so you see "Ceiling Rye", not `ceilingrye`.
- **Item condition, auto-pickup** — adjustable before spawning.

A full description is in [docs/USAGE.en.md](docs/USAGE.en.md).

---

## Installation

**Requirements**

- *Casualties: Unknown* (Steam appid `4576510`)
- [BepInEx 5.4.23.x **x64**](https://github.com/BepInEx/BepInEx/releases) — the 64-bit build

**Steps**

1. **Install BepInEx.** Download `BepInEx_win_x64_5.4.23.*.zip` and extract it into the game's root folder — the one containing `CasualtiesUnknown.exe` (in Steam: right-click the game → *Manage* → *Browse local files*). You should end up with `winhttp.dll` and a `BepInEx` folder next to the game executable.
2. **Run the game once**, then close it. This lets BepInEx create `BepInEx\plugins\` and `BepInEx\config\`.
3. **Install the mod.** Drop `GodMode.dll` into:
   ```
   <game folder>\BepInEx\plugins\
   ```
4. **Play.** Load into a run and press `F8`. The window should appear in the top-left corner.

> **Upgrading?** Close the game *completely* before overwriting `GodMode.dll`. While the game is running the DLL is memory-mapped and Windows will refuse the copy with *"The process cannot access the file because it is being used by another process"* — that is expected, not a broken download.

**Linux / Steam Deck (Proton):** add this to the game's launch options:
```
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

### Verifying it loaded

Open `<game folder>\BepInEx\LogOutput.log` and search for `GodMode`. A healthy startup looks like:

```
[Info   :BepInEx] Loading [GodMode 1.0.0]
[Info   :GodMode] [GodMode] hooked: PlayerCamera.Update
...  (12 hook lines in total)
[Info   :GodMode] [GodMode] loaded v1.0.0
[Info   :GodMode] [GodMode] watchdog started
[Info   :GodMode] [GodMode][spawner] loaded: 284 items + 102 entities + 36 blocks = 422 entries
[Info   :GodMode] [GodMode][ui] uGUI window created, font = ...
```

If there are **no `[GodMode]` lines at all**, the DLL is in the wrong place or BepInEx isn't installed. If a line says `hook failed` or `method not found`, the game has updated and the mod needs fixing — please [open an issue](../../issues) with the log.

---

## Configuration

Created on first launch at:

```
<game folder>\BepInEx\config\casualtiesunknown.godmode.cfg
```

Common options:

```ini
[Keys]
GodMode = F1                 # rebind anything here
FlyNoClip = F2
MaxSkills = F3
FullHeal = F4
TeleportToMouse = F5
Fullbright = F6
StarterKit = F7
ItemSpawner = F8

[Options]
StartWithGodMode = true      # turn god mode on automatically when a run starts
StartWithFly = false
ShowHud = true               # on-screen status text
Watchdog = true              # recover automatically if the main thread stalls >8s
CrashGuard = true            # pull out-of-bounds NPCs back into the world
WorldMargin = 12             # tiles of margin kept when flying near the border

# ---- spawn window (F8) ----
SpawnPosMode = 2             # 0 = at player, 1 = at mouse, 2 = a point you pick
SpawnCondition = 1           # condition of spawned items, 0..1
SpawnAutoPickup = true
SpawnBatchPerFrame = 15
SpawnMaxBatch = 800
PauseWhileOpen = true        # freeze game time while the window is open
DumpInventory = true         # export the spawn list to BepInEx\config for classification reports
```

> If you want to report a **mis-categorised** entry, keep `DumpInventory = true`, open the window once, and attach `<game folder>\BepInEx\config\GodMode.spawnlist.log` (columns: `category | prefab | components`).

---

## Building from source

You need the game installed (the compiler references its assemblies) and Windows PowerShell 5.1+ / PowerShell 7. No SDK, no NuGet, no Visual Studio — it uses the C# compiler that ships with Windows.

```powershell
git clone https://github.com/xianyun66666/Casualties-Unknown-GodMode.git
cd <repo>
pwsh -File .\build.ps1
```

The script locates the game automatically via the Steam registry and `libraryfolders.vdf`. Override it if needed:

```powershell
pwsh -File .\build.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
# or set it once:
setx CASUALTIES_UNKNOWN_PATH "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
```

By default the DLL is written straight into `BepInEx\plugins\`. **While the game is running that write fails** — build to a staging path instead and copy it over after closing the game:

```powershell
pwsh -File .\build.ps1 -OutPath .\dist\GodMode.dll
```

The project deliberately targets **C# 5** (`/langversion:5`) because it is compiled with the legacy `csc.exe` from the .NET Framework. So: no `?.`, no string interpolation, no expression-bodied members, no `nameof`.

---

## Compatibility

| | |
|---|---|
| Game | *Casualties: Unknown* (Steam appid `4576510`) |
| Tested on | build `24774057` (beta) |
| BepInEx | 5.4.23.4 (5.4.23.x should all work) |
| Unity | 2022.3.62, Mono |
| Platform | Windows x64 (Linux/Proton untested but should work) |

**This mod is version-sensitive.** It works by patching named methods of the game's assembly, so a game update that renames or reshapes those methods can break individual features (they fail soft — you get a log line and that feature stops working, the rest keeps running). If that happens, please open an issue with your `LogOutput.log`.

It only **reads** the game's localisation tables and object registries; it never redistributes game content.

### Known limitations

- The spawn list's category for a given object is **inferred from which components it carries**, so a few entries will inevitably be mis-categorised. Reports with the exported list are welcome.
- Terrain blocks are placed into the world (that's a wall), not dropped as items.
- `Time.timeScale` is frozen while the spawn window is open (configurable) so clicking the UI doesn't also swing your weapon.

---

## Credits

- **Orsoniks** — *Casualties: Unknown*. All game content belongs to them.
- **[BepInEx](https://github.com/BepInEx/BepInEx)** and **[HarmonyX](https://github.com/BepInEx/HarmonyX)** — the modding framework this is built on.
- The UI hit-testing approach is modelled on the in-game console patterns used by other community mods.

## License

[MIT](LICENSE) — do whatever you like with the code. The game itself is not covered by this licence and is not distributed here.
