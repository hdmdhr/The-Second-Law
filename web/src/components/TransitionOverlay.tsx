import { useEffect, useRef, useState } from "react";
import { guildAssets } from "../data/demoData";
import styles from "../App.module.css";

interface TransitionOverlayProps {
  pinned: boolean;
  onDone: () => void;
}

export default function TransitionOverlay({ pinned, onDone }: TransitionOverlayProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const finishedRef = useRef(false);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (pinned) {
      return;
    }

    const timeout = window.setTimeout(() => {
      if (!finishedRef.current) {
        finishedRef.current = true;
        onDone();
      }
    }, 12000);

    return () => window.clearTimeout(timeout);
  }, [onDone, pinned]);

  function finish() {
    if (finishedRef.current) {
      return;
    }

    finishedRef.current = true;
    onDone();
  }

  function startWhenReady() {
    setReady(true);
    if (pinned) {
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
        src={guildAssets.counterVideo}
        autoPlay
        playsInline
        preload="auto"
        onCanPlay={startWhenReady}
        onEnded={finish}
      />
    </div>
  );
}
