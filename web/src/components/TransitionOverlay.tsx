import { useEffect, useRef } from "react";
import { guildAssets } from "../data/demoData";
import styles from "../App.module.css";

interface TransitionOverlayProps {
  onDone: () => void;
}

export default function TransitionOverlay({ onDone }: TransitionOverlayProps) {
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const timeout = window.setTimeout(onDone, 9000);
    return () => window.clearTimeout(timeout);
  }, [onDone]);

  return (
    <div className={styles.transitionOverlay}>
      <video
        ref={videoRef}
        className={styles.transitionVideo}
        src={guildAssets.counterVideo}
        autoPlay
        playsInline
        muted
        onEnded={onDone}
      />
    </div>
  );
}
