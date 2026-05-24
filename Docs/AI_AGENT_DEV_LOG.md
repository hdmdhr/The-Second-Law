# Second Law Unity Demo - AI Agent Dev Log

## Current State

- Unity project created in this repo for `6000.4.7f1`.
- Beginner-facing Notion architecture doc: https://www.notion.so/3674dfd23cc281c89859d68c73d13f3f
- Demo bootstraps itself at runtime through `SecondLawBootstrapper`; no hand-authored scene is required.
- Current playable loop: guild UI -> accept slime quest -> battle -> rewards/level progress -> cached client letter -> choice-based reply.
- Web guild prototype branch is active: `web/` contains a React + Vite + TypeScript + CSS Modules shell for the WebGL-first guild experience.
- Web guild assets in `web/public/assets/guild/` are symlinks to Unity Resources art/video files, avoiding duplicate large PNG/MP4 files while keeping browser-friendly URLs.
- Web prototype implements the guild hall, alpha-tested PNG hotspot hover/click, counter transition video, counter page, mock progression/rewards, client letter/reply choices, and placeholder pages for board/party/shop.
- Web/Unity bridge is stubbed in TypeScript with `startQuest`, `setLanguage`, `setSkipTransitions`, `unityReady`, and `battleFinished`; first pass uses a mock battle screen instead of a real Unity WebGL canvas.
- RPGUI is the chosen reference/library direction for the next Web guild styling pass; RPG CSS was rejected as too small/unproven for this project.
- Unity UI Toolkit guild remains available as the fallback path until the Web shell connects to real Unity WebGL combat.
- Job scope is Uniform Valkyrie Lv1-Lv10; undefined `花容凶器` is intentionally excluded.
- Enemy scope is one slime type with simple chase/attack AI.
- UI language defaults to Chinese and can toggle to English at runtime.
- Guild main UI now uses UI Toolkit for the guild hall entry, alpha-tested PNG mouse hotspots, orange outline hover feedback, and counter/placeholder pages.
- Guild hall uses `Assets/Resources/Art/Guild/demo-bg-0.png` as the entry background.
- Guild hall hotspots use same-size PNG masks in `Assets/Resources/Art/Guild/Hotspots/`; detection samples texture alpha so only non-transparent pixels are clickable.
- Hotspot pointer sampling accounts for UI Toolkit screen coordinates and `ScaleAndCrop` background fitting, avoiding the earlier vertical flip and offset between hover/click areas and the painted PNG masks.
- Hotspot overlay ignores UI Toolkit picking and the top bar is brought to the front, so guild language and skip-transition controls remain clickable.
- Counter hotspot plays `Assets/Resources/Video/Guild/lobby-to-desk.mp4` at 1.5x speed before opening the counter page, unless cutscenes are skipped.
- Counter transition now prepares the video first and waits for the first frame before hiding the guild UI, reducing the visible click-to-video flash.
- Unity Video, Audio, and Animation built-in modules are enabled in `Packages/manifest.json` for the counter transition video and imported animation assets.
- `Assets/Resources/UI/Guild/UnityDefaultRuntimeTheme.tss` supplies the runtime UI Toolkit default theme.
- Player/enemy visuals now use PNG spritesheets from Tiny RPG Character Asset Pack: Soldier for player, Orc for enemy placeholder.
- A small Boar animation subset from Fantasy Monsters is imported under `Assets/FantasyMonsters/Common/Animations/Boar/` for future enemy replacement experiments.
- `AGENTS.md` is the project guide for future coding agents.
- New Notion Web documentation lives under the RPG Second Law page:
  - Web architecture learning guide: https://www.notion.so/36a4dfd23cc28183b976da83114225ca
  - Web guild UI implementation spec: https://www.notion.so/36a4dfd23cc2815992c3d36aef883f0c

## Key Commits

- `0f694bd` Initial Unity demo prototype
- `68f6fff` Add runtime localization toggle
- `8d320f6` Use generated guild background
- `d58a77d` 添加公会热区入口原型
- `db0f08a` 修复柜台转场闪烁

## Important Files

- `Assets/Scripts/SecondLaw/Core/SecondLawBootstrapper.cs`: creates the runtime game object after scene load.
- `Assets/Scripts/SecondLaw/Core/SecondLawGame.cs`: top-level flow controller for guild, battle, rewards, reset.
- `Assets/Scripts/SecondLaw/Data/DemoData.cs`: hard-coded demo stats, quest, letter, and skill definitions.
- `Assets/Scripts/SecondLaw/Core/LocalizationService.cs`: Chinese/English runtime text table.
- `web/`: React/Vite Web guild prototype with mock data, local state, PNG hotspot sampling, and a stub Unity bridge.
- `Assets/Scripts/SecondLaw/UI/GuildUiController.cs`: UI Toolkit guild hall entry, hotspots, counter page, transition video, quest, reward, letter/reply.
- `Assets/Resources/UI/Guild/GuildHub.uxml`: UI Toolkit guild hall and counter page structure.
- `Assets/Resources/UI/Guild/GuildHub.uss`: UI Toolkit layout, hotspot, and panel styling.
- `Assets/Resources/Art/Guild/Hotspots/`: same-size PNG hotspot masks for the counter, request board, party table, and equipment/shop regions.
- `Assets/Resources/UI/Guild/UnityDefaultRuntimeTheme.tss`: runtime UI Toolkit theme import.
- `Assets/Resources/Video/Guild/lobby-to-desk.mp4`: current counter transition video.
- `Assets/FantasyMonsters/Common/Animations/Boar/`: imported Boar animation controller and clips from the Fantasy Monsters package.
- `Packages/manifest.json`: enables Unity Video/Audio/Animation modules for runtime video playback and imported animation assets.
- `Assets/Scripts/SecondLaw/UI/HudController.cs`: battle HUD and skill command display.
- `Assets/Scripts/SecondLaw/Combat/PlayerController.cs`: ASDW movement and combo input (`L > direction > action`).
- `Assets/Scripts/SecondLaw/Combat/BattleController.cs`: arena setup, player/slime spawning, hit queries, victory.
- `Assets/Scripts/SecondLaw/Combat/Combatant.cs`: HP/stamina/ammo, damage, guard, stun/charm, death.
- `Assets/Scripts/SecondLaw/Combat/SpriteSheetAnimator.cs`: runtime 100x100 PNG spritesheet slicing and simple loop/one-shot animation playback.
- `AGENTS.md`: project north star, verification checklist, and documentation rules.

## Current Controls

- `A/S/D/W`: move on pseudo-3D arena plane.
- `J`: attack.
- `K`: jump.
- `L`: guard and skill starter.
- `Space`: shoot.
- `Q`: ultimate.
- Skill pattern: `L`, then `W/S/forward`, then `J/K/Space`.
- Shortcut slot: `L > K > J`, currently mapped to Lv1 uppercut.

## Known Gaps

- Real Unity WebGL build/canvas is not connected to `web/` yet; the first Web pass uses a mock battle screen.
- Battle HUD is still generated by Canvas code; Unity fallback guild side uses UI Toolkit while the Web prototype uses React.
- Web guild title currently sits in the lower-left and can block the equipment hotspot; next pass should move it toward the screen center.
- Web hotspot feedback needs a more obvious orange highlight border.
- Clicking the counter currently has a brief flash before the transition video, and the Web video is muted.
- After the counter transition ends, the counter UI should overlay the video final frame instead of returning to the original static guild background.
- Only the counter hotspot has a real transition video; notice board, party table, and shop/equipment hotspots currently open placeholder pages.
- The imported Boar animation assets are not wired into the battle yet; current enemy visuals still use the existing Orc placeholder.
- Hotspot visuals are now PNG-mask based and clickable, but the exact mask art may still need hands-on cleanup in Photoshop if edges feel too broad or too narrow.
- No real animation, audio, title logo, save file UI, or AI API provider yet.
- Balance is placeholder and needs hands-on tuning in Unity.
- Localization uses in-code dictionaries; move to JSON/ScriptableObject later if text grows.
- Before each code/resource commit, update this log and the Notion beginner architecture doc.
- TODO: Soldier idle animation still appears to drift horizontally frame-by-frame in Unity. Likely cause is uneven frame alignment inside the source spritesheet; next fix should either pin idle to a single frame or add per-frame visual offsets/crop normalization in `SpriteSheetAnimator`.
