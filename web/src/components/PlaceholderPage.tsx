import type { GuildView } from "../types";
import styles from "../App.module.css";

interface PlaceholderPageProps {
  view: Extract<GuildView, "board" | "party" | "shop">;
  translate: (key: string) => string;
  useVideoBackdrop?: boolean;
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

export default function PlaceholderPage({ view, translate, useVideoBackdrop = false, onBack }: PlaceholderPageProps) {
  const keys = pageKeys[view];

  return (
    <main
      className={[
        styles.pageShell,
        useVideoBackdrop ? styles.pageShellOverVideo : "",
        view === "party" ? styles.partyPageShell : ""
      ].join(" ")}
    >
      {useVideoBackdrop ? null : <div className={styles.pageBackdrop} />}
      <section className={[styles.placeholderPanel, view === "party" ? styles.partyPanel : ""].join(" ")}>
        <button className={styles.backButton} type="button" onClick={onBack}>
          {translate("guild.back")}
        </button>
        <h1>{translate(keys.title)}</h1>
        <p>{translate(keys.body)}</p>
        {view === "party" ? (
          <div className={styles.partyOptions}>
            <button className={styles.partyOption} type="button">
              <strong>{translate("guild.party.option.local")}</strong>
              <span>{translate("guild.party.option.local.body")}</span>
            </button>
            <button className={styles.partyOption} type="button">
              <strong>{translate("guild.party.option.online")}</strong>
              <span>{translate("guild.party.option.online.body")}</span>
            </button>
            <button className={styles.partyOption} type="button">
              <strong>{translate("guild.party.option.invite")}</strong>
              <span>{translate("guild.party.option.invite.body")}</span>
            </button>
          </div>
        ) : (
          <div className={styles.placeholderCards}>
            <span />
            <span />
            <span />
          </div>
        )}
      </section>
    </main>
  );
}
