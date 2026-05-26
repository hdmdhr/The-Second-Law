import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useRef,
  type ReactNode
} from "react";
import {
  armBackgroundAudio,
  fadeOutAndStop,
  syncBackgroundAudioState,
  type VideoProgressSnapshot
} from "./guildAudioController";
import type { BackgroundAudioCue } from "./types";

interface GuildAudioContextValue {
  armBackgroundAudio: (cue: BackgroundAudioCue | null) => Promise<void>;
  syncWithVideoProgress: (progress: VideoProgressSnapshot) => void;
  releaseBackgroundAudio: (fadeOutSeconds?: number) => void;
}

const GuildAudioContext = createContext<GuildAudioContextValue | null>(null);

export function GuildAudioProvider({ children }: { children: ReactNode }) {
  const audioRef = useRef<HTMLAudioElement>(null);
  const cueRef = useRef<BackgroundAudioCue | null>(null);
  const armedRef = useRef(false);
  const releasingRef = useRef(false);
  const fadeOutFrameRef = useRef<number | null>(null);
  const audibleStartedRef = useRef(false);

  const cancelFadeOut = useCallback(() => {
    if (fadeOutFrameRef.current !== null) {
      window.cancelAnimationFrame(fadeOutFrameRef.current);
      fadeOutFrameRef.current = null;
    }

    releasingRef.current = false;
  }, []);

  const arm = useCallback(
    (cue: BackgroundAudioCue | null): Promise<void> => {
      cancelFadeOut();

      const audio = audioRef.current;
      if (!cue || !audio) {
        cueRef.current = null;
        armedRef.current = false;
        audibleStartedRef.current = false;
        return Promise.resolve();
      }

      audibleStartedRef.current = false;
      cueRef.current = cue;
      armedRef.current = false;

      return armBackgroundAudio(audio, cue)
        .then(() => {
          armedRef.current = true;
        })
        .catch(() => {
          armedRef.current = false;
        });
    },
    [cancelFadeOut]
  );

  const syncWithVideoProgress = useCallback((progress: VideoProgressSnapshot) => {
    if (releasingRef.current) {
      return;
    }

    const audio = audioRef.current;
    const cue = cueRef.current;
    if (!audio || !cue || !armedRef.current) {
      return;
    }

    const result = syncBackgroundAudioState(audio, cue, progress, audibleStartedRef.current);

    if (result.holdPaused) {
      if (!audio.paused) {
        audio.pause();
      }

      if (audio.currentTime > 0) {
        audio.currentTime = 0;
      }
    }

    if (result.shouldStartPlayback) {
      if (progress.skipTransition) {
        audibleStartedRef.current = true;
        if (audio.currentTime > 0) {
          audio.currentTime = 0;
        }
      } else if (!audibleStartedRef.current) {
        audibleStartedRef.current = true;
        audio.currentTime = 0;
      }

      if (audio.paused) {
        void audio.play().catch(() => {
          audibleStartedRef.current = false;
        });
      }
    }
  }, []);

  const release = useCallback(
    (fadeOutSeconds = 3) => {
      cancelFadeOut();

      const audio = audioRef.current;
      if (!audio || audio.paused) {
        cueRef.current = null;
        armedRef.current = false;
        audibleStartedRef.current = false;
        audio?.pause();
        if (audio) {
          audio.currentTime = 0;
          audio.volume = 0;
        }
        return;
      }

      releasingRef.current = true;

      void fadeOutAndStop(audio, fadeOutSeconds).then(() => {
        releasingRef.current = false;
        cueRef.current = null;
        armedRef.current = false;
        audibleStartedRef.current = false;
      });
    },
    [cancelFadeOut]
  );

  const value = useMemo(
    () => ({
      armBackgroundAudio: arm,
      syncWithVideoProgress,
      releaseBackgroundAudio: release
    }),
    [arm, release, syncWithVideoProgress]
  );

  return (
    <GuildAudioContext.Provider value={value}>
      {children}
      <audio ref={audioRef} preload="auto" aria-hidden="true" style={{ display: "none" }} />
    </GuildAudioContext.Provider>
  );
}

export function useGuildAudio(): GuildAudioContextValue {
  const context = useContext(GuildAudioContext);
  if (!context) {
    throw new Error("useGuildAudio must be used within GuildAudioProvider");
  }

  return context;
}
