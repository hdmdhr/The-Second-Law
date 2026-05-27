export interface BackgroundAudioCue {
  src: string;
  loop?: boolean;
  volume?: number;
  fadeInSeconds?: number;
  startSeconds?: number;
  startProgress?: number;
  startOffsetSeconds?: number;
}

export const DEFAULT_BACKGROUND_AUDIO_VOLUME = 0.5;

export function resolveCueStartTime(cue: BackgroundAudioCue, videoDuration: number): number {
  if (!Number.isFinite(videoDuration) || videoDuration <= 0) {
    return cue.startSeconds ?? 0;
  }

  if (cue.startSeconds !== undefined) {
    return Math.max(0, cue.startSeconds);
  }

  const progress = Math.min(1, Math.max(0, cue.startProgress ?? 0.5));
  const offset = cue.startOffsetSeconds ?? 0;
  return Math.max(0, videoDuration * progress + offset);
}

export function resolveCueVolume(
  cue: BackgroundAudioCue,
  videoCurrentTime: number,
  videoDuration: number
): number {
  const targetVolume = cue.volume ?? DEFAULT_BACKGROUND_AUDIO_VOLUME;
  const startTime = resolveCueStartTime(cue, videoDuration);

  if (videoCurrentTime < startTime) {
    return 0;
  }

  const fadeInSeconds = cue.fadeInSeconds ?? 0;
  if (fadeInSeconds <= 0) {
    return targetVolume;
  }

  const fadeProgress = Math.min(1, (videoCurrentTime - startTime) / fadeInSeconds);
  return targetVolume * fadeProgress;
}
