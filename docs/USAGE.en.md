# Spawn window — full manual

[中文](USAGE.md) · **English**

Opened with `F8`. Requires being **inside a run** (not the main menu) — the block list and
world bounds can only be read once a world exists.

```
┌───────────────────────────────────────────────────────────────────────────┐
│ Spawner — 422 total / 422 matching / page 1/27                            │
│ Group: Everything   Sub: Everything (12 categories)   Search: -           │
│ Place: pick a point (12, -4)   Condition: 100%   Auto-pickup: on          │
├───────────────────────────────────────────────────────────────────────────┤
│ ▸ [Items/Medical] Defibrack                    (defibrack)                │
│   [Plants] Ceiling Rye                         (ceilingrye)               │
│   ...                                                              (16 rows)
├───────────────────────────────────────────────────────────────────────────┤
│ [Everything][Items][Creatures][Plants][Structures][Blocks] [Search][Clear][Keys][Close]
│ [◀][  Sub-category  ][▶][= all]                     [Reset keys][PgUp][PgDn]  │
│ [Position: pick a point][Pick][-10%][+10%][Auto-pick][All][R5][R20][Stop] │
└───────────────────────────────────────────────────────────────────────────┘
```

> The **"Keys"** button on the first row replaces this list with the 9 action bindings — see
> [the "Key bindings" section of the README](../README.en.md#key-bindings). This document covers
> the spawn window; the key page merely borrows the same area.

## Categories

### Groups (row 1)

| Group | Contains |
|---|---|
| **Everything** | everything, sorted by name |
| **Items** | every pickable item, sub-categorised by the game's own item category |
| **Creatures** | traders, animals, monsters (`Creatures/Trader`, `Creatures/Animal`, `Creatures/Monster`) |
| **Plants** | trees, mushrooms, vines, moss, rye, reeds, roses, cacti … |
| **Structures** | buildings and installations, plus `Structures/Crystal` (decorative mineral clusters) |
| **Blocks** | terrain blocks — these are **placed into the world**, not dropped |

### Sub-categories (row 2)

`◀` / `▶` cycle through the sub-categories **of the current group**; the middle button
shows the current one. `= all` resets to no sub-category filter. `Tab` also cycles.

## Spawn position

The first button in row 3 cycles three modes:

| Mode | Button | Spawns at |
|:---:|---|---|
| 0 | `Position: at player` | your feet |
| 1 | `Position: at mouse` | the world position under the cursor |
| **2** | **`Position: pick a point`** | **a point you choose** (default) |

### Picking a point

1. Click **`Pick`** — the button turns orange and reads `Picking…`.
2. Move the mouse over the world. An on-screen marker follows it, labelled with the current
   world coordinates.
3. **Left-click** to confirm. A message shows `spawn point set to X, Y`.
4. From then on, clicking any row spawns the object at that point.

| Input | Effect |
|---|---|
| `←` `→` `↑` `↓` | move the spawn point one tile |
| `Shift` + arrows | move it 0.25 tiles (fine adjustment) |
| `Esc` or **right-click** | cancel picking (the point stays where it was) |

The marker is a black box with a yellow dot; it turns **red** while picking. It is only
shown in "pick a point" mode.

> **Why this exists:** the previous version offered only "at mouse" / "at player". Clicking a
> row in the window means the cursor is necessarily *on the window*, so the world coordinate
> derived from it frequently landed **outside the map** — and objects spawned out there made
> the game freeze on every chunk/fluid lookup. Every spawn position is now clamped to the
> world bounds, so spawning out of bounds is impossible.

## Other controls

| Input | Effect |
|---|---|
| Left-click a row | spawn that entry |
| Mouse wheel | page up/down |
| Row 1 buttons | group / search / clear filter / **keys** / close |
| Row 2 buttons | sub-category ◀ ▶ / all / **reset keys** / previous page / next page |
| Row 3 buttons | position / pick / condition ∓10% / auto-pickup / spawn all / random ×5 / random ×20 / stop |
| `↑` `↓` | move the selected row ※ in pick mode the arrows nudge the spawn point instead — press `Esc` first |
| `Enter` | spawn the selected row |
| `PageUp` `PageDown` `Home` `End` | paging |
| `Tab` | next sub-category |
| `/` | enter/leave the search box (type to filter, `Backspace` to delete) |
| `Esc` | leave search; press again to close the window |
| `F8` | toggle the window |

## The key bindings page

Click **"Keys"** on the first row and the same window becomes the key page (a "Reset keys" button
appears on row 2):

```
┌───────────────────────────────────────────────────────────────────────────┐
│ Key bindings - click a row, then press the key you want (9 actions)        │
│ Left-click a row → press a new key → applied instantly, written to the cfg │
│ Conflicts are not blocked, but the row turns red and says "conflicts with…"│
├───────────────────────────────────────────────────────────────────────────┤
│ ▸ God mode          →   [F1]         (default F1)                          │
│   Flight / noclip   →   [F2]         (default F2)                          │
│   ...                                                             (9 rows) │
│   One-hit kill      →   [F9]         (default F9)                          │
└───────────────────────────────────────────────────────────────────────────┘
(The "⚠ conflicts with …" marker only appears once you actually bind two actions to the same key.)
```

| Input | Effect |
|---|---|
| Left-click a row | select it and wait for a key (the row shows `◀ press a new key…`) |
| Press any key | binds immediately, writes back to `BepInEx\config\casualtiesunknown.godmode.cfg`, status line updates |
| `Esc` while waiting | cancel this rebind |
| `Esc` (not waiting) | back to the spawn list |
| "Keys" button | toggle between the key page and the spawn list |
| "Reset keys" button | restore all 9 defaults (F1–F9) |
| Opening the window with `F8` | always starts on the spawn list, never stuck on the key page |

**Hotkeys are suppressed while rebinding** — otherwise rebinding god mode to `F1` would toggle it on
that very press. Only three buttons work on the key page: `Close` / `Keys` / `Reset keys`.

## Where the data comes from

The list is assembled at runtime from three sources, so new game content appears
automatically:

| Source | Yields |
|---|---|
| `Item.GlobalItems` | all items |
| `Resources.LoadAll<GameObject>("")` filtered to prefabs carrying an `Item` or `BuildingEntity` component | creatures, plants, structures |
| `WorldGeneration.GetBlockInfo(ushort)` probed from 0 upwards until it returns null | terrain blocks |

Spawned with the game's own `Utils.Create(id, pos, rot)` — the exact same code path as the
game's built-in `spawn` console command — falling back to instantiating the prefab the mod
already holds if the id is not under `Resources`. Blocks go through
`WorldGeneration.SetBlock`, which clamps internally.

## Where the names come from

All names come from the game's own localisation tables (`Locale`):

- items → `ItemInfo.fullName`
- creatures / plants / structures → `Locale.GetBuilding(id)`

`BuildingEntity.fullName` is **empty on prefabs** — the game only fills it in `Start()`
after instantiation, which is why earlier versions showed blank names.

## Reporting a mis-categorised entry

Categories are inferred from the components an object carries, so some entries will be
wrong. With `DumpInventory = true` (the default), opening the window writes:

```
<game folder>\BepInEx\config\GodMode.spawnlist.log
```

with one line per entry, `category | prefab | components`. Attach that file to an issue and
the rules can be corrected.

## Log lines

Every spawn writes a line with its coordinates:

```
[GodMode][spawner] spawned ceilingrye (Ceiling Rye) @ 12,-4  category=Plants
[GodMode][spawner] batch spawning 100, base point 12,-4 (mode=pick a point)
```

If something goes wrong, these tell you exactly where the object ended up.
