import type { GuildView } from "../types";
import styles from "../App.module.css";

interface PlaceholderPageProps {
  view: Extract<GuildView, "board" | "party" | "shop">;
  translate: (key: string) => string;
  onBack: () => void;
}

const pageKeys = {
  board: {
    title: "guild.placeholder.board.title",
    body: "guild.placeholder.board.body"
  },
  party: {
    title: "guild.placeholder.party.title",
    body: "guild.placeholder.party.body"
  },
  shop: {
    title: "guild.placeholder.shop.title",
    body: "guild.placeholder.shop.body"
  }
} as const;

export default function PlaceholderPage({ view, translate, onBack }: PlaceholderPageProps) {
  const keys = pageKeys[view];

  return (
    <main className={styles.pageShell}>
      <div className={styles.pageBackdrop} />
      <section className={styles.placeholderPanel}>
        <button className={styles.backButton} type="button" onClick={onBack}>
          {translate("guild.back")}
        </button>
        <h1>{translate(keys.title)}</h1>
        <p>{translate(keys.body)}</p>
        <div className={styles.placeholderCards}>
          <span />
          <span />
          <span />
        </div>
      </section>
    </main>
  );
}
