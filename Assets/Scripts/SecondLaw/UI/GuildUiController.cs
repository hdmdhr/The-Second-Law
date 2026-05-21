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

            Text title = RuntimeUi.AddText(canvas.transform, "Title", "Adventurers' Guild - Second Law Demo", 38, new Color(1f, 0.91f, 0.72f), TextAnchor.UpperLeft);
            RuntimeUi.SetRect(title.rectTransform, new Vector2(0.05f, 0.83f), new Vector2(0.78f, 0.97f), Vector2.zero, Vector2.zero);

            Text tagline = RuntimeUi.AddText(canvas.transform, "Tagline", "F-rank counter / local prototype / cached letter loop", 21, new Color(0.83f, 0.76f, 0.64f));
            RuntimeUi.SetRect(tagline.rectTransform, new Vector2(0.05f, 0.76f), new Vector2(0.75f, 0.82f), Vector2.zero, Vector2.zero);

            RectTransform questPanel = RuntimeUi.AddPanel(canvas.transform, "Quest Panel", new Color(0.17f, 0.16f, 0.15f, 0.95f));
            RuntimeUi.SetRect(questPanel, new Vector2(0.05f, 0.09f), new Vector2(0.48f, 0.56f), Vector2.zero, Vector2.zero);

            Text questTitle = RuntimeUi.AddText(questPanel, "Quest Title", game.CurrentQuest.title, 28, new Color(1f, 0.92f, 0.68f));
            RuntimeUi.SetRect(questTitle.rectTransform, new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.93f), Vector2.zero, Vector2.zero);

            Text questBody = RuntimeUi.AddText(questPanel, "Quest Body", game.CurrentQuest.description + "\n\nTarget: " + game.CurrentQuest.targetCount + " slimes\nReward: " + game.CurrentQuest.rewardExperience + " EXP / " + game.CurrentQuest.rewardGold + " Gold", 22, Color.white);
            RuntimeUi.SetRect(questBody.rectTransform, new Vector2(0.05f, 0.32f), new Vector2(0.95f, 0.73f), Vector2.zero, Vector2.zero);

            Button start = RuntimeUi.AddButton(questPanel, "Start Quest", "Accept Request", new Color(0.49f, 0.22f, 0.17f));
            RuntimeUi.SetRect(start.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.24f), Vector2.zero, Vector2.zero);
            start.onClick.AddListener(game.StartQuest);

            Button reset = RuntimeUi.AddButton(questPanel, "Reset Progress", "Reset Demo", new Color(0.20f, 0.22f, 0.25f));
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
            builder.AppendLine("Level " + state.level + "   EXP " + state.experience + "/" + game.Progression.ExperienceForNextLevel(state.level));
            builder.AppendLine("Gold " + state.gold + "   Reputation " + state.reputation + "   Talent Points " + state.talentPoints + "   Affection " + state.affection);

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
                letterText.text = "Complete a request to receive a client letter.";
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

            AddChoiceColumn("Opening", template.replyOpenings, 0, value => selectedOpening = value);
            AddChoiceColumn("Body", template.replyBodies, 1, value => selectedBody = value);
            AddChoiceColumn("Closing", template.replyClosings, 2, value => selectedClosing = value);

            Button send = RuntimeUi.AddButton(replyPanel, "Send Reply", "Send Reply", new Color(0.42f, 0.30f, 0.14f));
            RuntimeUi.SetRect(send.GetComponent<RectTransform>(), new Vector2(0.78f, 0.16f), new Vector2(0.97f, 0.84f), Vector2.zero, Vector2.zero);
            send.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(selectedOpening) || string.IsNullOrEmpty(selectedBody) || string.IsNullOrEmpty(selectedClosing))
                {
                    letterText.text = "Choose one opening, body, and closing before sending.";
                    return;
                }

                game.Progression.ApplyReply(template);
                letterText.text = selectedOpening + "\n\n" + selectedBody + "\n\n" + selectedClosing + "\n\nReply sent. Affection +" + template.affectionReward;
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
    }
}
