using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    public sealed class SecondLawGame : MonoBehaviour
    {
        public ProgressionService Progression { get; private set; }
        public QuestDefinition CurrentQuest { get; private set; }
        public CharacterStats PlayerBaseStats { get; private set; }
        public CharacterStats SlimeBaseStats { get; private set; }

        private GuildUiController guildUi;
        private HudController hud;
        private BattleController battle;
        private List<SkillDefinition> skills;

        private void Awake()
        {
            skills = DemoData.CreateUniformValkyrieSkills();
            Progression = new ProgressionService(skills);
            PlayerBaseStats = DemoData.CreateUniformValkyrieStats();
            SlimeBaseStats = DemoData.CreateSlimeStats();
            CurrentQuest = DemoData.CreateFirstQuest();
        }

        private void Start()
        {
            EnsureCamera();
            ShowGuild();
        }

        public void StartQuest()
        {
            if (battle != null)
            {
                Destroy(battle.gameObject);
            }

            if (guildUi != null)
            {
                guildUi.Hide();
            }

            hud = HudController.Create(this);
            battle = BattleController.Create(this, CurrentQuest);
        }

        public void FinishBattle(bool victory)
        {
            if (hud != null)
            {
                Destroy(hud.gameObject);
                hud = null;
            }

            if (battle != null)
            {
                Destroy(battle.gameObject);
                battle = null;
            }

            if (victory)
            {
                IReadOnlyList<string> rewards = Progression.ApplyQuestRewards(CurrentQuest);
                ShowGuild(rewards);
            }
            else
            {
                ShowGuild(new[] { "Retreated from battle. No rewards granted." });
            }
        }

        public void ResetProgress()
        {
            Progression.Reset();
            ShowGuild(new[] { "Demo progress reset." });
        }

        public IReadOnlyList<SkillDefinition> Skills()
        {
            return skills;
        }

        private void ShowGuild(IReadOnlyList<string> rewardMessages = null)
        {
            EnsureCamera();
            if (guildUi == null)
            {
                guildUi = GuildUiController.Create(this);
            }

            guildUi.Show(rewardMessages);
        }

        private static void EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.transform.position = new Vector3(0f, 0.2f, -10f);
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
        }
    }
}
