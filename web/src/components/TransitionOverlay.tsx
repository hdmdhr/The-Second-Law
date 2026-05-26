import { useCallback, useEffect, useRef, useState } from "react";
import { useGuildAudio } from "../audio/useGuildAudio";
import styles from "../App.module.css";

type MediaElementWithGain = HTMLMediaElement & {
  __secondLawGainChain?: {
    context: AudioContext;
    gain: GainNode;
  };
};

type WindowWithWebkitAudio = Window & {
  webkitAudioContext?: typeof AudioContext;
};

interface TransitionOverlayProps {
  src: string;
  pinned: boolean;
  postrollSrc?: string;
  postrollLoop?: boolean;
  postrollMuted?: boolean;
  postrollAudioFadeOutSeconds?: number;
  startWithPostroll?: boolean;
  audioFadeOutSeconds?: number;
  videoAudioGain?: number;
  playbackRate?: number;
  onDone: () => void;
}

export default function TransitionOverlay({
  src,
  pinned,
  postrollSrc,
  postrollLoop = false,
  postrollMuted = false,
  postrollAudioFadeOutSeconds,
  startWithPostroll = false,
  audioFadeOutSeconds,
  videoAudioGain = 1.3,
  playbackRate = 1,
  onDone
}: TransitionOverlayProps) {
  const { syncWithVideoProgress } = useGuildAudio();
  const primaryVideoRef = useRef<HTMLVideoElement>(null);
  const postrollVideoRef = useRef<HTMLVideoElement>(null);
  const finishedRef = useRef(false);
  const postrollPendingRef = useRef(false);
  const postrollStartedRef = useRef(false);
  const [primaryReady, setPrimaryReady] = useState(false);
  const [postrollReady, setPostrollReady] = useState(false);
  const [postrollActive, setPostrollActive] = useState(false);

  const postrollLeadInSeconds = 0.8;

  const reportVideoProgress = useCallback(() => {
    const video = primaryVideoRef.current;
    if (!video) {
      return;
    }

    syncWithVideoProgress({
      currentTime: video.currentTime,
      duration: video.duration,
      playbackRate,
      pinned: pinned || startWithPostroll
    });
  }, [pinned, playbackRate, startWithPostroll, syncWithVideoProgress]);

  useEffect(() => {
    if (pinned || postrollActive) {
      return;
    }

    const timeout = window.setTimeout(() => {
      if (!finishedRef.current) {
        activatePostroll();
      }
    }, 12000);

    return () => window.clearTimeout(timeout);
  }, [pinned, postrollActive, postrollReady, postrollSrc]);

  useEffect(() => {
    setPrimaryReady(false);
    setPostrollReady(false);
    setPostrollActive(false);
    postrollPendingRef.current = false;
    postrollStartedRef.current = false;
    finishedRef.current = false;
  }, [postrollSrc, src]);

  useEffect(() => {
    reportVideoProgress();
  }, [pinned, playbackRate, reportVideoProgress, startWithPostroll]);

  useEffect(() => {
    if (!startWithPostroll || !postrollSrc) {
      return;
    }

    if (postrollStartedRef.current) {
      setPostrollActive(true);
      ensurePostrollPlaying();
      return;
    }

    if (postrollActive) {
      return;
    }

    const video = postrollVideoRef.current;
    if (!video || !isVideoReady(video)) {
      postrollPendingRef.current = true;
      video?.load();
      return;
    }

    postrollPendingRef.current = false;
    startPostrollVideo(video);
  }, [postrollActive, postrollReady, postrollSrc, startWithPostroll]);

  useEffect(() => {
    if (!pinned || !postrollSrc || !postrollStartedRef.current) {
      return;
    }

    setPostrollActive(true);
    ensurePostrollPlaying();
  }, [pinned, postrollSrc]);

  useEffect(() => {
    if (primaryVideoRef.current) {
      primaryVideoRef.current.playbackRate = playbackRate;
      primaryVideoRef.current.volume = 1;
      applyMediaGain(primaryVideoRef.current, videoAudioGain);
    }
  }, [playbackRate, src, videoAudioGain]);

  function finish() {
    if (finishedRef.current) {
      return;
    }

    finishedRef.current = true;
    onDone();
  }

  function activatePostroll() {
    if (!postrollSrc) {
      finish();
      return;
    }

    if (postrollStartedRef.current) {
      setPostrollActive(true);
      if (!finishedRef.current) {
        finish();
      }
      return;
    }

    const video = postrollVideoRef.current;
    if (!video || !isVideoReady(video)) {
      postrollPendingRef.current = true;
      video?.load();
      return;
    }

    startPostrollVideo(video);
    finish();
  }

  function startPostrollVideo(video: HTMLVideoElement, restart = false) {
    if (postrollStartedRef.current && !restart) {
      ensurePostrollPlaying();
      return;
    }

    postrollStartedRef.current = true;
    setPostrollActive(true);
    video.currentTime = 0;
    video.volume = 1;
    video.muted = Boolean(postrollMuted);
    video.playbackRate = postrollLoop ? randomLoopPlaybackRate() : 1;
    applyMediaGain(video, videoAudioGain);
    playVideo(video, "Loop playback");
  }

  function ensurePostrollPlaying() {
    const video = postrollVideoRef.current;
    if (!video || !postrollStartedRef.current) {
      return;
    }

    if (video.paused || video.ended) {
      playVideo(video, "Loop playback");
    }
  }

  function restartPostrollLoop() {
    const video = postrollVideoRef.current;
    if (!video) {
      return;
    }

    startPostrollVideo(video, true);
  }

  function updateAudioFade(video: HTMLVideoElement | null, muted: boolean, fadeOutSeconds?: number) {
    if (!video || muted || video.muted || !fadeOutSeconds || !Number.isFinite(video.duration) || video.duration <= 0) {
      return;
    }

    const remaining = video.duration - video.currentTime;
    if (remaining <= fadeOutSeconds) {
      video.volume = Math.max(0, remaining / fadeOutSeconds);
      return;
    }

    video.volume = 1;
  }

  function startPrimaryWhenReady() {
    setPrimaryReady(true);
    if (primaryVideoRef.current) {
      primaryVideoRef.current.playbackRate = playbackRate;
      primaryVideoRef.current.volume = 1;
      applyMediaGain(primaryVideoRef.current, videoAudioGain);
    }

    if (pinned || postrollActive) {
      return;
    }

    const video = primaryVideoRef.current;
    if (video) {
      playVideo(video, "Transition playback");
    }

    reportVideoProgress();
  }

  function preparePostrollWhenReady() {
    setPostrollReady(true);
    const video = postrollVideoRef.current;
    if (!video) {
      return;
    }

    video.muted = Boolean(postrollMuted);
    video.volume = 1;
    applyMediaGain(video, videoAudioGain);

    if (postrollStartedRef.current) {
      ensurePostrollPlaying();
      return;
    }

    if (!startWithPostroll && !postrollPendingRef.current) {
      return;
    }

    postrollPendingRef.current = false;
    startPostrollVideo(video);
    if (!startWithPostroll) {
      finish();
    }
  }

  function maybeActivatePostrollEarly() {
    if (postrollStartedRef.current || finishedRef.current || !postrollSrc || pinned) {
      return;
    }

    const video = primaryVideoRef.current;
    if (!video || !Number.isFinite(video.duration) || video.duration <= 0) {
      return;
    }

    const remaining = video.duration - video.currentTime;
    if (remaining > postrollLeadInSeconds) {
      return;
    }

    activatePostroll();
  }

  function playVideo(video: HTMLVideoElement, context: string) {
    applyMediaGain(video, videoAudioGain);
    void video.play().catch((error) => {
      if (video.muted) {
        console.warn(`[SecondLaw] ${context} was blocked.`, error);
        return;
      }

      video.muted = true;
      void video.play().catch((mutedError) => {
        console.warn(`[SecondLaw] ${context} was blocked even after muting.`, mutedError);
      });
    });
  }

  function randomLoopPlaybackRate() {
    return 0.7 + Math.random() * 0.6;
  }

  function isVideoReady(video: HTMLVideoElement) {
    return video.readyState >= HTMLMediaElement.HAVE_FUTURE_DATA;
  }

  function applyMediaGain(mediaElement: HTMLMediaElement, gainValue: number) {
    if (!Number.isFinite(gainValue) || gainValue === 1) {
      return;
    }

    try {
      const mediaWithGain = mediaElement as MediaElementWithGain;
      const chain = mediaWithGain.__secondLawGainChain ?? createMediaGainChain(mediaWithGain);
      chain.gain.gain.value = gainValue;
      if (chain.context.state === "suspended") {
        void chain.context.resume();
      }
    } catch (error) {
      console.warn("[SecondLaw] Audio gain setup failed.", error);
    }
  }

  function createMediaGainChain(mediaElement: MediaElementWithGain) {
    const AudioContextConstructor = window.AudioContext ?? (window as WindowWithWebkitAudio).webkitAudioContext;
    if (!AudioContextConstructor) {
      throw new Error("Web Audio API is unavailable.");
    }

    const context = new AudioContextConstructor();
    const source = context.createMediaElementSource(mediaElement);
    const gain = context.createGain();
    source.connect(gain);
    gain.connect(context.destination);
    mediaElement.__secondLawGainChain = { context, gain };
    return mediaElement.__secondLawGainChain;
  }

  return (
    <div className={[styles.transitionOverlay, pinned ? styles.transitionOverlayPinned : ""].join(" ")}>
      <video
        ref={primaryVideoRef}
        className={[
          styles.transitionVideo,
          primaryReady && !postrollActive ? styles.transitionVideoReady : ""
        ].join(" ")}
        src={src}
        autoPlay
        playsInline
        loop={false}
        preload="auto"
        onCanPlay={startPrimaryWhenReady}
        onLoadedMetadata={reportVideoProgress}
        onEnded={() => {
          if (!postrollStartedRef.current) {
            activatePostroll();
          }
        }}
        onTimeUpdate={() => {
          updateAudioFade(primaryVideoRef.current, false, audioFadeOutSeconds);
          reportVideoProgress();
          maybeActivatePostrollEarly();
        }}
      />
      {postrollSrc ? (
        <video
          ref={postrollVideoRef}
          className={[
            styles.transitionVideo,
            postrollActive ? styles.transitionVideoReady : ""
          ].join(" ")}
          src={postrollSrc}
          playsInline
          loop={false}
          muted={postrollMuted}
          preload="auto"
          onCanPlay={preparePostrollWhenReady}
          onEnded={postrollLoop ? restartPostrollLoop : undefined}
          onTimeUpdate={() =>
            updateAudioFade(postrollVideoRef.current, Boolean(postrollMuted), postrollAudioFadeOutSeconds)
          }
        />
      ) : null}
    </div>
  );
}
