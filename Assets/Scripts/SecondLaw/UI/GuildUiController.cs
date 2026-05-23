using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace SecondLaw
{
    public sealed class GuildUiController : MonoBehaviour
    {
        private const string SkipTransitionKey = "SecondLaw.SkipGuildTransitions";
        private const float CounterTransitionSpeed = 1.5f;

        private SecondLawGame game;
        private UIDocument document;
        private VisualElement documentRoot;
        private VisualElement root;
        private VisualElement hubLayer;
        private VisualElement pageLayer;
        private VisualElement pageBackground;
        private VisualElement pageDim;
        private VisualElement counterPage;
        private VisualElement placeholderPage;
        private VisualElement replyPanel;
        private VisualElement replyColumns;
        private Label statusText;
        private Label letterText;
        private Label placeholderTitle;
        private Label placeholderBody;
        private Toggle skipToggle;
        private Button sendReplyButton;
        private VideoClip counterTransitionClip;
        private VideoPlayer transitionPlayer;
        private bool counterVideoBackgroundActive;
        private bool debugHotspots;
        private bool counterPageVisible;
        private bool counterCompletedQuest;
        private string currentPlaceholderKey;
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
            selectedOpening = null;
            selectedBody = null;
            selectedClosing = null;
            counterVideoBackgroundActive = false;
            StopCounterVideo();

            SetDisplay(documentRoot, true);
            RefreshStaticLabels();

            if (rewardMessages != null)
            {
                ShowCounterPage(IsQuestCompletionMessage(rewardMessages), false);
                return;
            }

            ShowHub();
        }

        public void Hide()
        {
            StopCounterVideo();
            SetDisplay(documentRoot, false);
        }

        private void Build()
        {
            GameObject uiObject = new GameObject("Guild UI Toolkit Document");
            uiObject.SetActive(false);
            uiObject.transform.SetParent(transform, false);

            document = uiObject.AddComponent<UIDocument>();
            document.panelSettings = CreateRuntimePanelSettings();
            uiObject.SetActive(true);

            documentRoot = document.rootVisualElement;
            if (documentRoot == null)
            {
                Debug.LogError("Guild UI Toolkit document did not create a root visual element.");
                return;
            }

            documentRoot.style.flexGrow = 1f;

            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Guild/GuildHub");
            if (tree != null)
            {
                documentRoot.Clear();
                tree.CloneTree(documentRoot);
            }
            else
            {
                Debug.LogWarning("GuildHub.uxml was not found. Building a minimal UI Toolkit fallback.");
                BuildFallbackTree(documentRoot);
            }

            StyleSheet styleSheet = Resources.Load<StyleSheet>("UI/Guild/GuildHub");
            if (styleSheet != null)
            {
                documentRoot.styleSheets.Add(styleSheet);
            }

            counterTransitionClip = Resources.Load<VideoClip>("Video/Guild/lobby-to-desk");
            BindElements();
            ApplyBackgrounds();
            RefreshStaticLabels();
        }

        private static PanelSettings CreateRuntimePanelSettings()
        {
            PanelSettings settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "Second Law Runtime Panel Settings";
            settings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/Guild/UnityDefaultRuntimeTheme");
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1440, 900);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            return settings;
        }

        private void BindElements()
        {
            root = documentRoot.Q<VisualElement>("guild-root");
            hubLayer = documentRoot.Q<VisualElement>("hub-layer");
            pageLayer = documentRoot.Q<VisualElement>("page-layer");
            pageBackground = documentRoot.Q<VisualElement>("page-background");
            pageDim = documentRoot.Q<VisualElement>("page-dim");
            counterPage = documentRoot.Q<VisualElement>("counter-page");
            placeholderPage = documentRoot.Q<VisualElement>("placeholder-page");
            replyPanel = documentRoot.Q<VisualElement>("reply-panel");
            replyColumns = documentRoot.Q<VisualElement>("reply-columns");
            statusText = documentRoot.Q<Label>("status-text");
            letterText = documentRoot.Q<Label>("letter-text");
            placeholderTitle = documentRoot.Q<Label>("placeholder-title");
            placeholderBody = documentRoot.Q<Label>("placeholder-body");
            skipToggle = documentRoot.Q<Toggle>("skip-toggle");
            sendReplyButton = documentRoot.Q<Button>("send-reply-button");

            RegisterHotspot("counter-hotspot", OpenCounterWithTransition);
            RegisterHotspot("board-hotspot", () => ShowPlaceholder("board"));
            RegisterHotspot("table-hotspot", () => ShowPlaceholder("table"));
            RegisterHotspot("shop-hotspot", () => ShowPlaceholder("shop"));

            documentRoot.Q<Button>("language-button").clicked += ToggleLanguage;
            documentRoot.Q<Button>("page-language-button").clicked += ToggleLanguage;
            documentRoot.Q<Button>("debug-button").clicked += ToggleDebugHotspots;
            documentRoot.Q<Button>("back-button").clicked += ShowHub;
            documentRoot.Q<Button>("placeholder-back-button").clicked += ShowHub;
            documentRoot.Q<Button>("start-quest-button").clicked += game.StartQuest;
            documentRoot.Q<Button>("reset-button").clicked += game.ResetProgress;
            sendReplyButton.clicked += SendReply;

            skipToggle.SetValueWithoutNotify(PlayerPrefs.GetInt(SkipTransitionKey, 0) == 1);
            skipToggle.RegisterValueChangedCallback(evt =>
            {
                PlayerPrefs.SetInt(SkipTransitionKey, evt.newValue ? 1 : 0);
                PlayerPrefs.Save();
            });
        }

        private void ApplyBackgrounds()
        {
            Texture2D guildBackground = Resources.Load<Texture2D>("Art/Guild/demo-bg-0");
            if (guildBackground == null)
            {
                return;
            }

            StyleBackground background = new StyleBackground(guildBackground);
            documentRoot.Q<VisualElement>("guild-background").style.backgroundImage = background;
            pageBackground.style.backgroundImage = background;
        }

        private void RegisterHotspot(string name, System.Action onClick)
        {
            VisualElement hotspot = documentRoot.Q<VisualElement>(name);
            hotspot.pickingMode = PickingMode.Position;
            hotspot.RegisterCallback<ClickEvent>(_ => onClick());
        }

        private void RefreshStaticLabels()
        {
            SetButtonText("language-button", LocalizationService.T("language.button"));
            SetButtonText("page-language-button", LocalizationService.T("language.button"));
            SetButtonText("debug-button", LocalizationService.T("guild.debug_hotspots"));
            SetButtonText("back-button", LocalizationService.T("guild.back"));
            SetButtonText("placeholder-back-button", LocalizationService.T("guild.back"));
            SetButtonText("start-quest-button", LocalizationService.T("button.accept"));
            SetButtonText("reset-button", LocalizationService.T("button.reset"));
            SetButtonText("send-reply-button", LocalizationService.T("reply.send"));

            skipToggle.label = LocalizationService.T("guild.skip_transition");
            SetLabelText("counter-hotspot-label", LocalizationService.T("guild.hotspot.counter"));
            SetLabelText("board-hotspot-label", LocalizationService.T("guild.hotspot.board"));
            SetLabelText("table-hotspot-label", LocalizationService.T("guild.hotspot.table"));
            SetLabelText("shop-hotspot-label", LocalizationService.T("guild.hotspot.shop"));
            SetLabelText("counter-title", LocalizationService.T("guild.counter.title"));
            SetLabelText("status-title", LocalizationService.T("guild.counter.status"));
            SetLabelText("letter-title", LocalizationService.T("guild.counter.letter"));
        }

        private void ShowHub()
        {
            counterPageVisible = false;
            currentPlaceholderKey = null;
            counterVideoBackgroundActive = false;
            StopCounterVideo();

            SetDisplay(hubLayer, true);
            SetDisplay(pageLayer, false);
            SetDisplay(counterPage, false);
            SetDisplay(placeholderPage, false);
            SetDisplay(pageBackground, false);
            SetDisplay(pageDim, false);
        }

        private void OpenCounterWithTransition()
        {
            if (transitionPlayer != null && transitionPlayer.isPlaying)
            {
                return;
            }

            if (skipToggle.value || counterTransitionClip == null)
            {
                ShowCounterPage(false, false);
                return;
            }

            PlayCounterTransition();
        }

        private void PlayCounterTransition()
        {
            Camera targetCamera = Camera.main;
            if (targetCamera == null)
            {
                ShowCounterPage(false, false);
                return;
            }

            GameObject videoObject = new GameObject("Guild Counter Transition Video");
            videoObject.transform.SetParent(transform, false);
            transitionPlayer = videoObject.AddComponent<VideoPlayer>();
            transitionPlayer.clip = counterTransitionClip;
            transitionPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            transitionPlayer.targetCamera = targetCamera;
            transitionPlayer.aspectRatio = VideoAspectRatio.FitVertically;
            transitionPlayer.playbackSpeed = CounterTransitionSpeed;
            transitionPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            transitionPlayer.isLooping = false;
            transitionPlayer.loopPointReached += OnCounterTransitionComplete;
            transitionPlayer.Play();

            SetDisplay(documentRoot, false);
        }

        private void OnCounterTransitionComplete(VideoPlayer player)
        {
            if (transitionPlayer != null)
            {
                transitionPlayer.Pause();
            }

            counterVideoBackgroundActive = true;
            SetDisplay(documentRoot, true);
            ShowCounterPage(false, true);
        }

        private void ShowCounterPage(bool completedQuest, bool useVideoBackground)
        {
            counterPageVisible = true;
            counterCompletedQuest = completedQuest;
            currentPlaceholderKey = null;
            counterVideoBackgroundActive = useVideoBackground;

            SetDisplay(hubLayer, false);
            SetDisplay(pageLayer, true);
            SetDisplay(counterPage, true);
            SetDisplay(placeholderPage, false);
            SetDisplay(pageBackground, !useVideoBackground);
            SetDisplay(pageDim, true);

            RefreshCounterPage(completedQuest);
        }

        private void RefreshCounterPage(bool completedQuest)
        {
            SetLabelText("quest-title", LocalizationService.T("quest.slime.title"));

            string questBodyText = LocalizationService.T("quest.slime.description") + "\n\n" +
                LocalizationService.T("quest.target") + ": " + game.CurrentQuest.targetCount + " " + LocalizationService.T("quest.slimes") + "\n" +
                LocalizationService.T("quest.reward") + ": " + game.CurrentQuest.rewardExperience + " " + LocalizationService.T("reward.exp") + " / " + game.CurrentQuest.rewardGold + " " + LocalizationService.T("reward.gold");
            SetLabelText("quest-body", questBodyText);

            RefreshStatus(lastRewardMessages);
            RefreshLetter(completedQuest);
        }

        private void ShowPlaceholder(string key)
        {
            counterPageVisible = false;
            currentPlaceholderKey = key;
            counterVideoBackgroundActive = false;
            StopCounterVideo();

            SetDisplay(hubLayer, true);
            SetDisplay(pageLayer, true);
            SetDisplay(counterPage, false);
            SetDisplay(placeholderPage, true);
            SetDisplay(pageBackground, false);
            SetDisplay(pageDim, true);
            RefreshPlaceholder();
        }

        private void RefreshPlaceholder()
        {
            if (string.IsNullOrEmpty(currentPlaceholderKey))
            {
                return;
            }

            placeholderTitle.text = LocalizationService.T("guild.placeholder." + currentPlaceholderKey + ".title");
            placeholderBody.text = LocalizationService.T("guild.placeholder." + currentPlaceholderKey + ".body");
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
                SetDisplay(replyPanel, false);
                return;
            }

            LetterTemplate template = game.CurrentQuest.completionLetter;
            letterText.text = LetterCacheService.GetOrCreateLetter(template);
            BuildReplyChoices(template);
        }

        private void BuildReplyChoices(LetterTemplate template)
        {
            selectedOpening = null;
            selectedBody = null;
            selectedClosing = null;
            SetDisplay(replyPanel, true);
            replyColumns.Clear();

            AddChoiceColumn(LocalizationService.T("reply.opening"), LocalizationService.ReplyOpenings(), value => selectedOpening = value);
            AddChoiceColumn(LocalizationService.T("reply.body"), LocalizationService.ReplyBodies(), value => selectedBody = value);
            AddChoiceColumn(LocalizationService.T("reply.closing"), LocalizationService.ReplyClosings(), value => selectedClosing = value);
        }

        private void AddChoiceColumn(string title, string[] values, System.Action<string> onPick)
        {
            VisualElement column = new VisualElement();
            column.AddToClassList("reply-column");
            replyColumns.Add(column);

            Label heading = new Label(title);
            heading.AddToClassList("reply-heading");
            column.Add(heading);

            for (int i = 0; i < values.Length && i < 3; i++)
            {
                string value = values[i];
                Button button = new Button(() => onPick(value))
                {
                    text = Shorten(value)
                };
                button.AddToClassList("reply-choice");
                column.Add(button);
            }
        }

        private void SendReply()
        {
            if (string.IsNullOrEmpty(selectedOpening) || string.IsNullOrEmpty(selectedBody) || string.IsNullOrEmpty(selectedClosing))
            {
                letterText.text = LocalizationService.T("reply.need_choices");
                return;
            }

            LetterTemplate template = game.CurrentQuest.completionLetter;
            game.Progression.ApplyReply(template);
            letterText.text = selectedOpening + "\n\n" + selectedBody + "\n\n" + selectedClosing + "\n\n" + LocalizationService.T("reply.sent") + template.affectionReward;
            RefreshStatus(null);
            SetDisplay(replyPanel, false);
        }

        private void ToggleLanguage()
        {
            LocalizationService.ToggleLanguage();
            RefreshStaticLabels();

            if (counterPageVisible)
            {
                RefreshCounterPage(counterCompletedQuest);
            }
            else if (!string.IsNullOrEmpty(currentPlaceholderKey))
            {
                RefreshPlaceholder();
            }
        }

        private void ToggleDebugHotspots()
        {
            debugHotspots = !debugHotspots;
            if (debugHotspots)
            {
                root.AddToClassList("show-hotspots");
            }
            else
            {
                root.RemoveFromClassList("show-hotspots");
            }
        }

        private void StopCounterVideo()
        {
            if (transitionPlayer == null)
            {
                return;
            }

            transitionPlayer.loopPointReached -= OnCounterTransitionComplete;
            Destroy(transitionPlayer.gameObject);
            transitionPlayer = null;
        }

        private bool IsQuestCompletionMessage(IReadOnlyList<string> rewardMessages)
        {
            if (rewardMessages == null || rewardMessages.Count == 0)
            {
                return false;
            }

            string reset = LocalizationService.T("demo.reset");
            string retreat = LocalizationService.T("battle.retreat");
            for (int i = 0; i < rewardMessages.Count; i++)
            {
                if (rewardMessages[i] == reset || rewardMessages[i] == retreat)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetButtonText(string name, string text)
        {
            Button button = documentRoot.Q<Button>(name);
            if (button != null)
            {
                button.text = text;
            }
        }

        private void SetLabelText(string name, string text)
        {
            Label label = documentRoot.Q<Label>(name);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static string Shorten(string value)
        {
            return value.Length <= 28 ? value : value.Substring(0, 25) + "...";
        }

        private void BuildFallbackTree(VisualElement parent)
        {
            VisualElement fallbackRoot = new VisualElement { name = "guild-root" };
            fallbackRoot.AddToClassList("guild-root");
            parent.Add(fallbackRoot);

            hubLayer = new VisualElement { name = "hub-layer" };
            hubLayer.AddToClassList("screen-layer");
            fallbackRoot.Add(hubLayer);

            VisualElement background = new VisualElement { name = "guild-background" };
            background.AddToClassList("guild-background");
            hubLayer.Add(background);

            foreach (string hotspotName in new[] { "counter-hotspot", "board-hotspot", "table-hotspot", "shop-hotspot" })
            {
                VisualElement hotspot = new VisualElement { name = hotspotName };
                hotspot.AddToClassList("hotspot");
                hotspot.AddToClassList(hotspotName);
                hubLayer.Add(hotspot);
            }
        }
    }
}
