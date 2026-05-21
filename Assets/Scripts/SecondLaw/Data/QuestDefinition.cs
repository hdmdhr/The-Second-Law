using UnityEngine;

namespace SecondLaw
{
    [CreateAssetMenu(menuName = "Second Law/Quest Definition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string questId = "slime_request";
        public string title = "F 级委托：清理史莱姆";
        public string clientName = "米娜，冒险者协会接待员";
        [TextArea] public string description = "三只史莱姆堵住了采药小路。";
        public string targetMonsterId = "slime";
        public int targetCount = 3;
        public int rewardExperience = 140;
        public int rewardGold = 35;
        public int rewardReputation = 4;
        public LetterTemplate completionLetter;
    }
}
