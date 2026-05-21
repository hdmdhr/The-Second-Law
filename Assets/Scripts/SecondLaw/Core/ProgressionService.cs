using System;
using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    [Serializable]
    public sealed class ProgressionState
    {
        public int level = 1;
        public int experience;
        public int gold;
        public int reputation;
        public int talentPoints;
        public int affection;
    }

    public sealed class ProgressionService
    {
        private const string SaveKey = "SecondLaw.Progression.v1";

        public ProgressionState State { get; private set; }
        public IReadOnlyList<SkillDefinition> Skills { get; private set; }

        public ProgressionService(IReadOnlyList<SkillDefinition> skills)
        {
            Skills = skills;
            Load();
        }

        public bool IsSkillUnlocked(SkillDefinition skill)
        {
            return skill != null && State.level >= skill.requiredLevel;
        }

        public SkillDefinition FindSkill(SkillInputCommand command)
        {
            for (int i = 0; i < Skills.Count; i++)
            {
                if (Skills[i].inputCommand == command)
                {
                    return Skills[i];
                }
            }

            return null;
        }

        public CharacterStats BuildPlayerStats(CharacterStats baseStats)
        {
            CharacterStats stats = baseStats.CloneRuntime();
            stats.level = State.level;
            stats.experience = State.experience;
            stats.gold = State.gold;
            stats.reputation = State.reputation;

            if (State.level >= 2)
            {
                stats.jumpHeight *= 1.4f;
            }

            if (State.level >= 4)
            {
                stats.reloadSeconds *= 0.75f;
            }

            if (State.level >= 6)
            {
                stats.moveSpeed *= 1.3f;
            }

            return stats;
        }

        public List<string> ApplyQuestRewards(QuestDefinition quest)
        {
            List<string> messages = new List<string>();
            State.experience += quest.rewardExperience;
            State.gold += quest.rewardGold;
            State.reputation += quest.rewardReputation;
            messages.Add("+" + quest.rewardExperience + " EXP");
            messages.Add("+" + quest.rewardGold + " Gold");
            messages.Add("+" + quest.rewardReputation + " Guild Reputation");

            while (State.level < 10 && State.experience >= ExperienceForNextLevel(State.level))
            {
                State.experience -= ExperienceForNextLevel(State.level);
                State.level++;
                State.talentPoints++;
                messages.Add("Level Up! Lv" + State.level + " / Talent Point +" + 1);
            }

            Save();
            return messages;
        }

        public void ApplyReply(LetterTemplate letter)
        {
            State.affection += Mathf.Max(0, letter.affectionReward);
            Save();
        }

        public int ExperienceForNextLevel(int level)
        {
            return 80 + level * 45;
        }

        public void Reset()
        {
            State = new ProgressionState();
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(LetterCacheService.CachePrefix + "slime_thanks");
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                State = JsonUtility.FromJson<ProgressionState>(PlayerPrefs.GetString(SaveKey));
            }

            if (State == null)
            {
                State = new ProgressionState();
            }
        }

        private void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(State));
            PlayerPrefs.Save();
        }
    }
}
