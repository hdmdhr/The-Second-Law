import type { Language } from "../types";
import styles from "../App.module.css";

interface TopBarProps {
  language: Language;
  translate: (key: string) => string;
  skipTransitions: boolean;
  debugHotspots: boolean;
  onToggleLanguage: () => void;
  onToggleSkipTransitions: () => void;
  onToggleDebugHotspots: () => void;
}

export default function TopBar({
  translate,
  skipTransitions,
  debugHotspots,
  onToggleLanguage,
  onToggleSkipTransitions,
  onToggleDebugHotspots
}: TopBarProps) {
  return (
    <div className={styles.topBar}>
      <button className={styles.toolButton} type="button" onClick={onToggleLanguage}>
        {translate("language.button")}
      </button>
      <label className={styles.togglePill}>
        <input type="checkbox" checked={skipTransitions} onChange={onToggleSkipTransitions} />
        <span>{translate("guild.skip_transition")}</span>
      </label>
      <button
        className={[styles.toolButton, debugHotspots ? styles.toolButtonActive : ""].join(" ")}
        type="button"
        onClick={onToggleDebugHotspots}
      >
        {translate("guild.debug_hotspots")}
      </button>
    </div>
  );
}
