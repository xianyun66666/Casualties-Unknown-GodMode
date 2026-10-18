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
│ [Everything][Items][Creatures][Plants][Structures][Blocks][Other] [Search][Clear][Keys][Close]
│ [◀][  ▼ Everything (click to pick a category)  ][▶][= all]  [Reset][PgUp][PgDn]  │
│ [Position: pick a point][Pick][-10%][+10%][Auto-pick][All][R5][R20][Stop] │
└───────────────────────────────────────────────────────────────────────────┘
```

Picking a group (say **Items**) switches the title to **"Category overview"** and turns the list
into category cells:

```
┌───────────────────────────────────────────────────────────────────────────┐
│ Spawner — category overview — pick a group (Items)                        │
│ Every sub-category under "Items". Click one to show only that category.   │
├───────────────────────────────────────────────────────────────────────────┤
│ ▶ Items/Weapons/Guns                            24      Items/Weapons/Guns│
│   Items/Weapons/Melee                           31      Items/Weapons/Melee│
│   Items/Ammo                                    11      Items/Ammo         │
│   ...                                                              (16 rows)
├───────────────────────────────────────────────────────────────────────────┤
│ [Everything][Items][Creatures][Plants][Structures][Blocks][Other] [Search][Clear][Keys][Close]
│ [◀][  ◀ Back to list: Everything  ][▶][= all]              [Reset][PgUp][PgDn]  │
│ [Position: pick a point][Pick][-10%][+10%][Auto-pick][All][R5][R20][Stop] │
└───────────────────────────────────────────────────────────────────────────┘
```

> The **"Keys"** button on the first row replaces this list with the 9 action bindings — see
> [the "Key bindings" section of the README](../README.en.md#key-bindings). This document covers
> the spawn window; the key page merely borrows the same area.

> **The character is frozen while the window is open.** Movement, aiming and attacks are all
> skipped, so clicking the UI no longer drags you around. Set `FreezeInputWhileOpen = false` in
> the config to walk with the menu up.

## Categories

### Groups (row 1)

| Group | Contains |
|---|---|
| **Everything** | everything, sorted by name |
| **Items** | every pickable item, split into ammo / wearables (armour · clothing) / weapons (guns · melee · other) / medical (dressings · medicine · drugs) / food / drinks / containers / junk / tools / materials |
| **Creatures** | monsters, traders, animals, NPCs (`Creatures/Monster`, `Creatures/Trader`, `Creatures/Animal`, `Creatures/NPC`) |
| **Plants** | mushrooms, trees, vines, groundcover, crops, flowers (rye, sand roses and cacti land in `Plants/Crops` or `Plants/Flowers`) |
| **Structures** | doors & walls, workbenches, power, storage, medical, defence, furniture, crystal, installations |
| **Blocks** | terrain blocks — these are **placed into the world**, not dropped; seven material classes |
| **Other** | corpses, interactable objects, and genuine odds and ends |

### Category overview (after picking a group)

Picking any group switches the window to the **category overview**: one cell per concrete
category, with a live count on the right.

- Click a row → only that category is shown, and the window returns to the normal list.
- In the normal list the row-2 label reads `▼ Items/Medical (click to pick a category)` —
  **click it to go straight back to the overview**.
- `Backspace` also toggles between the list and the overview.

### Sub-categories (row 2)

`◀` / `▶` cycle through the categories **of the current group**; the middle button
shows the current one. `= all` resets to no category filter. `Tab` also cycles.

### Search

Press `/` and type. **As soon as a keyword is entered, the group and category filters stop
applying** — the whole list is searched, so you can find a monster while the Items group is
selected. Clearing the search box brings the category filter back.

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
| Left-click a row | spawn that entry (in the category overview: **enter that category**) |
| Mouse wheel | page up/down |
| Row 1 buttons | groups (Everything/Items/Creatures/Plants/Structures/Blocks/Other) / search / clear filter / **keys** / close |
| Row 2 buttons | category ◀ ▶ / **category label (click to return to the overview)** / all / **reset keys** / previous page / next page |
| Row 3 buttons | position / pick / condition ∓10% / auto-pickup / spawn all / random ×5 / random ×20 / stop |
| `↑` `↓` | move the selected row ※ in pick mode the arrows nudge the spawn point instead — press `Esc` first |
| `Enter` | spawn the selected row (in the overview: enter that category) |
| `PageUp` `PageDown` `Home` `End` | paging |
| `Tab` | next category |
| `Backspace` | toggle between the list and the category overview |
| `/` | enter/leave the search box (type to filter, `Backspace` to delete; **search ignores group and category**) |
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
| `Resources.LoadAll<GameObject>("")` filtered to prefabs carrying an `Item`, `BuildingEntity` or `UsableObject` component | creatures, plants, structures, furniture and interactable objects |
| `WorldGeneration.GetBlockInfo(ushort)` probed from 0 upwards (stops after 512 consecutive gaps) | terrain blocks |

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

Categories are inferred **component-first, with name keywords as a fallback** — corpses go
through `CorpseScript`, crystal enemies through `CrystalEnemy`, decorative crystal through
`CrystalBehaviour`, traders through `TraderScript`, the spider family through `SpiderHandler` /
`SpiderHandlerTBE`; only what is left over is decided by name. Some entries will still be
wrong. With `DumpInventory = true` (the default), opening the window writes:

```
<game folder>\BepInEx\config\GodMode.spawnlist.log
```

with one line per entry, `category | id | display name | extra` (for items the extra column is
the game's own category, for entities it is the component list). Attach that file to an issue
and the rules can be corrected.

## Log lines

Every spawn writes a line with its coordinates:

```
[GodMode][spawner] spawned ceilingrye (Ceiling Rye) @ 12,-4  category=Plants
[GodMode][spawner] batch spawning 100, base point 12,-4 (mode=pick a point)
```

If something goes wrong, these tell you exactly where the object ended up.
