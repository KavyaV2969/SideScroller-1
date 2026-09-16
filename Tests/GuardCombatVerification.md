# Guard combat implementation and verification

Implemented and verified on 2026-09-10 in the connected Unity 6000.5.9f1 Editor, using the configured Unity CLI MCP server (`unity mcp`) and its live Pipeline connection.

## Controls and defaults

Hold Right Click to block. A blocked enemy hit stores one riposte; Left Click while holding Block consumes it. Without a stored riposte, Left Click commits to a parry attempt. Releasing Block clears the stored opportunity. Guard initiation and successful parries require ground contact.

Inspector values are on the scene's Player component:

| Value | Default |
|---|---:|
| Block damage reduction, physical and elemental | 0.75 (75%) |
| Riposte damage multiplier | 2 |
| Parry counter damage multiplier | 3 |
| Parry startup | 0.05 s |
| Parry active duration | 0.15 s |
| Missed parry recovery | 0.30 s |
| Parry region local offset | (0.8, 0) |
| Parry region radius | 0.85 |

The Inspector validates the multiplier ordering. Multipliers scale physical and elemental attack damage before armor/resistance mitigation; the existing critical-hit rules remain in use. Actual damage still depends on those stats.

## Integration

Four explicit `PlayerState` subclasses: Block, Parry, ParryRecovery, and CounterAttack. Riposte and successful parry share the counter state and clip, with a per-call damage multiplier. Parry timing uses gameplay seconds. The new Animator states follow the existing boolean Entry/Exit convention.

Block holds `PlayerGraphics_22`. Parry presents 23; counterattack runs 23 -> 24 -> 25, hitting on 24 through `attackTrigger` and completing through `currentStateTrigger`. A missed parry recovers through 24 -> 25, with no damage event. The requested sprite names use an uppercase P in the imported asset.

`EntityCombat` retains the existing circle overlap at an animation event. Attack states begin/end each swing; one resolution latch prevents repeated events or frames from resolving the same swing twice. Damageable owners are deduplicated across colliders. The enemy event checks parry-region overlap before damage, excludes successfully parrying players, then follows the existing damage path. `PlayerHealth` applies block mitigation and grants riposte only after an accepted, nonlethal blocked hit. Block suppresses its hit's knockback to preserve grounded guarding.

The active player was a scene object, with no player prefab in the asset database. Its original `EntityHealth` component was changed to `PlayerHealth`, preserving the component identity and serialized health settings. No prefab was created.

## Changed files and assets

- `Assets/InputSystem/PlayerInputSystem.inputactions` and its regenerated C# wrapper: Block action and Right Click binding.
- `Assets/Scripts/Player/Player.cs`, new `Player.Guard.cs`, and `PlayerHealth.cs`: guard ownership, settings, input/reset lifecycle and mitigation.
- New states: `PlayerBlockState.cs`, `PlayerParryState.cs`, `PlayerParryRecoveryState.cs`, `PlayerCounterAttackState.cs`.
- Existing `GroundedState`, `PlayerIdleState`, `PlayerMoveState`, `PlayerAttackState`, `PlayerJumpAttackState`: guard entry, early returns after state transitions, and swing lifecycle.
- `Assets/Scripts/Core/Entity/EntityCombat.cs`, `EntityAnimationTriggers.cs`, and `Assets/Scripts/Enemy/States/EnemyAttackState.cs`: ordered resolution and event integration.
- `Assets/Animations/Controllers/Player.controller` and four new clips under `Assets/Animations/Clips/Player/`: PlayerBlock, PlayerParry, PlayerParryRecovery, PlayerCounterAttack.
- `Assets/Scenes/SampleScene.unity`: PlayerHealth wiring and guard balance values only.
- New Unity scripts/clips include their generated `.meta` files.
- This verification note, the reusable live verification script, and the final assertion log under `Tests/`.

## Tests actually performed

Unity recompiled successfully. The final live Play Mode run passed all assertions with zero captured console errors. See `GuardCombatPlayModeResults.txt` for the individual assertions.

The run used virtual Mouse/Keyboard events through the real input action map, the real scene Player and Enemy, physics overlap queries, and their real Animator clips/events. A runtime-only platform and controlled enemy position made the tests repeatable. Critical rolls were disabled for damage comparisons. The enemy overlap method was invoked directly for precise timing and duplicate-event checks; a separate test advanced the real enemy Animator across its attack event to verify that path too.

Verified Block pose and input; mitigation for physical plus elemental damage; one stored riposte; release clearing; riposte consumption; active parry startup/window and region mismatch; automatic counterattack; zero damage from a successful parry; repeated same-swing suppression across colliders/events/frames; vulnerable miss recovery and cancellation lockout; multiplier reset and normal damage after counters; movement/input disable, component disable/re-enable and death during counter; movement, jump, aerial guard rejection, dash/gravity restoration, jump attack landing/completion and the full three-hit normal combo.

Temporary input settings/devices were restored, Play Mode was exited, then the verified scene/controller/clips were saved through the Editor. Runtime fixture objects and stat changes were not saved. Animator clips, sprite references, action binding and health component identity were checked before saving.

## Re-run

With this scene open, enter Play Mode, then invoke through the connected Pipeline:

```powershell
unity command eval_file Tests/GuardCombatLiveVerification.cs
```

Wait for `COMPLETE` in `Temp/GuardWork/playmode-results.txt`, inspect the console, and exit Play Mode. The verifier moves/kills the runtime player and changes runtime stats, so use a fresh Play Mode session each run.

## Scope

Enemy attack regions remain instantaneous event-driven overlap circles. No persistent hitbox framework was introduced. Parry attempts commit on Attack and stop providing block mitigation immediately; releasing Block does not cancel the committed attempt/recovery. Counters have no added invulnerability. No aerial guard, stamina, posture, directional block, UI, VFX or audio was added. Dialogue movement-disable integration was tested through its existing API; full quest/dialogue, wall-jump traversal and build/platform regression suites were not run.
