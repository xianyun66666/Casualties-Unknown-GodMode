# Changelog

All notable changes to this mod. This project follows [Semantic Versioning](https://semver.org/).

---

## [1.2.0] — 2026-10-18 — dismemberment fix, drug cleanup, finer categories

Two long-standing bugs that really hurt, plus a rebuilt spawn window that groups things
by category instead of one endless list.

### Fixed: no guns after losing an arm

**Symptom**: once an arm was bitten or cut off, healing it — no matter how many times, no
matter how full the health bars went — never brought the gun back up. Melee felt limp too.

**Root cause**: `Limb.Dismember()` does more than set `dismembered = true`; it also calls
`gameObject.SetActive(false)` — **the limb object itself is deactivated**. "Full heal" only
flipped `dismembered` back to `false` and refilled `skinHealth` / `muscleHealth`, and never
re-enabled the object. At engine level that arm still did not exist, so aiming, holding a
gun and swinging all failed.

**Fix**: healing now also calls `SetActive(true)` and clears `broken` / `dislocated` /
`splinted` plus the bone-heal and dislocation timers. The limb was deactivated, not
destroyed, so prosthetics and armour attached to it survive.

### Fixed: drug side effects would not clear

**Symptom**: after Full Heal, "Overdose", "Withdrawal", "Stimulated" and "Energized" stayed
on screen — and a while later you were shaking, losing blood and seeing stars again.

**Root cause**: none of that state lives on `Body`. `overdose1~4` and `withdrawal1~4` come
straight from `Painkillers.actualOpiateReception` (>50 / >20 / >5 is overdose,
<-5 / -15 / -25 / -34 is withdrawal), `drugoverdose` reads `Body.overdoseIndex > 3`,
`stimulated` reads `Body.onHardStimulants`, `energized` reads `Body.caffeinated > 0`.
None of it was ever cleared.

**Fix**: Full Heal now zeroes all five `Painkillers` values (`opiateAmount` /
`opiateTolerance` / `opiateReception` / `antagonistAmount` / `actualOpiateReception`) and
resets `onHardStimulants` / `caffeinated` / `stimulantMultiplier` / `overdoseIndex` /
`focusedLevel`, along with neural-booster residue, desensitisation and vomiting.

### Improved: finer categories

The old window was one flat list, and the game only buckets items into 8 coarse categories
(`custom` alone holds dozens). Now:

- **Row 1** picks a top-level group: All / Items / Creatures / Plants / Structures / Blocks / Other.
- **Picking a group lands you on a category grid** — each cell is one concrete category with a
  live count; click a cell to narrow down to it.
- **The row-2 category label** jumps back to the grid at any time.
- Items split into `ammo` / `wearables (armour · clothing)` / `weapons (guns · melee · other)` /
  `medical (dressings · medicine · drugs)` / `food` / `drinks` / `containers` / `junk` /
  `tools` / `materials`.
- Creatures split into `monsters` / `traders` / `animals` / `NPCs`; plants into `mushrooms` /
  `trees` / `vines` / `groundcover` / `crops` / `flowers`; structures into `doors & walls` /
  `workbenches` / `power` / `storage` / `medical` / `defence` / `furniture` / `crystal`;
  blocks into seven material classes.
- Classification is **component-first** (corpses, crystal enemies, traders and spiders each go
  their own way); name keywords are only a fallback. So a sand rose is no longer filed as a
  building, and a manifested crystal enemy is no longer filed as decoration.

### Fixed: the character still moved while the window was open

**Symptom**: clicking in the window made the character walk, punch and shoot.

**Root cause**: only `Time.timeScale` was frozen, but player input is read in
`PlayerCamera.HandleInput()`, which does not care about time scale at all.

**Fix**: a prefix on `HandleInput` skips the whole method while the window is open (movement,
aiming and attacks all at once), plus `body.moveDir` is zeroed as a safety net. Set
`FreezeInputWhileOpen = false` if you would rather keep walking with the menu up.

### Fixed: things could not be found in the window

Two causes, both fixed:

1. **Search was trapped inside the current group** — looking for a monster while the Items
   group was selected found nothing. Typing a keyword now ignores group and category and
   searches the whole list.
2. **The list itself was incomplete** — entities only matched `Item` / `BuildingEntity`, which
   missed everything deriving from `UsableObject` (furniture, terminals and so on). Block
   probing tolerance went from 256 to 512, so sparse block ids are no longer truncated.

### Notes

- New config key `FreezeInputWhileOpen` (default `true`). Existing configs need no edits;
  missing keys are filled in automatically.
- The inventory dump log header changed to
  `category | id | display name | extra` — for items the extra column is the game's own
  category, for entities it is the component list. If something looks mis-filed, paste that
  line into an issue.

---

## [1.1.0] — 2026-10-17 — one-hit kill + rebindable keys

### New feature

| Key | What it does |
|---|---|
| `F9` | **One-hit kill** — one swing destroys anything: creatures, monsters, plants, structures, blocks |

- **How it works:** a single Harmony prefix on `Body.Attack(AttackInfo atk, int slot)`. That method is the
  single funnel for every attack the player makes
  (`PlayerCamera.HandleAttacks` → `Body.UseItemInHand` → `Body.Attack`), and it dispatches by what you hit:
  - block hit → `WorldGeneration.DamageBlock(..., atk.structuralDamage, ...)`
  - structure hit → `BuildingEntity.health -= atk.structuralDamage`
  - creature hit → `BuildingEntity.health -= atk.damage`

  So the prefix just multiplies `atk.damage` and `atk.structuralDamage` by a factor (default `100000`).
  **One hook covers blocks, structures and creatures at once.** `atk` is a throwaway object created per
  attack, so nothing leaks into your save.
- **Player only:** the prefix checks `__instance == PlayerCamera.main.body`. NPC-on-NPC fights keep vanilla damage.
- **The prefix returns `void`, not `false`** — returning `false` would skip the original method, i.e. you could
  no longer swing at all.
- **Factor is capped at one million:** damage numbers go through an `(int)` cast, and tens of millions overflow
  into negative values. Tune it with `[Options] OneHitMultiplier`.

### New feature: rebind keys inside the window

The spawn window gains a **"Key bindings"** button on the first row and a **"Reset keys"** button on the second.

- All 9 actions (god mode / flight / skills / cleanse / teleport / fullbright / starter kit / spawner / one-hit kill)
  are rebindable: left-click a row → it turns into `◀ press a new key…` → press a key → applied instantly and
  written back to the cfg.
- **Conflicts are visible and fixable:** binding a duplicate is allowed, but that row turns **red** and shows
  `⚠ conflicts with "XXX"`, plus a message at the top.
- **Hotkeys are suppressed while rebinding** — otherwise rebinding god mode to `F1` would toggle it on that very press.
- **The key page reuses the same 16 row widgets** as the spawn list, so no extra UI objects were created.
- **The game's own key system is left alone.** The game does ship `Settings` / `KeyBinds` / `SettingKeybind`
  (`PlayerCamera.HandleAttacks()` reads the attack key via `KeyBinds.GetBind("attack")`), but that is the game's
  own save data — touching it means touching saves. The mod stores its keys in
  `BepInEx\config\casualtiesunknown.godmode.cfg` instead.

### Other

- Key names shown in the top-left status line are now read dynamically: rebind `F1` to `G` and it shows `[G off]`.
- Pressing `F8` always reopens the spawn list instead of staying on the key page.

---

## [1.0.0] — 2026-10-16 — first public release

This is the first public release. Everything described below was built and tested in
private builds numbered `0.1.0` – `0.9.0`; the version was renumbered to `1.0.0` for
publication. The development history is kept at the bottom of this file.

**Tested against:** Casualties: Unknown Demo, Steam build `24774057` (Unity 2022.3.62),
BepInEx `5.4.23.4`.

### Features

| Key | What it does |
|---|---|
| `F1` | God mode — health, blood, oxygen, hunger, thirst, stamina, energy, consciousness, all limbs; blocks dismemberment and death |
| `F2` | Flight / noclip, clamped to the world border |
| `F3` | Max out all skills |
| `F4` | Full heal **and** cleanse — removes bleeding, infection, fractures, dislocations, pain, embedded shrapnel |
| `F5` | Teleport to the mouse cursor |
| `F6` | Fullbright |
| `F7` | Starter kit |
| `F8` | **Spawn window** — spawn anything in the game, sorted into categories, at a position you choose |

All hotkeys are rebindable in `BepInEx\config\casualtiesunknown.godmode.cfg`.

### Spawn window

- Builds its list at runtime from three in-game sources, so content added by a game
  update shows up automatically:
  1. `Item.GlobalItems` — items (284 at the time of writing)
  2. `Resources.LoadAll<GameObject>("")` filtered by the `Item` / `BuildingEntity`
     components — the exact same source the game's own console `spawn` command autocompletes
     from (102 entities)
  3. `WorldGeneration.GetBlockInfo(ushort)` probed by incrementing the id — blocks
     (36 blocks; blocks are `UInt16` ids in a world array, not prefabs)
- Categories: Items / Creatures / Plants / Structures / Blocks, with sub-categories.
- Display names come from the game's localisation tables (`Locale.GetBuilding` /
  `GetOther` / `GetItem`), so they match the language the game is running in.
- Three spawn-position modes: at the player, at the mouse, or at a point you click in the
  world. Pick mode draws a live marker with the exact world coordinates; arrow keys nudge it
  (`Shift` for 0.25-tile steps).
- Batch spawn (`x5` / `x20` / `All`) runs through a queue so the frame rate survives.
- Every spawn writes its id, name, category and coordinates to the log.
- The full list is exported to `BepInEx\config\GodMode.spawnlist.log`.

### Stability work

- **Crash guard for out-of-bounds NPCs.** The game's `TraderScript.OnWillRenderObject`
  indexes the world fluid array without clamping (`fluid[v.x, v.y] = 0`, while the game's own
  `SetLiquid` does clamp). A trader that wanders past the map edge throws
  `IndexOutOfRangeException` *during render culling*, which wedges the render pipeline and
  hangs the game with the main thread spinning at 100%. The mod skips that call when the
  block position is out of bounds and walks the NPC back inside — including its `desiredPos`
  field, otherwise it drifts straight back out.
- **Flight bounds.** Flying is clamped to the world border, with a configurable margin.
- **Watchdog thread.** If the main thread stops advancing for more than 8 seconds, skill
  values are forced back to safe numbers and a diagnostic line is written to
  `BepInEx\config\GodMode.stall.log`.
- **Skill/XP clamp.** Hooks on `Skills.AddExp`, `Skills.CheckForLevelUp` and
  `Skills.GetExperienceForLevel` — the game's XP curve overflows at very high skill levels,
  which sent `CheckForLevelUp` into an infinite loop.
- **Spawn positions are always clamped to the world.** Spawning outside the world made the
  game choke on chunk and fluid lookups every frame.
- **Time-freeze self-healing.** `PauseWhileOpen` freezes `Time.timeScale` so clicking the
  window does not swing your fist; if the window is closed while still frozen, it is restored
  automatically.

### Notes

- **Single-player only.** There is a co-op mod for this game; god mode in co-op is cheating.
- The mod never redistributes game files — it only reads them at compile time.
- The plugin's own component never receives `Update` / `OnGUI` in this game (both counters
  stay at 0), and neither the game nor the reference mods declare any `OnGUI` method to hook.
  The window is therefore built in **uGUI** (`Canvas` + `TextMeshProUGUI`) — Canvas rendering
  does not depend on our component's `Update` — and all mouse/keyboard interaction is
  hand-rolled in a postfix patch on `ConsoleScript.Update`, which is proven to run every
  frame.

---

## Development history (pre-release builds)

### [0.9.0] — spawn point picker, real names, freeze fix
#### Added
- Three spawn-position modes and click-to-place with a live world marker (arrow-key nudging,
  `Shift` for fine steps).
- Names resolved from `Locale.GetBuilding` / `GetOther` / `GetItem`; item sub-category keys
  translated too.
#### Fixed
- **Spawning a few objects could freeze the game.** The old "at mouse" mode converted the raw
  cursor position to a world position — but when you click a row in the window, the cursor is
  *on the window*, so the coordinate landed outside the world. All spawn positions are now
  clamped, and batch spawns log their base point.
- **Spawned objects had no name.** `BuildingEntity.fullName` is empty on prefabs; the game
  only fills it in `Start()` after instantiation.
- **Mis-categorised entries.** Plants (ceiling rye, hydreed, sand rose, pop fruit, frigiant,
  xaloris, cactus, bouncecap) were filed as structures; medical station and recharging station
  were filed as creatures because they carry a `Talker` (`Talker` is no longer a criterion);
  crystal clusters were filed as crystal enemies (they are decorative); `reinforcedrope` was
  filed as "other" although it is an item.
- `GodMode.spawnlist.log` was written to `BepInEx\` instead of `BepInEx\config\`
  (`Paths.ConfigPath` ends with a backslash, so `GetDirectoryName` stripped one level too many).

### [0.8.0] — spawn anything, with categories
#### Added
- The spawn window grew from items-only to **everything**, sorted into categories.
- Runtime list building from the three in-game sources listed above.

### [0.7.0] — uGUI rewrite
#### Changed
- The spawn window was rebuilt in uGUI. The previous IMGUI implementation could never
  appear (see the note under 1.0.0).
- Added `PauseWhileOpen`.

### [0.6.0] — full cleanse
#### Changed
- The heal key now heals *and* removes bleeding, infection, fractures, dislocations, pain and
  other negative status effects, instead of only healing.

### [0.5.0] — watchdog
#### Added
- Watchdog thread and skill/XP clamping hooks.
#### Fixed
- Freeze when gaining XP.

### [0.4.0] — crash guard
#### Added
- Crash guard for out-of-bounds traders, and flight bounds clamping.

### [0.3.0]
#### Fixed
- Plugin loaded but no key did anything.

### [0.1.0] — initial
- God mode, flight, max skills, heal, teleport, fullbright, starter kit.

---

[1.0.0]: https://github.com/xianyun66666/Casualties-Unknown-GodMode/releases/tag/v1.0.0
