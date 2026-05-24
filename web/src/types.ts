export type Language = "zh" | "en";

export type GuildView = "hub" | "counter" | "board" | "party" | "shop" | "battleMock";

export type HotspotId = "counter" | "board" | "party" | "shop";

export interface HotspotDefinition {
  id: HotspotId;
  labelKey: string;
  maskSrc: string;
}

export interface QuestDefinition {
  questId: string;
  titleKey: string;
  clientKey: string;
  descriptionKey: string;
  targetMonsterKey: string;
  targetCount: number;
  rewardExperience: number;
  rewardGold: number;
  rewardReputation: number;
}

export interface LetterTemplate {
  templateId: string;
  senderKey: string;
  bodyKey: string;
  replyOpeningKeys: string[];
  replyBodyKeys: string[];
  replyClosingKeys: string[];
  affectionReward: number;
}

export interface ProgressionState {
  level: number;
  experience: number;
  gold: number;
  reputation: number;
  talentPoints: number;
  affection: number;
}

export interface BattleResult {
  victory: boolean;
  questId: string;
}
