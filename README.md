# TSP — Theo Score Points

A [Celeste](https://www.celestegame.com/) / [Everest](https://everestapi.github.io/) code
mod that adds a scoring system rewarding you for keeping **Theo** close to Madeline.

## Why

Theo gameplay is fun, but most of the time you can throw Theo far ahead and grab him later,
or leave him behind entirely in some rooms. TSP pushes the opposite: it makes it worthwhile
to keep Theo *with* you at all times, which naturally forces riskier, more interesting and
more watchable play (finding strats to carry Theo through hard sections instead of ditching
him).

## Scoring model — the "ratchet"

The scoring is built so that **it can never be farmed**. Every earlier attempt at an
accumulating score (points per frame while Theo is near) could be gamed by just moving
around a lot near Theo. The current model is the opposite — a one-way ratchet:

- **Each room starts at 100 points.**
- The room score can **only go down, never up.** There is nothing to farm: moving around
  near Theo merely *preserves* your score, it never increases it; moving away from Theo
  bleeds it.
- **Continuous bleed:** while Theo is farther than the *near radius*, the score drains
  continuously, proportional to how far he is (up to the *far radius*) and to elapsed time.
  Bringing Theo back close **stops** the bleed but never refunds what was lost.
- **Theo held ⇒ zero distance ⇒ zero loss.** Carry Theo the whole room and you keep 100.
- **Arming:** the ratchet only starts once you've picked Theo up at least once in the room.
  This avoids unfairly bleeding you when Theo spawns far away and you have to go fetch him.
  A room where Theo is never picked up scores nothing (it isn't banked).
- **Death** resets the current room back to 100 (fresh attempt). The **best** score per room
  is kept, so retrying can only improve a room, never farm it.
- The chapter total is the sum of the best score of each room, banked on room transition
  (and on chapter complete for the last room).

### Tuning (Mod Options)

| Option | Meaning | Default |
| --- | --- | --- |
| Enabled | Master toggle | on |
| Near Radius | Distance (px, 8px = 1 tile) under which Theo counts as "close" (no bleed) | 48 |
| Far Radius | Distance at which the bleed is maximal | 160 |
| Bleed Rate | Points lost per second at maximal separation | 25 |
| HUD Display | Hidden / Total only / Total + current room | Total + room |

## Code architecture

| File | Role |
| --- | --- |
| [`TspModule.cs`](TspModule.cs) | `EverestModule` entry point. Subscribes to Everest events (`Level.OnLoadLevel`, `OnTransitionTo`, `OnComplete`, `Player.OnDie`) and hooks `Level.Update` to drive the tracker each frame. |
| [`ScoreTracker.cs`](ScoreTracker.cs) | The scoring core (the ratchet). Holds the current room score, arms on first Theo pickup, bleeds while Theo is far, and banks the best score per room. |
| [`ScoreHud.cs`](ScoreHud.cs) | HUD entity (`Tags.HUD | Tags.Global`). Draws `TSP : <total>` and the live room score, colored red→pink (`#e5bebb`) by remaining room score. |
| [`TspSettings.cs`](TspSettings.cs) | `EverestModuleSettings` — the Mod Options above. |
| [`TspSession.cs`](TspSession.cs) | `EverestModuleSession` — per-room best scores, persisted across save & quit; resets when the chapter is restarted. |
| [`Dialog/`](Dialog) | English / French labels for the Mod Options menu. |

### Event → scoring flow

1. `OnLoadLevel` → `ScoreTracker.Reset` (room score back to 100) and spawns the HUD.
2. `Level.Update` hook → `ScoreTracker.Update` each frame: find Theo, arm on pickup, bleed
   the score based on Theo's distance.
3. `OnTransitionTo` / `OnComplete` → `ScoreTracker.Bank`: keep the best score for the room
   just left, then reset.
4. `OnDie` → `ScoreTracker.Reset`: current room back to 100, chapter total untouched.

## Building

Requires the .NET SDK (the mod targets `net8.0`, matching the .NET build of Everest).

```sh
dotnet build
```

The project references `Celeste.dll`, `MMHOOK_Celeste.dll` and `FNA.dll` from your Celeste
install via the `CelestePrefix` MSBuild property (defaults to
`~/.steam/steam/steamapps/common/Celeste`). Override it if your install lives elsewhere:

```sh
dotnet build -p:CelestePrefix="/path/to/Celeste"
```

A post-build step copies `TSP.dll`, `everest.yaml` and the `Dialog/` files into
`<CelestePrefix>/Mods/TSP/`, so a build is all you need before launching the game with
Everest.

## Status

Scoring logic is implemented and compiles; in-game validation (Chapter 5 / Mirror Temple,
the Theo rooms) is the remaining step.
