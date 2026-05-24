import styles from "../App.module.css";

interface TopBarProps {
  translate: (key: string) => string;
  skipTransitions: boolean;
  transitionSpeedLabel: string;
  onToggleLanguage: () => void;
  onToggleSkipTransitions: () => void;
  onCycleTransitionSpeed: () => void;
}

export default function TopBar({
  translate,
  skipTransitions,
  transitionSpeedLabel,
  onToggleLanguage,
  onToggleSkipTransitions,
  onCycleTransitionSpeed
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
      <button className={styles.toolButton} type="button" onClick={onCycleTransitionSpeed}>
        {transitionSpeedLabel}
      </button>
    </div>
  );
}
