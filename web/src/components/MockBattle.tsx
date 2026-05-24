import styles from "../App.module.css";

interface MockBattleProps {
  translate: (key: string) => string;
  onVictory: () => void;
  onRetreat: () => void;
}

export default function MockBattle({ translate, onVictory, onRetreat }: MockBattleProps) {
  return (
    <main className={styles.battleMock}>
      <section className={styles.battlePanel}>
        <h1>{translate("battle.mock.title")}</h1>
        <p>{translate("battle.mock.body")}</p>
        <div className={styles.mockArena}>
          <div className={styles.mockHero}>Lv1</div>
          <div className={styles.mockSlime}>3x</div>
        </div>
        <div className={styles.buttonRow}>
          <button className={styles.primaryButton} type="button" onClick={onVictory}>
            {translate("button.mockVictory")}
          </button>
          <button className={styles.secondaryButton} type="button" onClick={onRetreat}>
            {translate("button.mockRetreat")}
          </button>
        </div>
      </section>
    </main>
  );
}
