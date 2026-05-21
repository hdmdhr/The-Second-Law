using UnityEngine;

namespace SecondLaw
{
    [CreateAssetMenu(menuName = "Second Law/Talent Definition")]
    public sealed class TalentDefinition : ScriptableObject
    {
        public string talentId;
        public string displayName;
        [TextArea] public string description;
        public int requiredLevel = 1;
        public float moveSpeedBonus;
        public float damageBonus;
        public float staminaRegenBonus;
        public float reloadSpeedBonus;
    }
}
