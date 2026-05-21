using UnityEngine;

namespace SecondLaw
{
    [CreateAssetMenu(menuName = "Second Law/Character Stats")]
    public sealed class CharacterStats : ScriptableObject
    {
        public string displayName = "Adventurer";
        public int maxHealth = 100;
        public int maxStamina = 100;
        public int maxAmmo = 6;
        public float staminaRegenPerSecond = 18f;
        public float reloadSeconds = 2.4f;
        public float attackPower = 12f;
        public float defense = 0f;
        public float moveSpeed = 4.8f;
        public float jumpHeight = 1.1f;
        public int level = 1;
        public int experience = 0;
        public int gold = 0;
        public int reputation = 0;

        public CharacterStats CloneRuntime()
        {
            CharacterStats clone = CreateInstance<CharacterStats>();
            clone.displayName = displayName;
            clone.maxHealth = maxHealth;
            clone.maxStamina = maxStamina;
            clone.maxAmmo = maxAmmo;
            clone.staminaRegenPerSecond = staminaRegenPerSecond;
            clone.reloadSeconds = reloadSeconds;
            clone.attackPower = attackPower;
            clone.defense = defense;
            clone.moveSpeed = moveSpeed;
            clone.jumpHeight = jumpHeight;
            clone.level = level;
            clone.experience = experience;
            clone.gold = gold;
            clone.reputation = reputation;
            return clone;
        }
    }
}
