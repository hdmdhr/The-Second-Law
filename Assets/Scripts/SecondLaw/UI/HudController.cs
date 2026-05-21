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
        }

        private void Update()
        {
            if (battle == null || battle.Player == null)
            {
                return;
            }

            Combatant player = battle.Player;
            statusText.text =
                "HP " + Mathf.CeilToInt(player.CurrentHealth) + "/" + player.Stats.maxHealth +
                "   Stamina " + Mathf.CeilToInt(player.CurrentStamina) + "/" + player.Stats.maxStamina +
                "   Ammo " + player.CurrentAmmo + "/" + player.Stats.maxAmmo + "\n" +
                "Lv " + game.Progression.State.level + "   Defeated " + battle.KillCount + "/" + game.CurrentQuest.targetCount +
                "   R reload / ASDW move";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Commands");
            builder.AppendLine("J Attack   K Jump   L Guard   Space Shoot   Q Ultimate");
            builder.AppendLine("Skills: L, then W/S/Forward, then J/K/Space. Shortcut: L>K>J.");
            for (int i = 0; i < game.Skills().Count; i++)
            {
                SkillDefinition skill = game.Skills()[i];
                string state = game.Progression.IsSkillUnlocked(skill) ? "OK" : "Lv" + skill.requiredLevel;
                builder.AppendLine(state + "  " + CommandLabel(skill.inputCommand) + "  " + skill.displayName);
            }

            skillText.text = builder.ToString();
        }

        private static string CommandLabel(SkillInputCommand command)
        {
            switch (command)
            {
                case SkillInputCommand.UpJump: return "L>W>K";
                case SkillInputCommand.Shoot: return "Space";
                case SkillInputCommand.ForwardShoot: return "L>Forward>Space";
                case SkillInputCommand.DownJump: return "L>S>K";
                case SkillInputCommand.ForwardJump: return "L>Forward>K";
                case SkillInputCommand.UpAttack: return "Q or L>W>J";
                case SkillInputCommand.Passive: return "Passive";
                default: return command.ToString();
            }
        }
    }
}
