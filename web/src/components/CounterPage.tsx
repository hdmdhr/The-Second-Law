import { firstQuest, slimeLetter } from "../data/demoData";
import type { ProgressionState } from "../types";
import styles from "../App.module.css";

interface CounterPageProps {
  translate: (key: string) => string;
  progression: ProgressionState;
  rewardMessages: string[];
  replyMessage: string;
  selectedOpening: string;
  selectedBody: string;
  selectedClosing: string;
  onPickOpening: (value: string) => void;
  onPickBody: (value: string) => void;
  onPickClosing: (value: string) => void;
  onSendReply: () => void;
  onStartQuest: () => void;
  onReset: () => void;
  onBack: () => void;
}

export default function CounterPage({
  translate,
  progression,
  rewardMessages,
  replyMessage,
  selectedOpening,
  selectedBody,
  selectedClosing,
  onPickOpening,
  onPickBody,
  onPickClosing,
  onSendReply,
  onStartQuest,
  onReset,
  onBack
}: CounterPageProps) {
  const hasLetter = rewardMessages.length > 0 || replyMessage.length > 0;

  return (
    <main className={styles.pageShell}>
      <div className={styles.pageBackdrop} />
      <section className={styles.pagePanelWide}>
        <header className={styles.pageHeader}>
          <button className={styles.backButton} type="button" onClick={onBack}>
            {translate("guild.back")}
          </button>
          <div>
            <h1>{translate("guild.counter.title")}</h1>
            <p>{translate(firstQuest.clientKey)}</p>
          </div>
        </header>

        <div className={styles.counterGrid}>
          <article className={styles.panel}>
            <h2>{translate(firstQuest.titleKey)}</h2>
            <p>{translate(firstQuest.descriptionKey)}</p>
            <dl className={styles.questFacts}>
              <div>
                <dt>{translate("quest.target")}</dt>
                <dd>
                  {firstQuest.targetCount} {translate(firstQuest.targetMonsterKey)}
                </dd>
              </div>
              <div>
                <dt>{translate("quest.reward")}</dt>
                <dd>
                  +{firstQuest.rewardExperience} {translate("reward.exp")} / +{firstQuest.rewardGold}{" "}
                  {translate("reward.gold")}
                </dd>
              </div>
            </dl>
            <div className={styles.buttonRow}>
              <button className={styles.primaryButton} type="button" onClick={onStartQuest}>
                {translate("button.accept")}
              </button>
              <button className={styles.secondaryButton} type="button" onClick={onReset}>
                {translate("button.reset")}
              </button>
            </div>
          </article>

          <article className={styles.panel}>
            <h2>{translate("guild.counter.status")}</h2>
            <dl className={styles.statusList}>
              <div>
                <dt>{translate("status.level")}</dt>
                <dd>{progression.level}</dd>
              </div>
              <div>
                <dt>{translate("status.gold")}</dt>
                <dd>{progression.gold}</dd>
              </div>
              <div>
                <dt>{translate("status.reputation")}</dt>
                <dd>{progression.reputation}</dd>
              </div>
              <div>
                <dt>{translate("status.talent_points")}</dt>
                <dd>{progression.talentPoints}</dd>
              </div>
              <div>
                <dt>{translate("status.affection")}</dt>
                <dd>{progression.affection}</dd>
              </div>
            </dl>
            {rewardMessages.length > 0 ? (
              <ul className={styles.rewardList}>
                {rewardMessages.map((message) => (
                  <li key={message}>{message}</li>
                ))}
              </ul>
            ) : null}
          </article>

          <article className={[styles.panel, styles.letterPanel].join(" ")}>
            <h2>{translate("guild.counter.letter")}</h2>
            <div className={styles.letterText}>
              {hasLetter ? (
                <>
                  <p className={styles.sender}>{translate(slimeLetter.senderKey)}</p>
                  <p>{replyMessage || translate(slimeLetter.bodyKey)}</p>
                </>
              ) : (
                <p>{translate("letter.empty")}</p>
              )}
            </div>
            {hasLetter ? (
              <div className={styles.replyGrid}>
                <ReplyColumn
                  title={translate("reply.opening")}
                  values={slimeLetter.replyOpeningKeys.map(translate)}
                  selected={selectedOpening}
                  onPick={onPickOpening}
                />
                <ReplyColumn
                  title={translate("reply.body")}
                  values={slimeLetter.replyBodyKeys.map(translate)}
                  selected={selectedBody}
                  onPick={onPickBody}
                />
                <ReplyColumn
                  title={translate("reply.closing")}
                  values={slimeLetter.replyClosingKeys.map(translate)}
                  selected={selectedClosing}
                  onPick={onPickClosing}
                />
                <button className={styles.primaryButton} type="button" onClick={onSendReply}>
                  {translate("reply.send")}
                </button>
              </div>
            ) : null}
          </article>
        </div>
      </section>
    </main>
  );
}

interface ReplyColumnProps {
  title: string;
  values: string[];
  selected: string;
  onPick: (value: string) => void;
}

function ReplyColumn({ title, values, selected, onPick }: ReplyColumnProps) {
  return (
    <div className={styles.replyColumn}>
      <h3>{title}</h3>
      {values.map((value) => (
        <button
          key={value}
          className={[styles.replyChoice, selected === value ? styles.replyChoiceSelected : ""].join(" ")}
          type="button"
          onClick={() => onPick(value)}
        >
          {value}
        </button>
      ))}
    </div>
  );
}
