# Robot Cafe

A Unity 2022.3.17f1 cozy cafe tycoon sim. The player is a robot running a space cafe — customizing the menu, arranging the layout, and serving robot customers each shift.

## Theme & Terminology

The cafe serves **fuel** (not coffee). Drinks are composed of **FuelTypes**: `Unleaded`, `Diesel`, `Premium`. Customers are robots. Keep this metaphor consistent in naming — use `FuelType`, fuel-themed drink names, etc.

## Project Structure

- `Assets/Scripts/Data/` — plain data types and constants (`FurnitureObject`, `GameConsts`, layout presets)
- `Assets/Scripts/Managers/` — singleton game managers (state, save, audio, UI, menu, orders, layout, planning, camera)
- `Assets/Scripts/Objects/` — in-world MonoBehaviours (player, customers, appliances, cups, delivery)
- `Assets/Scripts/UI/` — UI MonoBehaviours (menu editor, drink editor, layout editor, planning, shift-end, pause)
- `Assets/Scripts/Utility/` — small helpers (player interactions, rigidbody push)

## Game Loop

Four states managed by `StateManager` (singleton, event-driven):

1. **MainMenuState** — loads Start + LevelGeo scenes
2. **PlanningState** — player arranges cafe layout and edits the menu; day increments here, furniture costs deducted on exit
3. **ShiftState** — live gameplay; timed shift, player serves orders, pausable
4. **ShiftEndState** — tallies money and high score, shows results

State changes broadcast via `OnStateChanged` delegate. Pausability is per-state (`IsPausable`).

## Architecture Conventions

- **Singletons** — all managers expose a static `Instance`; never use `FindObjectOfType` to reach them
- **Event-driven coordination** — managers subscribe to `StateManager.OnStateChanged` and `StateManager.OnGamePausedChanged` rather than polling or calling each other directly
- **ScriptableObjects** — `FurnitureObject` is a ScriptableObject asset; furniture data lives in assets, not in scene objects or code
- **Serializable save types** — runtime types (e.g. `CafeElement`) have separate serializable counterparts for JSON; don't conflate the two

## Scene Structure

| Scene | Role |
|-------|------|
| `Start` | Main menu |
| `LevelGeo` | Persistent cafe geometry; always loaded |
| `Planning` | Layout + menu editors |
| `Shift` | Live gameplay (loaded additively on top of LevelGeo) |
| `Pause` | Overlay loaded during Shift |

`LevelGeo` is loaded once and persists across Planning and Shift. `Shift` is loaded additively on top of it.

## Save File

`Assets/SaveData/saveData.json` — JSON, version-checked (`CURRENT_SAVEDATA_VERSION = 1`). All read/write goes through `SaveManager`.

## Key Tools & Packages

- **FMOD** — audio
- **DOTween** — tweening
- **Cinemachine** — camera
- **TextMesh Pro** — UI text
- **URP** — render pipeline
- **Unity Input System** — player input

## Jira

Project key: `RC` on `fractalriver.atlassian.net`

## Obsidian Vault

Design notes, feature specs, milestones, and systems documentation live in the project's Obsidian vault at:

```
/Users/izelmoctezuma/Library/Mobile Documents/com~apple~CloudDocs/Robot Cafe
```

Top-level sections: `Design/`, `Features/`, `Milestones/`, `Systems/`, plus `Cafe Planning Preview.md`.

Read access to this path is pre-authorized in `.claude/settings.json` — no permission prompt needed.
