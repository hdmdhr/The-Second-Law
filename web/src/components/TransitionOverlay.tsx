import { useEffect, useRef, useState } from "react";
import styles from "../App.module.css";

interface TransitionOverlayProps {
  src: string;
  pinned: boolean;
  loop?: boolean;
  muted?: boolean;
  audioFadeOutSeconds?: number;
  playbackRate?: number;
  onDone: () => void;
}

export default function TransitionOverlay({
  src,
  pinned,
  loop = false,
  muted = false,
  audioFadeOutSeconds,
  playbackRate = 1,
  onDone
}: TransitionOverlayProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const finishedRef = useRef(false);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (pinned || loop) {
      return;
    }

    const timeout = window.setTimeout(() => {
      if (!finishedRef.current) {
        finishedRef.current = true;
        onDone();
      }
    }, 12000);

    return () => window.clearTimeout(timeout);
  }, [loop, onDone, pinned]);

  useEffect(() => {
    setReady(false);
    finishedRef.current = false;
  }, [src]);

  useEffect(() => {
    if (videoRef.current) {
      videoRef.current.playbackRate = playbackRate;
      videoRef.current.muted = muted;
      videoRef.current.volume = 1;
    }
  }, [muted, playbackRate, src]);

  function finish() {
    if (finishedRef.current) {
      return;
    }

    finishedRef.current = true;
    onDone();
  }

  function restartLoop() {
    const video = videoRef.current;
    if (!video) {
      return;
    }

    video.currentTime = 0;
    video.volume = 1;
    void video.play().catch((error) => {
      console.warn("[SecondLaw] Loop playback was blocked.", error);
    });
  }

  function updateLoopAudioFade() {
    const video = videoRef.current;
    if (!video || muted || !loop || !audioFadeOutSeconds || !Number.isFinite(video.duration) || video.duration <= 0) {
      return;
    }

    const remaining = video.duration - video.currentTime;
    if (remaining <= audioFadeOutSeconds) {
      video.volume = Math.max(0, remaining / audioFadeOutSeconds);
      return;
    }

    video.volume = 1;
  }

  function startWhenReady() {
    setReady(true);
    if (videoRef.current) {
      videoRef.current.playbackRate = playbackRate;
      videoRef.current.muted = muted;
      videoRef.current.volume = 1;
    }

    if (pinned && !loop) {
      return;
    }

    void videoRef.current?.play().catch((error) => {
      console.warn("[SecondLaw] Counter transition playback was blocked.", error);
    });
  }

  return (
    <div className={[styles.transitionOverlay, pinned ? styles.transitionOverlayPinned : ""].join(" ")}>
      <video
        ref={videoRef}
        className={[styles.transitionVideo, ready || pinned ? styles.transitionVideoReady : ""].join(" ")}
        src={src}
        autoPlay
        playsInline
        loop={false}
        muted={muted}
        preload="auto"
        onCanPlay={startWhenReady}
        onEnded={loop ? restartLoop : finish}
        onTimeUpdate={updateLoopAudioFade}
      />
    </div>
  );
}
