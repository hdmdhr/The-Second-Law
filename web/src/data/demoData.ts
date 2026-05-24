import type { HotspotDefinition, LetterTemplate, ProgressionState, QuestDefinition } from "../types";

export const guildAssets = {
  background: "/assets/guild/demo-bg-0.png",
  counterVideo: "/assets/guild/video/lobby-to-desk.mp4",
  boardVideo: "/assets/guild/video/lobby-to-bulletin.mp4",
  tableVideo: "/assets/guild/video/lobby-to-table.mp4",
  tableLoopVideo: "/assets/guild/video/table-loop.mp4"
} as const;

export const hotspots: HotspotDefinition[] = [
  {
    id: "counter",
    labelKey: "guild.hotspot.counter",
    maskSrc: "/assets/guild/hotspots/counter-zone.png"
  },
  {
    id: "board",
    labelKey: "guild.hotspot.board",
    maskSrc: "/assets/guild/hotspots/board-icon.png"
  },
  {
    id: "party",
    labelKey: "guild.hotspot.table",
    maskSrc: "/assets/guild/hotspots/table-zone.png"
  },
  {
    id: "shop",
    labelKey: "guild.hotspot.shop",
    maskSrc: "/assets/guild/hotspots/equipment-zone.png"
  }
];

export const firstQuest: QuestDefinition = {
  questId: "f_rank_slime_cleanup",
  titleKey: "quest.slime.title",
  clientKey: "quest.slime.client",
  descriptionKey: "quest.slime.description",
  targetMonsterKey: "quest.slimes",
  targetCount: 3,
  rewardExperience: 140,
  rewardGold: 35,
  rewardReputation: 4
};

export const slimeLetter: LetterTemplate = {
  templateId: "slime_thanks",
  senderKey: "letter.slime.sender",
  bodyKey: "letter.slime.body",
  replyOpeningKeys: ["reply.opening.0", "reply.opening.1", "reply.opening.2"],
  replyBodyKeys: ["reply.body.0", "reply.body.1", "reply.body.2"],
  replyClosingKeys: ["reply.closing.0", "reply.closing.1", "reply.closing.2"],
  affectionReward: 1
};

export const initialProgression: ProgressionState = {
  level: 1,
  experience: 0,
  gold: 0,
  reputation: 0,
  talentPoints: 0,
  affection: 0
};

export function experienceForNextLevel(level: number) {
  return 80 + level * 45;
}
