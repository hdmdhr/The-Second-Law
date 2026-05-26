import {
  BackgroundAudioCue,
  resolveCueStartTime,
  resolveCueVolume
} from "./types";

export interface VideoProgressSnapshot {
  currentTime: number;
  duration: number;
  playbackRate?: number;
  pinned?: boolean;
  skipTransition?: boolean;
}

export interface BackgroundAudioSyncResult {
  volume: number;
  shouldStartPlayback: boolean;
  holdPaused: boolean;
}

export function armBackgroundAudio(audio: HTMLAudioElement, cue: BackgroundAudioCue): Promise<void> {
  audio.src = cue.src;
  audio.loop = cue.loop ?? true;
  audio.volume = 0;
  audio.currentTime = 0;

  // Unlock autoplay during the user gesture, then hold at 0:00 until the cue start time.
  return audio
    .play()
    .then(() => {
      audio.pause();
      audio.currentTime = 0;
    })
    .catch(() => undefined);
}

export function syncBackgroundAudioState(
  audio: HTMLAudioElement,
  cue: BackgroundAudioCue,
  progress: VideoProgressSnapshot,
  audibleStarted: boolean
): BackgroundAudioSyncResult {
  const { currentTime, duration, pinned, skipTransition } = progress;

  if (skipTransition) {
    const volume = cue.volume ?? 0.7;
    audio.volume = volume;
    return { volume, shouldStartPlayback: true, holdPaused: false };
  }

  if (!Number.isFinite(duration) || duration <= 0) {
    if (pinned) {
      const volume = cue.volume ?? 0.7;
      audio.volume = volume;
      return { volume, shouldStartPlayback: !audibleStarted, holdPaused: false };
    }

    audio.volume = 0;
    return { volume: 0, shouldStartPlayback: false, holdPaused: true };
  }

  const startTime = resolveCueStartTime(cue, duration);
  const effectiveTime = pinned ? duration : currentTime;

  if (!audibleStarted && effectiveTime < startTime) {
    audio.volume = 0;
    return { volume: 0, shouldStartPlayback: false, holdPaused: true };
  }

  const volume = resolveCueVolume(cue, effectiveTime, duration);
  audio.volume = volume;

  if (!audibleStarted && effectiveTime >= startTime) {
    return { volume, shouldStartPlayback: true, holdPaused: false };
  }

  return { volume, shouldStartPlayback: false, holdPaused: false };
}

export function fadeOutAndStop(
  audio: HTMLAudioElement,
  fadeOutSeconds: number
): Promise<void> {
  if (fadeOutSeconds <= 0 || audio.paused) {
    audio.pause();
    audio.currentTime = 0;
    audio.volume = 0;
    return Promise.resolve();
  }

  return new Promise((resolve) => {
    const startVolume = audio.volume;
    const startedAt = performance.now();
    const fadeDurationMs = fadeOutSeconds * 1000;

    const tick = (now: number) => {
      const progress = Math.min(1, (now - startedAt) / fadeDurationMs);
      audio.volume = Math.max(0, startVolume * (1 - progress));

      if (progress < 1) {
        window.requestAnimationFrame(tick);
        return;
      }

      audio.pause();
      audio.currentTime = 0;
      audio.volume = 0;
      resolve();
    };

    window.requestAnimationFrame(tick);
  });
}

export function isBackgroundAudioAudible(audio: HTMLAudioElement): boolean {
  return !audio.paused && !audio.ended && audio.volume > 0.001;
}
