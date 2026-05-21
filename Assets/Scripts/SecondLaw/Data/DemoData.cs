using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    public static class DemoData
    {
        public static CharacterStats CreateUniformValkyrieStats()
        {
            CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();
            stats.displayName = "Uniform Valkyrie";
            stats.maxHealth = 135;
            stats.maxStamina = 100;
            stats.maxAmmo = 6;
            stats.staminaRegenPerSecond = 18f;
            stats.reloadSeconds = 2.4f;
            stats.attackPower = 13f;
            stats.defense = 1f;
            stats.moveSpeed = 4.8f;
            stats.jumpHeight = 1.1f;
            return stats;
        }

        public static CharacterStats CreateSlimeStats()
        {
            CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();
            stats.displayName = "Slime";
            stats.maxHealth = 38;
            stats.maxStamina = 0;
            stats.maxAmmo = 0;
            stats.attackPower = 8f;
            stats.defense = 0f;
            stats.moveSpeed = 2.3f;
            return stats;
        }

        public static QuestDefinition CreateFirstQuest()
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "f_rank_slime_cleanup";
            quest.title = "F-Rank Request: Slime Cleanup";
            quest.clientName = "Mina, Guild Receptionist";
            quest.description = "Three slimes are blocking the herb road. Clear them out, then return to the guild counter for your reward.";
            quest.targetMonsterId = "slime";
            quest.targetCount = 3;
            quest.rewardExperience = 140;
            quest.rewardGold = 35;
            quest.rewardReputation = 4;
            quest.completionLetter = CreateSlimeLetter();
            return quest;
        }

        public static LetterTemplate CreateSlimeLetter()
        {
            LetterTemplate letter = ScriptableObject.CreateInstance<LetterTemplate>();
            letter.templateId = "slime_thanks";
            letter.senderName = "Mina";
            letter.body =
                "To our newest F-rank adventurer,\n\n" +
                "The herb road is open again. The apothecary sent a basket of mint leaves and said the town smells a little braver today.\n\n" +
                "You came back with mud on your shoes and slime gel on your cuffs, which means you did the work properly. The guild has recorded your first successful request.";
            letter.replyOpenings = new[]
            {
                "Dear Mina,",
                "To the Adventurers' Guild,",
                "Mina,"
            };
            letter.replyBodies = new[]
            {
                "I am glad the road is safe again. Please send my thanks to the apothecary.",
                "The slimes were tougher than expected, but this was a good first step.",
                "Next time, I will return with cleaner cuffs and a faster report."
            };
            letter.replyClosings = new[]
            {
                "Respectfully, your new F-rank adventurer.",
                "I will keep doing what I can.",
                "Yes adventure, but not too adventurous."
            };
            letter.affectionReward = 1;
            return letter;
        }

        public static List<SkillDefinition> CreateUniformValkyrieSkills()
        {
            return new List<SkillDefinition>
            {
                Skill("uppercut", "Lv1 上勾拳", "向前位移并向上击飞敌人。", 1, SkillInputCommand.UpJump, SkillEffectType.Uppercut, 0.65f, 12, 0, 1.15f, 1.2f, 0.45f, 2.4f, 0.45f),
                Skill("vital_spark", "Lv2 活力四射", "跳跃高度增加40%。", 2, SkillInputCommand.Passive, SkillEffectType.Passive, 0f, 0, 0, 0f, 0f, 0f, 0f, 0f),
                Skill("pistol_shot", "Lv3 手枪点射", "用左轮手枪发射子弹攻击敌人。", 3, SkillInputCommand.Shoot, SkillEffectType.Bullet, 0.35f, 0, 1, 1.05f, 7f, 0.35f, 1.2f, 0.12f),
                Skill("peony_stance", "Lv4 站如芍药", "站立不动时再装填弹夹加速25%。", 4, SkillInputCommand.Passive, SkillEffectType.Passive, 0f, 0, 0, 0f, 0f, 0f, 0f, 0f),
                Skill("burst_fire", "Lv5 火力四射", "连射4发子弹，击退敌人的同时靠后坐力向后位移。", 5, SkillInputCommand.ForwardShoot, SkillEffectType.BurstShot, 1.1f, 8, 4, 0.85f, 7f, 0.35f, 1.0f, 0.1f),
                Skill("lily_step", "Lv6 行如百合", "移动速度增加30%。", 6, SkillInputCommand.Passive, SkillEffectType.Passive, 0f, 0, 0, 0f, 0f, 0f, 0f, 0f),
                Skill("grapple", "Lv7 坐如牡丹", "使用寝技将近程范围内的最多3名敌人击倒在地。", 7, SkillInputCommand.DownJump, SkillEffectType.Knockdown, 1.25f, 18, 0, 0.8f, 1.6f, 0.65f, 0.6f, 1.1f),
                Skill("close_defense", "Lv8 近距离正当防卫", "普通攻击连招增加一段不消耗弹药的破防枪击。", 8, SkillInputCommand.Passive, SkillEffectType.Passive, 0f, 0, 0, 0f, 0f, 0f, 0f, 0f),
                Skill("shining_wink", "Lv9 闪耀 Wink!", "使前方中近距离的敌人魅惑僵直3秒，任意攻击会解除魅惑。", 9, SkillInputCommand.ForwardJump, SkillEffectType.Charm, 2f, 14, 0, 0f, 2.8f, 0.75f, 0f, 3f),
                Skill("machine_gun", "Lv10 机关枪", "挥舞裙下的机关枪横扫周围敌人，将敌人向四周击飞。", 10, SkillInputCommand.UpAttack, SkillEffectType.MachineGun, 2.4f, 20, 6, 0.7f, 3.4f, 0.8f, 2.8f, 0.55f)
            };
        }

        private static SkillDefinition Skill(
            string id,
            string displayName,
            string description,
            int level,
            SkillInputCommand command,
            SkillEffectType effect,
            float cooldown,
            int staminaCost,
            int ammoCost,
            float damageMultiplier,
            float range,
            float depthTolerance,
            float knockback,
            float stun)
        {
            SkillDefinition skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.skillId = id;
            skill.displayName = displayName;
            skill.description = description;
            skill.requiredLevel = level;
            skill.inputCommand = command;
            skill.effectType = effect;
            skill.cooldownSeconds = cooldown;
            skill.staminaCost = staminaCost;
            skill.ammoCost = ammoCost;
            skill.damageMultiplier = damageMultiplier;
            skill.range = range;
            skill.depthTolerance = depthTolerance;
            skill.knockback = knockback;
            skill.stunSeconds = stun;
            return skill;
        }
    }
}
