# Second Law Demo

Unity and Web prototype for **《转生公会：第二法则》**.

This repo is intentionally script-first: placeholder sprites, combat objects, the guild UI, quest flow, rewards, and the cached letter/reply loop are created at runtime. Open the folder with Unity `6000.4.7f1` or newer, press Play in any empty scene, and the demo bootstraps itself.

## Web Guild Prototype

The recommended first screen for the WebGL demo is now the React guild shell in `web/`.

```bash
cd web
npm install
npm run dev
```

Open the Vite URL, usually `http://127.0.0.1:5173/`. The Web prototype currently includes:

Use Node `20.19+` for the Web project. The repo includes `web/.nvmrc`; in Codex, the bundled Node 22 runtime can also run the build.

- guild hall background and alpha-tested PNG hotspot masks
- counter, request-board, table, and gear-shop transition videos; counter/request-board/table use looping backdrops and smooth video handoff
- independent looping BGM for counter, table, and gear-shop transitions/pages
- counter page with quest, reward, progression, client letter, and reply choices
- placeholder pages for the request board, party finder, and gear shop
- mock Unity bridge flow: accept request -> mock battle -> victory/retreat -> return to guild

The real Unity WebGL canvas is not connected yet. The existing Unity UI Toolkit guild remains as an Editor/Play Mode fallback until the Web shell can enter and return from real Unity combat.
The Web guild assets in `web/public/assets/guild/` are symlinks to the Unity `Assets/Resources` files, so images and video stay single-source during the prototype.

## Sharing The Web Prototype

The current Web prototype builds to static files, so it can be shared through GitHub Pages. The repository includes `.github/workflows/deploy-web.yml`, which installs the Web dependencies, builds `web/`, and publishes `web/dist` to Pages.

```bash
cd web
npm run build
```

The output is `web/dist/`. For GitHub Pages project URLs, Vite must generate asset paths under the repository name, so this repository builds with `VITE_BASE_PATH=/The-Second-Law/`. The workflow derives that path from the GitHub repository name; local development still uses `/`.

After pushing this branch, open the GitHub repository:

1. Go to `Settings -> Pages`.
2. Set `Build and deployment -> Source` to `GitHub Actions`.
3. Open `Actions -> Deploy Web Guild to GitHub Pages` and run it manually, or push to `main`.

GitHub Pages deployments are restricted by the `github-pages` environment. This workflow deploys from `main` by default. To deploy directly from an experimental branch, add that branch under `Settings -> Environments -> github-pages -> Deployment branches and tags`, or temporarily set the environment to allow all branches.

For this repository, the expected Pages URL is:

```text
https://hdmdhr.github.io/The-Second-Law/
```

Until the real Unity WebGL canvas is connected, this shared page will show the Web guild shell and mock battle flow only.

## Controls

- The demo starts in Chinese. Use the `English` / `中文` button in the guild or battle HUD to switch languages.
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

- React/Vite Web guild prototype for the WebGL-first path
- Single-player local demo
- Uniform Valkyrie job from Lv1-Lv10, excluding the undefined `花容凶器`
- One slime enemy species with close-range AI
- Soldier/Orc PNG spritesheet placeholders for the player and enemy visuals
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
