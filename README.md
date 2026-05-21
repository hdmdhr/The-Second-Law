# Second Law Unity Demo

Unity prototype for **《转生公会：第二法则》**.

This repo is intentionally script-first: placeholder sprites, combat objects, the guild UI, quest flow, rewards, and the cached letter/reply loop are created at runtime. Open the folder with Unity `6000.4.7f1` or newer, press Play in any empty scene, and the demo bootstraps itself.

## Controls

- `A/S/D/W`: move on the LF2-style `x/y` arena plane
- `J`: basic attack
- `K`: jump
- `L`: guard / skill starter
- `Space`: Lv3 Pistol Shot
- `Q`: ultimate, currently mapped to Lv10 Machine Gun
- `L > W > K`: Lv1 Uppercut
- `L > D/A-forward > Space`: Lv5 Burst Fire
- `L > S > K`: Lv7 Grapple Knockdown
- `L > D/A-forward > K`: Lv9 Shining Wink
- `L > W > J`: Lv10 Machine Gun
- `L > K > J`: configurable shortcut slot, currently mapped to Lv1 Uppercut
- `R`: manual reload

## Implemented Scope

- Single-player local demo
- Uniform Valkyrie job from Lv1-Lv10, excluding the undefined `花容凶器`
- One slime enemy species with close-range AI
- Pseudo-3D depth-axis melee checks
- Ammo, stamina, reload, knockback, stun, charm, death, and victory states
- Guild quest UI, battle launch, reward settlement, level-up flow
- Cached local letter generation and choice-based reply
- Runtime placeholder art and UI

## Unity Notes

The demo does not require hand-made scenes. For convenience, the editor menu includes:

- `Second Law/Create Bootstrap Scene`
- `Second Law/Reset Demo Progress`

The first menu item creates `Assets/Scenes/SecondLawDemo.unity`, but the runtime bootstrap works in a new empty scene too.

## Next Steps

1. Replace generated placeholder sprites with AI-generated CG and character sheets.
2. Tune combat timing after playing on keyboard for at least 10 minutes.
3. Split letter templates into external JSON or ScriptableObject assets.
4. Add a real cached-AI provider behind the existing local letter cache.
