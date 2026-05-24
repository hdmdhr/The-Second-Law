import { useEffect, useRef, useState } from "react";
import styles from "../App.module.css";

interface TransitionOverlayProps {
  src: string;
  pinned: boolean;
  loop?: boolean;
  playbackRate?: number;
  onDone: () => void;
}

export default function TransitionOverlay({ src, pinned, loop = false, playbackRate = 1, onDone }: TransitionOverlayProps) {
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
    }
  }, [playbackRate, src]);

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
    void video.play().catch((error) => {
      console.warn("[SecondLaw] Table loop playback was blocked.", error);
    });
  }

  function startWhenReady() {
    setReady(true);
    if (videoRef.current) {
      videoRef.current.playbackRate = playbackRate;
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
        muted={loop}
        preload="auto"
        onCanPlay={startWhenReady}
        onEnded={loop ? restartLoop : finish}
      />
    </div>
  );
}
