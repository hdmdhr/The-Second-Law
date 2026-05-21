using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    public enum GameLanguage
    {
        Chinese,
        English
    }

    public static class LocalizationService
    {
        private const string LanguageKey = "SecondLaw.Language";
        private static GameLanguage? cachedLanguage;

        private static readonly Dictionary<string, string> Zh = new Dictionary<string, string>
        {
            { "language.button", "English" },
            { "guild.title", "冒险者协会 - 第二法则 Demo" },
            { "guild.tagline", "F 级柜台 / 本地原型 / 缓存来信系统" },
            { "quest.target", "目标" },
            { "quest.slimes", "史莱姆" },
            { "quest.reward", "报酬" },
            { "button.accept", "接受委托" },
            { "button.reset", "重置进度" },
            { "status.level", "等级" },
            { "status.gold", "金币" },
            { "status.reputation", "协会声望" },
            { "status.talent_points", "天赋点" },
            { "status.affection", "好感度" },
            { "letter.empty", "完成一个委托后，会在这里收到委托人的来信。" },
            { "reply.opening", "开头" },
            { "reply.body", "正文" },
            { "reply.closing", "结尾" },
            { "reply.send", "发送回信" },
            { "reply.need_choices", "请先各选择一个开头、正文和结尾。" },
            { "reply.sent", "回信已寄出。好感度 +" },
            { "hud.hp", "生命" },
            { "hud.stamina", "体力" },
            { "hud.ammo", "弹药" },
            { "hud.defeated", "击败" },
            { "hud.reload_move", "R 装填 / ASDW 移动" },
            { "hud.commands", "按键" },
            { "hud.controls", "J 攻击   K 跳跃   L 防御   Space 射击   Q 大招" },
            { "hud.combo_help", "技能：L 起手，然后 W/S/前方，再接 J/K/Space。快捷技：L>K>J。" },
            { "hud.unlocked", "已解锁" },
            { "hud.passive", "被动" },
            { "reward.exp", "经验" },
            { "reward.gold", "金币" },
            { "reward.reputation", "协会声望" },
            { "reward.level_up", "升级！Lv" },
            { "reward.talent", " / 天赋点 +" },
            { "battle.retreat", "战斗撤退。没有获得报酬。" },
            { "demo.reset", "Demo 进度已重置。" },
            { "cache.note", "\n\n-- 本地缓存信件草稿 --\n这段文字现在保存在本机；之后可以在同一接口接入 AI 生成并缓存结果。" },
            { "quest.slime.title", "F 级委托：清理史莱姆" },
            { "quest.slime.client", "米娜，冒险者协会接待员" },
            { "quest.slime.description", "三只史莱姆堵住了采药小路。把它们清掉，然后回到协会柜台领取报酬。" },
            { "letter.slime.sender", "米娜" },
            { "letter.slime.body", "致我们最新的 F 级冒险者：\n\n采药小路已经重新开放。药剂师送来了一篮薄荷叶，说今天镇上的空气闻起来都勇敢了一点。\n\n你带着鞋底的泥和袖口的史莱姆凝胶回来，这说明你确实认真完成了委托。协会已经记录下你的第一次成功任务。" },
            { "reply.opening.0", "亲爱的米娜：" },
            { "reply.opening.1", "致冒险者协会：" },
            { "reply.opening.2", "米娜：" },
            { "reply.body.0", "很高兴那条路又安全了。请替我向药剂师问好。" },
            { "reply.body.1", "史莱姆比想象中难缠，不过这是很好的第一步。" },
            { "reply.body.2", "下次我会带着更干净的袖口和更快的报告回来。" },
            { "reply.closing.0", "谨上，你们的新 F 级冒险者。" },
            { "reply.closing.1", "我会继续做自己能做到的事。" },
            { "reply.closing.2", "适度冒险，不要太勉强自己。" },
            { "skill.uppercut", "Lv1 上勾拳" },
            { "skill.vital_spark", "Lv2 活力四射" },
            { "skill.pistol_shot", "Lv3 手枪点射" },
            { "skill.peony_stance", "Lv4 站如芍药" },
            { "skill.burst_fire", "Lv5 火力四射" },
            { "skill.lily_step", "Lv6 行如百合" },
            { "skill.grapple", "Lv7 坐如牡丹" },
            { "skill.close_defense", "Lv8 近距离正当防卫" },
            { "skill.shining_wink", "Lv9 闪耀 Wink!" },
            { "skill.machine_gun", "Lv10 机关枪" }
        };

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "language.button", "中文" },
            { "guild.title", "Adventurers' Guild - Second Law Demo" },
            { "guild.tagline", "F-rank counter / local prototype / cached letter loop" },
            { "quest.target", "Target" },
            { "quest.slimes", "slimes" },
            { "quest.reward", "Reward" },
            { "button.accept", "Accept Request" },
            { "button.reset", "Reset Demo" },
            { "status.level", "Level" },
            { "status.gold", "Gold" },
            { "status.reputation", "Reputation" },
            { "status.talent_points", "Talent Points" },
            { "status.affection", "Affection" },
            { "letter.empty", "Complete a request to receive a client letter." },
            { "reply.opening", "Opening" },
            { "reply.body", "Body" },
            { "reply.closing", "Closing" },
            { "reply.send", "Send Reply" },
            { "reply.need_choices", "Choose one opening, body, and closing before sending." },
            { "reply.sent", "Reply sent. Affection +" },
            { "hud.hp", "HP" },
            { "hud.stamina", "Stamina" },
            { "hud.ammo", "Ammo" },
            { "hud.defeated", "Defeated" },
            { "hud.reload_move", "R reload / ASDW move" },
            { "hud.commands", "Commands" },
            { "hud.controls", "J Attack   K Jump   L Guard   Space Shoot   Q Ultimate" },
            { "hud.combo_help", "Skills: L, then W/S/Forward, then J/K/Space. Shortcut: L>K>J." },
            { "hud.unlocked", "OK" },
            { "hud.passive", "Passive" },
            { "reward.exp", "EXP" },
            { "reward.gold", "Gold" },
            { "reward.reputation", "Guild Reputation" },
            { "reward.level_up", "Level Up! Lv" },
            { "reward.talent", " / Talent Point +" },
            { "battle.retreat", "Retreated from battle. No rewards granted." },
            { "demo.reset", "Demo progress reset." },
            { "cache.note", "\n\n-- cached local letter draft --\nThis text is stored locally now; the same hook can later call an AI provider and cache the result." },
            { "quest.slime.title", "F-Rank Request: Slime Cleanup" },
            { "quest.slime.client", "Mina, Guild Receptionist" },
            { "quest.slime.description", "Three slimes are blocking the herb road. Clear them out, then return to the guild counter for your reward." },
            { "letter.slime.sender", "Mina" },
            { "letter.slime.body", "To our newest F-rank adventurer,\n\nThe herb road is open again. The apothecary sent a basket of mint leaves and said the town smells a little braver today.\n\nYou came back with mud on your shoes and slime gel on your cuffs, which means you did the work properly. The guild has recorded your first successful request." },
            { "reply.opening.0", "Dear Mina," },
            { "reply.opening.1", "To the Adventurers' Guild," },
            { "reply.opening.2", "Mina," },
            { "reply.body.0", "I am glad the road is safe again. Please send my thanks to the apothecary." },
            { "reply.body.1", "The slimes were tougher than expected, but this was a good first step." },
            { "reply.body.2", "Next time, I will return with cleaner cuffs and a faster report." },
            { "reply.closing.0", "Respectfully, your new F-rank adventurer." },
            { "reply.closing.1", "I will keep doing what I can." },
            { "reply.closing.2", "Yes adventure, but not too adventurous." },
            { "skill.uppercut", "Lv1 Uppercut" },
            { "skill.vital_spark", "Lv2 Vital Spark" },
            { "skill.pistol_shot", "Lv3 Pistol Shot" },
            { "skill.peony_stance", "Lv4 Peony Stance" },
            { "skill.burst_fire", "Lv5 Burst Fire" },
            { "skill.lily_step", "Lv6 Lily Step" },
            { "skill.grapple", "Lv7 Peony Grapple" },
            { "skill.close_defense", "Lv8 Close-Range Self Defense" },
            { "skill.shining_wink", "Lv9 Shining Wink!" },
            { "skill.machine_gun", "Lv10 Machine Gun" }
        };

        public static GameLanguage CurrentLanguage
        {
            get
            {
                if (!cachedLanguage.HasValue)
                {
                    cachedLanguage = PlayerPrefs.GetString(LanguageKey, "zh") == "en" ? GameLanguage.English : GameLanguage.Chinese;
                }

                return cachedLanguage.Value;
            }
        }

        public static string CurrentLanguageCode => CurrentLanguage == GameLanguage.English ? "en" : "zh";

        public static void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == GameLanguage.Chinese ? GameLanguage.English : GameLanguage.Chinese);
        }

        public static void SetLanguage(GameLanguage language)
        {
            cachedLanguage = language;
            PlayerPrefs.SetString(LanguageKey, language == GameLanguage.English ? "en" : "zh");
            PlayerPrefs.Save();
        }

        public static string T(string key)
        {
            Dictionary<string, string> table = CurrentLanguage == GameLanguage.English ? En : Zh;
            if (table.TryGetValue(key, out string value))
            {
                return value;
            }

            return key;
        }

        public static string SkillName(SkillDefinition skill)
        {
            return skill == null ? string.Empty : T("skill." + skill.skillId);
        }

        public static string[] ReplyOpenings()
        {
            return Choices("reply.opening");
        }

        public static string[] ReplyBodies()
        {
            return Choices("reply.body");
        }

        public static string[] ReplyClosings()
        {
            return Choices("reply.closing");
        }

        private static string[] Choices(string prefix)
        {
            return new[] { T(prefix + ".0"), T(prefix + ".1"), T(prefix + ".2") };
        }
    }
}
