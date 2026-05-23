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
        private const float HotspotAlphaThreshold = 0.2f;
        private const int HotspotOutlineRadius = 5;

        private SecondLawGame game;
        private readonly List<GuildHotspot> hotspots = new List<GuildHotspot>();
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
        private bool preparingCounterTransition;
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
            ApplyInlineLayout();
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
            Sprite guildBackground = Resources.Load<Sprite>("Art/Guild/demo-bg-0");
            if (guildBackground == null)
            {
                Debug.LogWarning("Guild background sprite was not found at Resources/Art/Guild/demo-bg-0.");
                return;
            }

            StyleBackground background = new StyleBackground(guildBackground);
            documentRoot.Q<VisualElement>("guild-background").style.backgroundImage = background;
            pageBackground.style.backgroundImage = background;
        }

        private void ApplyInlineLayout()
        {
            root.style.flexGrow = 1f;
            root.style.width = Length.Percent(100f);
            root.style.height = Length.Percent(100f);

            VisualElement guildBackground = documentRoot.Q<VisualElement>("guild-background");
            SetAbsoluteFill(guildBackground);
            guildBackground.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            VisualElement guildShade = documentRoot.Q<VisualElement>("guild-shade");
            SetAbsoluteFill(guildShade);
            guildShade.style.backgroundColor = new Color(0.02f, 0.02f, 0.03f, 0.10f);

            SetAbsoluteFill(hubLayer);
            SetAbsoluteFill(pageLayer);
            SetAbsoluteFill(pageBackground);
            SetAbsoluteFill(pageDim);
            pageBackground.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            pageDim.style.backgroundColor = new Color(0.02f, 0.02f, 0.03f, 0.42f);

            VisualElement topBar = documentRoot.Q<VisualElement>("top-bar");
            topBar.style.position = Position.Absolute;
            topBar.style.top = 20f;
            topBar.style.right = 24f;
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.alignItems = Align.Center;

            StyleTopButton(documentRoot.Q<Button>("language-button"));
            StyleTopButton(documentRoot.Q<Button>("debug-button"));
            StyleTopButton(documentRoot.Q<Button>("page-language-button"));
            StyleTopButton(documentRoot.Q<Button>("back-button"));
            StyleTopButton(documentRoot.Q<Button>("placeholder-back-button"));
            StyleTopButton(documentRoot.Q<Button>("start-quest-button"));
            StyleTopButton(documentRoot.Q<Button>("reset-button"));
            StyleTopButton(sendReplyButton);

            skipToggle.style.height = 42f;
            skipToggle.style.marginLeft = 8f;
            skipToggle.style.paddingLeft = 12f;
            skipToggle.style.paddingRight = 12f;
            skipToggle.style.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 0.70f);
            skipToggle.style.color = new Color(0.96f, 0.91f, 0.80f, 1f);

            VisualElement hotspotLayer = documentRoot.Q<VisualElement>("hotspot-layer");
            SetAbsoluteFill(hotspotLayer);
            hotspots.Clear();
            ConfigureImageHotspot("counter-hotspot", "Art/Guild/Hotspots/counter-zone");
            ConfigureImageHotspot("board-hotspot", "Art/Guild/Hotspots/board-icon");
            ConfigureImageHotspot("table-hotspot", "Art/Guild/Hotspots/table-zone");
            ConfigureImageHotspot("shop-hotspot", "Art/Guild/Hotspots/equipment-zone");
        }

        private static void SetAbsoluteFill(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;
        }

        private static void StyleTopButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.style.height = 42f;
            button.style.marginLeft = 8f;
            button.style.paddingLeft = 16f;
            button.style.paddingRight = 16f;
            button.style.backgroundColor = new Color(0.12f, 0.13f, 0.16f, 0.86f);
            button.style.color = new Color(1f, 0.94f, 0.82f, 1f);
            button.style.borderLeftColor = new Color(1f, 0.86f, 0.58f, 0.42f);
            button.style.borderRightColor = new Color(1f, 0.86f, 0.58f, 0.42f);
            button.style.borderTopColor = new Color(1f, 0.86f, 0.58f, 0.42f);
            button.style.borderBottomColor = new Color(1f, 0.86f, 0.58f, 0.42f);
            button.style.borderLeftWidth = 1f;
            button.style.borderRightWidth = 1f;
            button.style.borderTopWidth = 1f;
            button.style.borderBottomWidth = 1f;
        }

        private void ConfigureImageHotspot(string name, string spritePath)
        {
            VisualElement hotspot = documentRoot.Q<VisualElement>(name);
            hotspot.style.position = Position.Absolute;
            hotspot.style.left = 0f;
            hotspot.style.top = 0f;
            hotspot.style.right = 0f;
            hotspot.style.bottom = 0f;
            hotspot.style.width = Length.Percent(100f);
            hotspot.style.height = Length.Percent(100f);
            hotspot.style.backgroundColor = Color.clear;
            hotspot.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            hotspot.pickingMode = PickingMode.Ignore;

            Sprite sprite = Resources.Load<Sprite>(spritePath);
            Texture2D texture = sprite != null ? sprite.texture : null;
            if (sprite == null || texture == null)
            {
                Debug.LogWarning("Guild hotspot sprite was not found or is not readable at Resources/" + spritePath + ".");
            }
            else
            {
                VisualElement highlight = CreateHotspotImageLayer(name + "-highlight", sprite);
                VisualElement outline = CreateHotspotImageLayer(name + "-outline", CreateOutlineSprite(name, texture));
                highlight.style.opacity = 0f;
                outline.style.opacity = 0f;
                hotspot.Insert(0, highlight);
                hotspot.Insert(1, outline);
            }

            Label label = hotspot.Q<Label>();
            if (label != null)
            {
                label.style.position = Position.Absolute;
                label.style.left = 0f;
                label.style.right = 0f;
                label.style.top = Length.Percent(44f);
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.color = new Color(1f, 0.92f, 0.72f, 1f);
                label.style.backgroundColor = new Color(0.04f, 0.035f, 0.03f, 0.74f);
                label.style.paddingTop = 8f;
                label.style.paddingBottom = 8f;
                label.style.display = DisplayStyle.None;
            }

            VisualElement highlightLayer = hotspot.Q<VisualElement>(name + "-highlight");
            VisualElement outlineLayer = hotspot.Q<VisualElement>(name + "-outline");
            hotspots.Add(new GuildHotspot(name, hotspot, highlightLayer, outlineLayer, texture));
        }

        private static VisualElement CreateHotspotImageLayer(string name, Sprite sprite)
        {
            VisualElement layer = new VisualElement { name = name };
            SetAbsoluteFill(layer);
            layer.pickingMode = PickingMode.Ignore;
            layer.style.backgroundColor = Color.clear;
            layer.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            if (sprite != null)
            {
                layer.style.backgroundImage = new StyleBackground(sprite);
            }

            return layer;
        }

        private void RegisterHotspot(string name, System.Action onClick)
        {
            VisualElement hotspot = documentRoot.Q<VisualElement>(name);
            hotspot.pickingMode = PickingMode.Ignore;
            hotspot.userData = onClick;
        }

        private void Update()
        {
            if (hubLayer == null || hubLayer.resolvedStyle.display == DisplayStyle.None || transitionPlayer != null)
            {
                return;
            }

            Vector2 pointerPosition = GetPointerPanelPosition();
            GuildHotspot hovered = FindHotspot(pointerPosition);

            for (int i = 0; i < hotspots.Count; i++)
            {
                SetHotspotHovered(hotspots[i], hotspots[i] == hovered);
            }

            if (hovered != null && Input.GetMouseButtonDown(0) && hovered.Element.userData is System.Action onClick)
            {
                onClick();
            }
        }

        private Vector2 GetPointerPanelPosition()
        {
            Vector2 screenPosition = Input.mousePosition;
            if (documentRoot.panel == null)
            {
                return new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            }

            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(documentRoot.panel, screenPosition);
            Rect rootRect = documentRoot.worldBound;
            if (rootRect.height > 0f)
            {
                panelPosition.y = rootRect.yMin + rootRect.yMax - panelPosition.y;
            }

            return panelPosition;
        }

        private GuildHotspot FindHotspot(Vector2 panelPosition)
        {
            for (int i = hotspots.Count - 1; i >= 0; i--)
            {
                GuildHotspot hotspot = hotspots[i];
                if (IsInsideHotspotAlpha(hotspot, panelPosition))
                {
                    return hotspot;
                }
            }

            return null;
        }

        private static bool IsInsideHotspotAlpha(GuildHotspot hotspot, Vector2 panelPosition)
        {
            if (hotspot.Texture == null)
            {
                return false;
            }

            Rect rect = hotspot.Element.worldBound;
            if (!TryGetTexturePixel(rect, hotspot.Texture, panelPosition, out int x, out int y))
            {
                return false;
            }

            return hotspot.Texture.GetPixel(x, y).a >= HotspotAlphaThreshold;
        }

        private static bool TryGetTexturePixel(Rect elementRect, Texture2D texture, Vector2 panelPosition, out int x, out int y)
        {
            x = 0;
            y = 0;

            if (!elementRect.Contains(panelPosition) || elementRect.width <= 0f || elementRect.height <= 0f)
            {
                return false;
            }

            float elementAspect = elementRect.width / elementRect.height;
            float textureAspect = (float)texture.width / texture.height;
            float drawWidth;
            float drawHeight;
            float drawLeft;
            float drawTop;

            if (elementAspect > textureAspect)
            {
                drawWidth = elementRect.width;
                drawHeight = elementRect.width / textureAspect;
                drawLeft = elementRect.xMin;
                drawTop = elementRect.yMin + (elementRect.height - drawHeight) * 0.5f;
            }
            else
            {
                drawHeight = elementRect.height;
                drawWidth = elementRect.height * textureAspect;
                drawLeft = elementRect.xMin + (elementRect.width - drawWidth) * 0.5f;
                drawTop = elementRect.yMin;
            }

            float localX = panelPosition.x - drawLeft;
            float localY = panelPosition.y - drawTop;
            if (localX < 0f || localY < 0f || localX > drawWidth || localY > drawHeight)
            {
                return false;
            }

            x = Mathf.Clamp(Mathf.FloorToInt(localX / drawWidth * texture.width), 0, texture.width - 1);
            y = Mathf.Clamp(Mathf.FloorToInt((1f - localY / drawHeight) * texture.height), 0, texture.height - 1);
            return true;
        }

        private void SetHotspotHovered(GuildHotspot hotspot, bool hovered)
        {
            if (hotspot.HighlightLayer != null)
            {
                hotspot.HighlightLayer.style.opacity = hovered ? 0.78f : debugHotspots ? 0.28f : 0f;
            }

            if (hotspot.OutlineLayer != null)
            {
                hotspot.OutlineLayer.style.opacity = hovered ? 1f : debugHotspots ? 0.65f : 0f;
            }

            Label label = hotspot.Element.Q<Label>();
            if (label != null)
            {
                label.style.display = debugHotspots ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static Sprite CreateOutlineSprite(string name, Texture2D source)
        {
            try
            {
                Color32[] sourcePixels = source.GetPixels32();
                Color32[] outlinePixels = new Color32[sourcePixels.Length];
                int width = source.width;
                int height = source.height;
                byte alphaThreshold = (byte)Mathf.Clamp(Mathf.RoundToInt(HotspotAlphaThreshold * 255f), 0, 255);
                Color32 outlineColor = new Color32(255, 178, 56, 255);
                int radiusSquared = HotspotOutlineRadius * HotspotOutlineRadius;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (sourcePixels[y * width + x].a < alphaThreshold)
                        {
                            continue;
                        }

                        for (int offsetY = -HotspotOutlineRadius; offsetY <= HotspotOutlineRadius; offsetY++)
                        {
                            int targetY = y + offsetY;
                            if (targetY < 0 || targetY >= height)
                            {
                                continue;
                            }

                            for (int offsetX = -HotspotOutlineRadius; offsetX <= HotspotOutlineRadius; offsetX++)
                            {
                                if (offsetX * offsetX + offsetY * offsetY > radiusSquared)
                                {
                                    continue;
                                }

                                int targetX = x + offsetX;
                                if (targetX < 0 || targetX >= width)
                                {
                                    continue;
                                }

                                int targetIndex = targetY * width + targetX;
                                if (sourcePixels[targetIndex].a >= alphaThreshold)
                                {
                                    continue;
                                }

                                outlinePixels[targetIndex] = outlineColor;
                            }
                        }
                    }
                }

                Texture2D outlineTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = name + " Outline Texture",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                outlineTexture.SetPixels32(outlinePixels);
                outlineTexture.Apply(false, true);
                return Sprite.Create(outlineTexture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not create guild hotspot outline for " + name + ": " + ex.Message);
                return null;
            }
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
            if (preparingCounterTransition || (transitionPlayer != null && transitionPlayer.isPlaying))
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
            transitionPlayer.playOnAwake = false;
            transitionPlayer.waitForFirstFrame = true;
            transitionPlayer.prepareCompleted += OnCounterTransitionPrepared;
            transitionPlayer.loopPointReached += OnCounterTransitionComplete;
            preparingCounterTransition = true;
            transitionPlayer.Prepare();
        }

        private void OnCounterTransitionPrepared(VideoPlayer player)
        {
            preparingCounterTransition = false;
            SetDisplay(documentRoot, false);
            player.Play();
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
            foreach (GuildHotspot hotspot in hotspots)
            {
                SetHotspotHovered(hotspot, false);
            }
        }

        private sealed class GuildHotspot
        {
            public GuildHotspot(string name, VisualElement element, VisualElement highlightLayer, VisualElement outlineLayer, Texture2D texture)
            {
                Name = name;
                Element = element;
                HighlightLayer = highlightLayer;
                OutlineLayer = outlineLayer;
                Texture = texture;
            }

            public string Name { get; }
            public VisualElement Element { get; }
            public VisualElement HighlightLayer { get; }
            public VisualElement OutlineLayer { get; }
            public Texture2D Texture { get; }
        }

        private void StopCounterVideo()
        {
            if (transitionPlayer == null)
            {
                return;
            }

            preparingCounterTransition = false;
            transitionPlayer.loopPointReached -= OnCounterTransitionComplete;
            transitionPlayer.prepareCompleted -= OnCounterTransitionPrepared;
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
