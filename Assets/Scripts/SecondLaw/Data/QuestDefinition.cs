using UnityEngine;

namespace SecondLaw
{
    [CreateAssetMenu(menuName = "Second Law/Quest Definition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string questId = "slime_request";
        public string title = "F-Rank Request: Slime Cleanup";
        public string clientName = "Mina, Guild Receptionist";
        [TextArea] public string description = "Three slimes are blocking the herb road outside town.";
        public string targetMonsterId = "slime";
        public int targetCount = 3;
        public int rewardExperience = 140;
        public int rewardGold = 35;
        public int rewardReputation = 4;
        public LetterTemplate completionLetter;
    }
}
