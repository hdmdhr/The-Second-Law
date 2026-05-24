import { useCallback, useMemo, useState } from "react";
import { createUnityBridge } from "./bridge/unityBridge";
import CounterPage from "./components/CounterPage";
import GuildHub from "./components/GuildHub";
import MockBattle from "./components/MockBattle";
import PlaceholderPage from "./components/PlaceholderPage";
import TopBar from "./components/TopBar";
import TransitionOverlay from "./components/TransitionOverlay";
import { experienceForNextLevel, firstQuest, guildAssets, initialProgression, slimeLetter } from "./data/demoData";
import { t } from "./data/i18n";
import type { BattleResult, GuildView, HotspotId, Language, ProgressionState } from "./types";
import styles from "./App.module.css";

const LanguageKey = "SecondLaw.Web.Language";
const SkipTransitionKey = "SecondLaw.Web.SkipGuildTransitions";
const ProgressionKey = "SecondLaw.Web.Progression";
type TransitionTarget = Extract<GuildView, "counter" | "board" | "party">;
type TransitionPhase = "playing" | "pinned";

interface TransitionState {
  target: TransitionTarget;
  phase: TransitionPhase;
  videoSrc: string;
  pinnedVideoSrc?: string;
  pinnedLoops?: boolean;
}

function readLanguage(): Language {
  return localStorage.getItem(LanguageKey) === "en" ? "en" : "zh";
}

function readBoolean(key: string) {
  return localStorage.getItem(key) === "1";
}

function readProgression() {
  const saved = localStorage.getItem(ProgressionKey);
  if (!saved) {
    return initialProgression;
  }

  try {
    return { ...initialProgression, ...JSON.parse(saved) } as ProgressionState;
  } catch {
    return initialProgression;
  }
}

function saveProgression(state: ProgressionState) {
  localStorage.setItem(ProgressionKey, JSON.stringify(state));
}

export default function App() {
  const [language, setLanguage] = useState<Language>(readLanguage);
  const [view, setView] = useState<GuildView>("hub");
  const [skipTransitions, setSkipTransitions] = useState(() => readBoolean(SkipTransitionKey));
  const [debugHotspots, setDebugHotspots] = useState(false);
  const [transitionState, setTransitionState] = useState<TransitionState | null>(null);
  const [progression, setProgression] = useState<ProgressionState>(readProgression);
  const [rewardMessages, setRewardMessages] = useState<string[]>([]);
  const [replyMessage, setReplyMessage] = useState("");
  const [selectedOpening, setSelectedOpening] = useState("");
  const [selectedBody, setSelectedBody] = useState("");
  const [selectedClosing, setSelectedClosing] = useState("");

  const translate = useCallback((key: string) => t(language, key), [language]);

  const handleBattleFinished = useCallback(
    (result: BattleResult) => {
      if (!result.victory) {
        setRewardMessages([translate("battle.retreat")]);
        setTransitionState(null);
        setView("counter");
        return;
      }

      setProgression((current) => {
        const next = { ...current };
        const messages = [
          `+${firstQuest.rewardExperience} ${translate("reward.exp")}`,
          `+${firstQuest.rewardGold} ${translate("reward.gold")}`,
          `+${firstQuest.rewardReputation} ${translate("reward.reputation")}`
        ];

        next.experience += firstQuest.rewardExperience;
        next.gold += firstQuest.rewardGold;
        next.reputation += firstQuest.rewardReputation;

        while (next.level < 10 && next.experience >= experienceForNextLevel(next.level)) {
          next.experience -= experienceForNextLevel(next.level);
          next.level += 1;
          next.talentPoints += 1;
          messages.push(`${translate("reward.level_up")}${next.level}${translate("reward.talent")}1`);
        }

        saveProgression(next);
        setRewardMessages(messages);
        return next;
      });

      setReplyMessage("");
      setSelectedOpening("");
      setSelectedBody("");
      setSelectedClosing("");
      setTransitionState(null);
      setView("counter");
    },
    [translate]
  );

  const bridge = useMemo(() => createUnityBridge(handleBattleFinished), [handleBattleFinished]);

  function toggleLanguage() {
    const next = language === "zh" ? "en" : "zh";
    setLanguage(next);
    localStorage.setItem(LanguageKey, next);
    bridge.setLanguage(next);
  }

  function toggleSkipTransitions() {
    const next = !skipTransitions;
    setSkipTransitions(next);
    localStorage.setItem(SkipTransitionKey, next ? "1" : "0");
    bridge.setSkipTransitions(next);
  }

  function openHotspot(hotspot: HotspotId) {
    const transition = transitionForHotspot(hotspot);
    if (transition) {
      openWithTransition(transition);
      return;
    }

    setTransitionState(null);
    setView(hotspot);
  }

  function transitionForHotspot(hotspot: HotspotId): TransitionState | null {
    if (hotspot === "counter") {
      return {
        target: "counter",
        phase: "playing",
        videoSrc: guildAssets.counterVideo
      };
    }

    if (hotspot === "board") {
      return {
        target: "board",
        phase: "playing",
        videoSrc: guildAssets.boardVideo
      };
    }

    if (hotspot === "party") {
      return {
        target: "party",
        phase: "playing",
        videoSrc: guildAssets.tableVideo,
        pinnedVideoSrc: guildAssets.tableLoopVideo,
        pinnedLoops: true
      };
    }

    return null;
  }

  function openWithTransition(next: TransitionState) {
    if (skipTransitions) {
      setTransitionState(
        next.pinnedVideoSrc
          ? {
              ...next,
              phase: "pinned"
            }
          : null
      );
      setView(next.target);
      return;
    }

    setTransitionState(next);
  }

  function startQuest() {
    bridge.startQuest(firstQuest.questId);
    setTransitionState(null);
    setView("battleMock");
  }

  function showHub() {
    setTransitionState(null);
    setView("hub");
  }

  function resetProgression() {
    saveProgression(initialProgression);
    setProgression(initialProgression);
    setRewardMessages([translate("demo.reset")]);
    setReplyMessage("");
    setSelectedOpening("");
    setSelectedBody("");
    setSelectedClosing("");
  }

  function sendReply() {
    if (!selectedOpening || !selectedBody || !selectedClosing) {
      setReplyMessage(translate("reply.need_choices"));
      return;
    }

    const next = {
      ...progression,
      affection: progression.affection + Math.max(0, slimeLetter.affectionReward)
    };
    saveProgression(next);
    setProgression(next);
    setReplyMessage(
      `${selectedOpening}\n\n${selectedBody}\n\n${selectedClosing}\n\n${translate("reply.sent")}${slimeLetter.affectionReward}`
    );
  }

  return (
    <div className={styles.app}>
      {view === "hub" ? (
        <>
          <GuildHub
            language={language}
            translate={translate}
            debugHotspots={debugHotspots}
            onOpenHotspot={openHotspot}
          />
          <TopBar
            language={language}
            translate={translate}
            skipTransitions={skipTransitions}
            debugHotspots={debugHotspots}
            onToggleLanguage={toggleLanguage}
            onToggleSkipTransitions={toggleSkipTransitions}
            onToggleDebugHotspots={() => setDebugHotspots((value) => !value)}
          />
        </>
      ) : null}

      {view === "counter" ? (
        <CounterPage
          translate={translate}
          useVideoBackdrop={transitionState?.phase === "pinned" && transitionState.target === "counter"}
          progression={progression}
          rewardMessages={rewardMessages}
          replyMessage={replyMessage}
          selectedOpening={selectedOpening}
          selectedBody={selectedBody}
          selectedClosing={selectedClosing}
          onPickOpening={setSelectedOpening}
          onPickBody={setSelectedBody}
          onPickClosing={setSelectedClosing}
          onSendReply={sendReply}
          onStartQuest={startQuest}
          onReset={resetProgression}
          onBack={showHub}
        />
      ) : null}

      {view === "board" || view === "party" || view === "shop" ? (
        <PlaceholderPage
          view={view}
          translate={translate}
          useVideoBackdrop={transitionState?.phase === "pinned" && transitionState.target === view}
          onBack={showHub}
        />
      ) : null}

      {view === "battleMock" ? (
        <MockBattle
          translate={translate}
          onVictory={() => window.secondLawWeb?.battleFinished({ victory: true, questId: firstQuest.questId })}
          onRetreat={() => window.secondLawWeb?.battleFinished({ victory: false, questId: firstQuest.questId })}
        />
      ) : null}

      {transitionState ? (
        <TransitionOverlay
          key={transitionState.target}
          src={
            transitionState.phase === "pinned" && transitionState.pinnedVideoSrc
              ? transitionState.pinnedVideoSrc
              : transitionState.videoSrc
          }
          pinned={transitionState.phase === "pinned"}
          loop={transitionState.phase === "pinned" && Boolean(transitionState.pinnedLoops)}
          onDone={() => {
            setTransitionState((current) =>
              current
                ? {
                    ...current,
                    phase: "pinned"
                  }
                : current
            );
            setView(transitionState.target);
          }}
        />
      ) : null}
    </div>
  );
}
