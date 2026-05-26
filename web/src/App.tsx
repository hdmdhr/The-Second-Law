import { useCallback, useMemo, useState } from "react";
import { useGuildAudio } from "./audio/useGuildAudio";
import type { BackgroundAudioCue } from "./audio/types";
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
const TransitionSpeedKey = "SecondLaw.Web.TransitionSpeed";
const ProgressionKey = "SecondLaw.Web.Progression";
const HubReleaseFadeSeconds = 3;
const transitionSpeeds = [1, 2, 3, 4] as const;
type TransitionSpeed = (typeof transitionSpeeds)[number];
type TransitionTarget = Extract<GuildView, "counter" | "board" | "party" | "shop">;
type TransitionPhase = "playing" | "pinned";

interface TransitionState {
  target: TransitionTarget;
  phase: TransitionPhase;
  videoSrc: string;
  pinnedVideoSrc?: string;
  pinnedLoops?: boolean;
  pinnedMuted?: boolean;
  pinnedAudioFadeOutSeconds?: number;
  transitionAudioFadeOutSeconds?: number;
  backgroundAudio?: BackgroundAudioCue;
}

function readLanguage(): Language {
  return localStorage.getItem(LanguageKey) === "en" ? "en" : "zh";
}

function readBoolean(key: string) {
  return localStorage.getItem(key) === "1";
}

function readTransitionSpeed(): TransitionSpeed {
  const saved = Number(localStorage.getItem(TransitionSpeedKey));
  return transitionSpeeds.includes(saved as TransitionSpeed) ? (saved as TransitionSpeed) : 2;
}

function nextTransitionSpeed(current: TransitionSpeed): TransitionSpeed {
  const currentIndex = transitionSpeeds.indexOf(current);
  return transitionSpeeds[(currentIndex + 1) % transitionSpeeds.length];
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
  const { armBackgroundAudio, syncWithVideoProgress, releaseBackgroundAudio } = useGuildAudio();
  const [language, setLanguage] = useState<Language>(readLanguage);
  const [view, setView] = useState<GuildView>("hub");
  const [skipTransitions, setSkipTransitions] = useState(() => readBoolean(SkipTransitionKey));
  const [transitionSpeed, setTransitionSpeed] = useState<TransitionSpeed>(readTransitionSpeed);
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
      releaseBackgroundAudio(HubReleaseFadeSeconds);

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
    [releaseBackgroundAudio, translate]
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

  function cycleTransitionSpeed() {
    setTransitionSpeed((current) => {
      const next = nextTransitionSpeed(current);
      localStorage.setItem(TransitionSpeedKey, String(next));
      return next;
    });
  }

  function openHotspot(hotspot: HotspotId) {
    const transition = transitionForHotspot(hotspot);
    if (transition) {
      if (transition.backgroundAudio) {
        void armBackgroundAudio(transition.backgroundAudio);
      } else {
        releaseBackgroundAudio(HubReleaseFadeSeconds);
      }

      openWithTransition(transition);
      return;
    }

    releaseBackgroundAudio(HubReleaseFadeSeconds);
    setTransitionState(null);
    setView(hotspot);
  }

  function transitionForHotspot(hotspot: HotspotId): TransitionState | null {
    if (hotspot === "counter") {
      return {
        target: "counter",
        phase: "playing",
        videoSrc: guildAssets.counterVideo,
        pinnedVideoSrc: guildAssets.counterLoopVideo,
        pinnedLoops: true,
        pinnedMuted: false,
        pinnedAudioFadeOutSeconds: 2,
        transitionAudioFadeOutSeconds: 2,
        backgroundAudio: {
          src: guildAssets.counterThemeMusic,
          loop: true,
          startSeconds: 5
        }
      };
    }

    if (hotspot === "board") {
      return {
        target: "board",
        phase: "playing",
        videoSrc: guildAssets.boardVideo,
        pinnedVideoSrc: guildAssets.boardLoopVideo,
        pinnedLoops: true,
        pinnedMuted: false,
        pinnedAudioFadeOutSeconds: 2,
        transitionAudioFadeOutSeconds: 2,
        backgroundAudio: {
          src: guildAssets.boardThemeMusic,
          loop: true,
          startProgress: 1,
          startOffsetSeconds: -2,
          fadeInSeconds: 2
        }
      };
    }

    if (hotspot === "party") {
      return {
        target: "party",
        phase: "playing",
        videoSrc: guildAssets.tableVideo,
        pinnedVideoSrc: guildAssets.tableLoopVideo,
        pinnedLoops: true,
        pinnedMuted: true,
        backgroundAudio: {
          src: guildAssets.tableThemeMusic,
          loop: true,
          startProgress: 0.5
        }
      };
    }

    if (hotspot === "shop") {
      return {
        target: "shop",
        phase: "playing",
        videoSrc: guildAssets.shopVideo,
        backgroundAudio: {
          src: guildAssets.shopThemeMusic,
          loop: true,
          startProgress: 0.5
        }
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

      if (next.backgroundAudio) {
        syncWithVideoProgress({
          currentTime: Number.MAX_SAFE_INTEGER,
          duration: Number.MAX_SAFE_INTEGER,
          pinned: true,
          skipTransition: true
        });
      }

      return;
    }

    setTransitionState(next);
  }

  function startQuest() {
    releaseBackgroundAudio(HubReleaseFadeSeconds);
    bridge.startQuest(firstQuest.questId);
    setTransitionState(null);
    setView("battleMock");
  }

  function showHub() {
    releaseBackgroundAudio(HubReleaseFadeSeconds);
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
    <div className={[styles.app, "rpgui-content"].join(" ")}>
      {view === "hub" ? (
        <>
          <GuildHub
            language={language}
            translate={translate}
            debugHotspots={false}
            onOpenHotspot={openHotspot}
          />
          <TopBar
            translate={translate}
            skipTransitions={skipTransitions}
            transitionSpeedLabel={language === "zh" ? `${transitionSpeed}倍速` : `${transitionSpeed}x`}
            onToggleLanguage={toggleLanguage}
            onToggleSkipTransitions={toggleSkipTransitions}
            onCycleTransitionSpeed={cycleTransitionSpeed}
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
          src={transitionState.videoSrc}
          pinned={transitionState.phase === "pinned"}
          postrollSrc={transitionState.pinnedVideoSrc}
          postrollLoop={Boolean(transitionState.pinnedLoops)}
          postrollMuted={transitionState.pinnedMuted}
          postrollAudioFadeOutSeconds={transitionState.pinnedAudioFadeOutSeconds}
          startWithPostroll={transitionState.phase === "pinned"}
          audioFadeOutSeconds={transitionState.transitionAudioFadeOutSeconds}
          playbackRate={transitionState.phase === "playing" ? transitionSpeed : 1}
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
