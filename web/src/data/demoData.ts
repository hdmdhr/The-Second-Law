import type { HotspotDefinition, LetterTemplate, ProgressionState, QuestDefinition } from "../types";

function publicAsset(path: string) {
  return `${import.meta.env.BASE_URL}${path.replace(/^\//, "")}`;
}

export const guildAssets = {
  background: publicAsset("/assets/guild/demo-bg-0.png"),
  counterVideo: publicAsset("/assets/guild/video/lobby-to-counter.mp4"),
  counterLoopVideo: publicAsset("/assets/guild/video/counter-loop.mp4"),
  boardVideo: publicAsset("/assets/guild/video/lobby-to-bulletin.mp4"),
  boardLoopVideo: publicAsset("/assets/guild/video/bulletin-loop.mp4"),
  tableVideo: publicAsset("/assets/guild/video/lobby-to-table.mp4"),
  tableLoopVideo: publicAsset("/assets/guild/video/table-loop.mp4"),
  shopVideo: publicAsset("/assets/guild/video/lobby-to-shop.mp4"),
  shopThemeMusic: publicAsset("/assets/guild/audio/shop-theme.mp3"),
  counterThemeMusic: publicAsset("/assets/guild/audio/counter-theme.mp3"),
  tableThemeMusic: publicAsset("/assets/guild/audio/table-theme.mp3")
} as const;

export const hotspots: HotspotDefinition[] = [
  {
    id: "counter",
    labelKey: "guild.hotspot.counter",
    maskSrc: publicAsset("/assets/guild/hotspots/counter-zone.png")
  },
  {
    id: "board",
    labelKey: "guild.hotspot.board",
    maskSrc: publicAsset("/assets/guild/hotspots/board-icon.png")
  },
  {
    id: "party",
    labelKey: "guild.hotspot.table",
    maskSrc: publicAsset("/assets/guild/hotspots/table-zone.png")
  },
  {
    id: "shop",
    labelKey: "guild.hotspot.shop",
    maskSrc: publicAsset("/assets/guild/hotspots/equipment-zone.png")
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
