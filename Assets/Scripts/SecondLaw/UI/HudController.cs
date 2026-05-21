using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SecondLaw
{
    public sealed class HudController : MonoBehaviour
    {
        private SecondLawGame game;
        private Canvas canvas;
        private Text statusText;
        private Text skillText;
        private Text languageButtonLabel;
        private BattleController battle;

        public static HudController Create(SecondLawGame game)
        {
            GameObject root = new GameObject("Battle HUD");
            HudController hud = root.AddComponent<HudController>();
            hud.game = game;
            hud.Build();
            return hud;
        }

        public void Bind(BattleController controller)
        {
            battle = controller;
        }

        private void Build()
        {
            canvas = RuntimeUi.CreateCanvas("HUD Canvas");
            canvas.transform.SetParent(transform, false);

            RectTransform statusPanel = RuntimeUi.AddPanel(canvas.transform, "HUD Status", new Color(0.05f, 0.06f, 0.07f, 0.78f));
            RuntimeUi.SetRect(statusPanel, new Vector2(0.02f, 0.80f), new Vector2(0.43f, 0.97f), Vector2.zero, Vector2.zero);
            statusText = RuntimeUi.AddText(statusPanel, "Status", string.Empty, 20, Color.white);
            RuntimeUi.Stretch(statusText.rectTransform, 16f, 12f, 16f, 12f);

            RectTransform skillPanel = RuntimeUi.AddPanel(canvas.transform, "Skill Status", new Color(0.05f, 0.06f, 0.07f, 0.78f));
            RuntimeUi.SetRect(skillPanel, new Vector2(0.57f, 0.62f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero);
            skillText = RuntimeUi.AddText(skillPanel, "Skills", string.Empty, 17, Color.white);
            RuntimeUi.Stretch(skillText.rectTransform, 16f, 12f, 16f, 12f);

            Button language = RuntimeUi.AddButton(canvas.transform, "Language Toggle", LocalizationService.T("language.button"), new Color(0.18f, 0.22f, 0.28f));
            RuntimeUi.SetRect(language.GetComponent<RectTransform>(), new Vector2(0.44f, 0.90f), new Vector2(0.56f, 0.97f), Vector2.zero, Vector2.zero);
            languageButtonLabel = language.GetComponentInChildren<Text>();
            language.onClick.AddListener(LocalizationService.ToggleLanguage);
        }

        private void Update()
        {
            if (battle == null || battle.Player == null)
            {
                return;
            }

            Combatant player = battle.Player;
            statusText.text =
                LocalizationService.T("hud.hp") + " " + Mathf.CeilToInt(player.CurrentHealth) + "/" + player.Stats.maxHealth +
                "   " + LocalizationService.T("hud.stamina") + " " + Mathf.CeilToInt(player.CurrentStamina) + "/" + player.Stats.maxStamina +
                "   " + LocalizationService.T("hud.ammo") + " " + player.CurrentAmmo + "/" + player.Stats.maxAmmo + "\n" +
                LocalizationService.T("status.level") + " " + game.Progression.State.level + "   " + LocalizationService.T("hud.defeated") + " " + battle.KillCount + "/" + game.CurrentQuest.targetCount +
                "   " + LocalizationService.T("hud.reload_move");

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(LocalizationService.T("hud.commands"));
            builder.AppendLine(LocalizationService.T("hud.controls"));
            builder.AppendLine(LocalizationService.T("hud.combo_help"));
            for (int i = 0; i < game.Skills().Count; i++)
            {
                SkillDefinition skill = game.Skills()[i];
                string state = game.Progression.IsSkillUnlocked(skill) ? LocalizationService.T("hud.unlocked") : LocalizationService.T("status.level") + skill.requiredLevel;
                builder.AppendLine(state + "  " + CommandLabel(skill.inputCommand) + "  " + LocalizationService.SkillName(skill));
            }

            skillText.text = builder.ToString();
            if (languageButtonLabel != null)
            {
                languageButtonLabel.text = LocalizationService.T("language.button");
            }
        }

        private static string CommandLabel(SkillInputCommand command)
        {
            switch (command)
            {
                case SkillInputCommand.UpJump: return "L>W>K";
                case SkillInputCommand.Shoot: return "Space";
                case SkillInputCommand.ForwardShoot: return LocalizationService.CurrentLanguage == GameLanguage.English ? "L>Forward>Space" : "L>前方>Space";
                case SkillInputCommand.DownJump: return "L>S>K";
                case SkillInputCommand.ForwardJump: return LocalizationService.CurrentLanguage == GameLanguage.English ? "L>Forward>K" : "L>前方>K";
                case SkillInputCommand.UpAttack: return LocalizationService.CurrentLanguage == GameLanguage.English ? "Q or L>W>J" : "Q 或 L>W>J";
                case SkillInputCommand.Passive: return LocalizationService.T("hud.passive");
                default: return command.ToString();
            }
        }
    }
}
