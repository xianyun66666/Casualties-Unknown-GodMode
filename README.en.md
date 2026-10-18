# GodMode — a cheat / sandbox mod for *Casualties: Unknown*

A [BepInEx 5](https://github.com/BepInEx/BepInEx) plugin that gives you god mode, flight, max skills, a full-status-cleanse heal (including **drug overdose and withdrawal** — and severed limbs are reattached well enough to hold a gun again), teleport, fullbright, **one-hit kill** — and a big searchable **spawn-anything** window (items, creatures, monsters, plants, structures, terrain blocks) with **categories fine enough to actually find things**, the spawn point chosen by you, **keys you can rebind right inside the window**, and **the character frozen while the window is open**.

[中文](README.md) · **English**

**⬇️ [Download the latest release](https://github.com/xianyun66666/Casualties-Unknown-GodMode/releases/latest)** (plugin + illustrated install guide + usage manual + source) · [Install steps](#installation)

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
| `F4` | **Cleanse** | Full heal **plus** removal of every negative status effect — bleeding, infection, fractures, dislocations, pain, **drug overdose / withdrawal / stimulants / caffeine**. Severed limbs are reattached too (otherwise a healed arm still cannot hold a gun). |
| `F5` | **Teleport** | Teleport to the mouse cursor. |
| `F6` | **Fullbright** | Full map lighting. |
| `F7` | **Starter kit** | Grants the starting loadout. |
| `F8` | **Spawn anything** | Opens the spawn window — see below. |
| `F9` | **One-hit kill** | One swing destroys anything: creatures, monsters, plants, structures, blocks. |

Every key is rebindable — **click "Key bindings" in the window** (see below) or edit the config file. Every feature can be toggled off.

### One-hit kill (`F9`)

Press `F9`, then swing at anything once:

| What you hit | Result |
|---|---|
| Creature / monster / spider / trader | Dies in one hit |
| Plant (ceiling rye, cactus, glowshroom…) | Shatters in one hit |
| Structure / fixture (door, workbench, recharging station…) | Shatters in one hit |
| Block (rock, ore, gravel…) | Shatters in one hit |

Melee, firearms, bare hands — anything that deals damage through the attack key.

Implementation is a single Harmony prefix on `Body.Attack(AttackInfo atk, int slot)`. That method is the one
funnel for every attack the player makes (`PlayerCamera.HandleAttacks` → `Body.UseItemInHand` → `Body.Attack`),
and it dispatches by target: blocks go to `WorldGeneration.DamageBlock(atk.structuralDamage)`, structures to
`BuildingEntity.health -= atk.structuralDamage`, creatures to `BuildingEntity.health -= atk.damage`. So the
prefix just multiplies both damage fields by a factor (default `100000`) — **one hook covers blocks, structures
and creatures**.

- **Player only:** NPC-on-NPC fights keep vanilla damage, so the world doesn't fall apart on its own.
- The factor is configurable, **but don't go past one million**: damage numbers go through an `(int)` cast and overflow into negatives.

### Key bindings

The spawn window has a **"Key bindings"** button on the first row and a **"Reset keys"** button on the second.

- Left-click a row → it turns into `◀ press a new key…` → press a key → applied instantly and written back to the cfg.
- All 9 actions are rebindable: god mode / flight / max skills / cleanse / teleport / fullbright / starter kit / spawner / one-hit kill.
- **Conflicts are allowed but flagged loudly:** the row turns red and shows `⚠ conflicts with "XXX"`. Both actions will fire on that key (that's just how Unity input works) — click the row again to pick another key.
- **Hotkeys are temporarily suppressed while rebinding**, otherwise rebinding god mode to `F1` would toggle it on that very press.
- The mod does **not** touch the game's own key system (`Settings` / `KeyBinds` / `SettingKeybind`); it stores keys in its own cfg and leaves your saves alone.

### The spawn window (`F8`)

Spawns **anything in the game**, pulled live from the game's own registries — so a game update that adds new items or creatures shows up automatically, with no mod update needed.

- **Pick a group, then pick a category** — row 1 is Everything / Items / Creatures / Plants / Structures / Blocks / Other. Choosing a group drops you onto a **category grid**: each cell is one concrete category with a live count, and clicking a cell narrows the list to it. The row-2 category label jumps back to the grid at any time.
- **Fine enough to be useful** — items split into ammo / wearables (armour · clothing) / weapons (guns · melee) / medical (dressings · medicine · drugs) / food / drinks / containers / junk / tools / materials; creatures into monsters / traders / animals / NPCs; plants into mushrooms / trees / vines / groundcover / crops / flowers; structures into doors & walls / workbenches / power / storage / medical / defence / furniture / crystal; blocks into seven material classes. Classification is component-first with name keywords as a fallback, so a sand rose is no longer filed as a building.
- **Search** — press `/` and type. **Typing a keyword ignores the current group and category** and searches the whole list, so you can find a monster while the Items group is selected.
- **The character is frozen while the window is open** — movement, aiming and attacks are all skipped, so clicking the UI no longer walks you off a cliff. Set `FreezeInputWhileOpen = false` to keep walking with the menu up.
- **Spawn position** — three modes: at your feet, at the mouse, or **at a point you pick yourself**. In pick mode you click anywhere in the world; an on-screen marker shows the exact world coordinates, and you can fine-tune with the arrow keys (hold `Shift` for 0.25-tile steps). Every spawn position is clamped inside the world bounds.
- **Batch spawn** — "spawn all filtered", "random ×5", "random ×20", spread over a grid and queued a few per frame.
- **Real names** — display names come from the game's own localisation tables, so you see "Ceiling Rye", not `ceilingrye`.
- **Item condition, auto-pickup** — adjustable before spawning.

A full description is in [docs/USAGE.en.md](docs/USAGE.en.md).

---

## Installation

**Download**

Grab a prebuilt package from [**Releases → latest**](https://github.com/xianyun66666/Casualties-Unknown-GodMode/releases/latest) — it contains `GodMode.dll`, a step-by-step install guide, the full usage manual, and the source. To build it yourself see [Building from source](#building-from-source).

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
[Info   :BepInEx] Loading [GodMode 1.2.2]
[Info   :GodMode] [GodMode] hooked: PlayerCamera.Update
...  (17 hook lines in total)
[Info   :GodMode] [GodMode] loaded v1.2.2
[Info   :GodMode] [GodMode] watchdog started
[Info   :GodMode] [GodMode][spawner] loaded: 284 items + ... entities + ... blocks = ... entries
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
FreezeInputWhileOpen = true  # freeze the character's controls (move/aim/attack); false = walk with the menu up
DumpInventory = true         # export the spawn list to BepInEx\config for classification reports
```

> If you want to report a **mis-categorised** entry, keep `DumpInventory = true`, open the window once, and attach `<game folder>\BepInEx\config\GodMode.spawnlist.log` (columns: `category | id | display name | extra`).

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
