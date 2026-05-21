using UnityEngine;

namespace SecondLaw
{
    [CreateAssetMenu(menuName = "Second Law/Skill Definition")]
    public sealed class SkillDefinition : ScriptableObject
    {
        public string skillId;
        public string displayName;
        [TextArea] public string description;
        public int requiredLevel = 1;
        public SkillInputCommand inputCommand;
        public SkillEffectType effectType;
        public float cooldownSeconds = 0.3f;
        public int staminaCost = 0;
        public int ammoCost = 0;
        public float damageMultiplier = 1f;
        public float range = 1.2f;
        public float depthTolerance = 0.45f;
        public float knockback = 1.5f;
        public float stunSeconds = 0.2f;

        public bool IsUnlocked(int level)
        {
            return level >= requiredLevel;
        }
    }
}
