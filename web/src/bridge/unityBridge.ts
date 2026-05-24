import type { BattleResult, Language } from "../types";

export interface UnityBridge {
  startQuest: (questId: string) => void;
  setLanguage: (language: Language) => void;
  setSkipTransitions: (enabled: boolean) => void;
}

declare global {
  interface Window {
    secondLawUnity?: Partial<UnityBridge>;
    secondLawWeb?: {
      unityReady: () => void;
      battleFinished: (result: BattleResult) => void;
    };
  }
}

export function createUnityBridge(onBattleFinished: (result: BattleResult) => void): UnityBridge {
  window.secondLawWeb = {
    unityReady: () => {
      console.info("[SecondLaw] Unity WebGL ready.");
    },
    battleFinished: onBattleFinished
  };

  return {
    startQuest: (questId) => {
      window.secondLawUnity?.startQuest?.(questId);
      console.info("[SecondLaw] startQuest", questId);
    },
    setLanguage: (language) => {
      window.secondLawUnity?.setLanguage?.(language);
      console.info("[SecondLaw] setLanguage", language);
    },
    setSkipTransitions: (enabled) => {
      window.secondLawUnity?.setSkipTransitions?.(enabled);
      console.info("[SecondLaw] setSkipTransitions", enabled);
    }
  };
}
