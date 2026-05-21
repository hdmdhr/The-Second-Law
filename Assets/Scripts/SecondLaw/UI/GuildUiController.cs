using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SecondLaw
{
    public sealed class GuildUiController : MonoBehaviour
    {
        private SecondLawGame game;
        private Canvas canvas;
        private Text statusText;
        private Text letterText;
        private RectTransform replyPanel;
        private string selectedOpening;
        private string selectedBody;
        private string selectedClosing;
        private IReadOnlyList<string> lastRewardMessages;

        public static GuildUiController Create(SecondLawGame game)
        {
            GameObject root = new GameObject("Guild UI");
            GuildUiController controller = root.AddComponent<GuildUiController>();
            controller.game = game;
            controller.Build();
            return controller;
        }

        public void Show(IReadOnlyList<string> rewardMessages = null)
        {
            lastRewardMessages = rewardMessages;
            canvas.gameObject.SetActive(true);
            selectedOpening = null;
            selectedBody = null;
            selectedClosing = null;
            RefreshStatus(rewardMessages);
            RefreshLetter(rewardMessages != null);
        }

        public void Hide()
        {
            canvas.gameObject.SetActive(false);
        }

        private void Build()
        {
            canvas = RuntimeUi.CreateCanvas("Guild Canvas");
            canvas.transform.SetParent(transform, false);

            RectTransform background = RuntimeUi.AddPanel(canvas.transform, "Guild Background", new Color(0.13f, 0.10f, 0.08f, 1f));
            RuntimeUi.Stretch(background, 0f, 0f, 0f, 0f);

            RectTransform artBand = RuntimeUi.AddPanel(canvas.transform, "Guild Illustration", new Color(0.28f, 0.18f, 0.12f, 1f));
            RuntimeUi.SetRect(artBand, new Vector2(0f, 0.57f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            Text title = RuntimeUi.AddText(canvas.transform, "Title", LocalizationService.T("guild.title"), 38, new Color(1f, 0.91f, 0.72f), TextAnchor.UpperLeft);
            RuntimeUi.SetRect(title.rectTransform, new Vector2(0.05f, 0.83f), new Vector2(0.78f, 0.97f), Vector2.zero, Vector2.zero);

            Text tagline = RuntimeUi.AddText(canvas.transform, "Tagline", LocalizationService.T("guild.tagline"), 21, new Color(0.83f, 0.76f, 0.64f));
            RuntimeUi.SetRect(tagline.rectTransform, new Vector2(0.05f, 0.76f), new Vector2(0.75f, 0.82f), Vector2.zero, Vector2.zero);

            Button language = RuntimeUi.AddButton(canvas.transform, "Language Toggle", LocalizationService.T("language.button"), new Color(0.18f, 0.22f, 0.28f));
            RuntimeUi.SetRect(language.GetComponent<RectTransform>(), new Vector2(0.81f, 0.88f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            language.onClick.AddListener(ToggleLanguage);

            RectTransform questPanel = RuntimeUi.AddPanel(canvas.transform, "Quest Panel", new Color(0.17f, 0.16f, 0.15f, 0.95f));
            RuntimeUi.SetRect(questPanel, new Vector2(0.05f, 0.09f), new Vector2(0.48f, 0.56f), Vector2.zero, Vector2.zero);

            Text questTitle = RuntimeUi.AddText(questPanel, "Quest Title", LocalizationService.T("quest.slime.title"), 28, new Color(1f, 0.92f, 0.68f));
            RuntimeUi.SetRect(questTitle.rectTransform, new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.93f), Vector2.zero, Vector2.zero);

            string questBodyText = LocalizationService.T("quest.slime.description") + "\n\n" +
                LocalizationService.T("quest.target") + ": " + game.CurrentQuest.targetCount + " " + LocalizationService.T("quest.slimes") + "\n" +
                LocalizationService.T("quest.reward") + ": " + game.CurrentQuest.rewardExperience + " " + LocalizationService.T("reward.exp") + " / " + game.CurrentQuest.rewardGold + " " + LocalizationService.T("reward.gold");
            Text questBody = RuntimeUi.AddText(questPanel, "Quest Body", questBodyText, 22, Color.white);
            RuntimeUi.SetRect(questBody.rectTransform, new Vector2(0.05f, 0.32f), new Vector2(0.95f, 0.73f), Vector2.zero, Vector2.zero);

            Button start = RuntimeUi.AddButton(questPanel, "Start Quest", LocalizationService.T("button.accept"), new Color(0.49f, 0.22f, 0.17f));
            RuntimeUi.SetRect(start.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.24f), Vector2.zero, Vector2.zero);
            start.onClick.AddListener(game.StartQuest);

            Button reset = RuntimeUi.AddButton(questPanel, "Reset Progress", LocalizationService.T("button.reset"), new Color(0.20f, 0.22f, 0.25f));
            RuntimeUi.SetRect(reset.GetComponent<RectTransform>(), new Vector2(0.52f, 0.08f), new Vector2(0.95f, 0.24f), Vector2.zero, Vector2.zero);
            reset.onClick.AddListener(game.ResetProgress);

            RectTransform statusPanel = RuntimeUi.AddPanel(canvas.transform, "Status Panel", new Color(0.10f, 0.12f, 0.14f, 0.95f));
            RuntimeUi.SetRect(statusPanel, new Vector2(0.52f, 0.36f), new Vector2(0.95f, 0.56f), Vector2.zero, Vector2.zero);
            statusText = RuntimeUi.AddText(statusPanel, "Status Text", string.Empty, 20, Color.white);
            RuntimeUi.Stretch(statusText.rectTransform, 24f, 18f, 24f, 18f);

            RectTransform letterPanelRoot = RuntimeUi.AddPanel(canvas.transform, "Letter Panel", new Color(0.94f, 0.88f, 0.75f, 0.97f));
            RuntimeUi.SetRect(letterPanelRoot, new Vector2(0.52f, 0.09f), new Vector2(0.95f, 0.34f), Vector2.zero, Vector2.zero);
            letterText = RuntimeUi.AddText(letterPanelRoot, "Letter Text", string.Empty, 18, new Color(0.13f, 0.11f, 0.09f));
            RuntimeUi.Stretch(letterText.rectTransform, 22f, 18f, 22f, 18f);

            replyPanel = RuntimeUi.AddPanel(canvas.transform, "Reply Panel", new Color(0.12f, 0.12f, 0.13f, 0.96f));
            RuntimeUi.SetRect(replyPanel, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.74f), Vector2.zero, Vector2.zero);
            replyPanel.gameObject.SetActive(false);
        }

        private void RefreshStatus(IReadOnlyList<string> rewardMessages)
        {
            ProgressionState state = game.Progression.State;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(LocalizationService.T("status.level") + " " + state.level + "   " + LocalizationService.T("reward.exp") + " " + state.experience + "/" + game.Progression.ExperienceForNextLevel(state.level));
            builder.AppendLine(LocalizationService.T("status.gold") + " " + state.gold + "   " + LocalizationService.T("status.reputation") + " " + state.reputation + "   " + LocalizationService.T("status.talent_points") + " " + state.talentPoints + "   " + LocalizationService.T("status.affection") + " " + state.affection);

            if (rewardMessages != null)
            {
                builder.AppendLine();
                for (int i = 0; i < rewardMessages.Count; i++)
                {
                    builder.AppendLine(rewardMessages[i]);
                }
            }

            statusText.text = builder.ToString();
        }

        private void RefreshLetter(bool completedQuest)
        {
            if (!completedQuest)
            {
                letterText.text = LocalizationService.T("letter.empty");
                replyPanel.gameObject.SetActive(false);
                return;
            }

            LetterTemplate template = game.CurrentQuest.completionLetter;
            letterText.text = LetterCacheService.GetOrCreateLetter(template);
            BuildReplyChoices(template);
        }

        private void BuildReplyChoices(LetterTemplate template)
        {
            replyPanel.gameObject.SetActive(true);
            for (int i = replyPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(replyPanel.GetChild(i).gameObject);
            }

            AddChoiceColumn(LocalizationService.T("reply.opening"), LocalizationService.ReplyOpenings(), 0, value => selectedOpening = value);
            AddChoiceColumn(LocalizationService.T("reply.body"), LocalizationService.ReplyBodies(), 1, value => selectedBody = value);
            AddChoiceColumn(LocalizationService.T("reply.closing"), LocalizationService.ReplyClosings(), 2, value => selectedClosing = value);

            Button send = RuntimeUi.AddButton(replyPanel, "Send Reply", LocalizationService.T("reply.send"), new Color(0.42f, 0.30f, 0.14f));
            RuntimeUi.SetRect(send.GetComponent<RectTransform>(), new Vector2(0.78f, 0.16f), new Vector2(0.97f, 0.84f), Vector2.zero, Vector2.zero);
            send.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(selectedOpening) || string.IsNullOrEmpty(selectedBody) || string.IsNullOrEmpty(selectedClosing))
                {
                    letterText.text = LocalizationService.T("reply.need_choices");
                    return;
                }

                game.Progression.ApplyReply(template);
                letterText.text = selectedOpening + "\n\n" + selectedBody + "\n\n" + selectedClosing + "\n\n" + LocalizationService.T("reply.sent") + template.affectionReward;
                RefreshStatus(null);
                replyPanel.gameObject.SetActive(false);
            });
        }

        private void AddChoiceColumn(string title, string[] values, int column, System.Action<string> onPick)
        {
            Text heading = RuntimeUi.AddText(replyPanel, title, title, 16, new Color(1f, 0.88f, 0.62f), TextAnchor.MiddleCenter);
            float minX = 0.02f + column * 0.25f;
            float maxX = minX + 0.22f;
            RuntimeUi.SetRect(heading.rectTransform, new Vector2(minX, 0.70f), new Vector2(maxX, 0.96f), Vector2.zero, Vector2.zero);

            for (int i = 0; i < values.Length && i < 3; i++)
            {
                string value = values[i];
                Button button = RuntimeUi.AddButton(replyPanel, title + i, Shorten(value), new Color(0.22f, 0.24f, 0.28f));
                RuntimeUi.SetRect(button.GetComponent<RectTransform>(), new Vector2(minX, 0.08f + i * 0.20f), new Vector2(maxX, 0.24f + i * 0.20f), Vector2.zero, Vector2.zero);
                button.onClick.AddListener(() => onPick(value));
            }
        }

        private static string Shorten(string value)
        {
            return value.Length <= 28 ? value : value.Substring(0, 25) + "...";
        }

        private void ToggleLanguage()
        {
            LocalizationService.ToggleLanguage();
            Destroy(canvas.gameObject);
            Build();
            Show(lastRewardMessages);
        }
    }
}
