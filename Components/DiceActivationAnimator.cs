using System;
using System.Collections.Generic;
using TMPro;
using TwitchDice.CustomSounds.TAK;
using TwitchDice.Extensions;
using TwitchDice.Twitch;
using TwitchDice.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TwitchDice.Components
{
    public partial class DiceActivationAnimator : MonoBehaviour
    {
        private static DiceActivationAnimator? s_current;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DiceActivationAnimator(IntPtr ptr) : base(ptr)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        private Image m_icon;
        private TextMeshProUGUI m_text;
        private RectTransform m_panel;


        private float m_fadeInTime = 0.25f;
        private float m_fadeOutTime = 0.25f;

        private CellSoundPlayer m_source;

        private readonly Queue<QueuedDiceEvent> m_queuedEvents = new();
        private readonly Dictionary<DiceTier, Sprite?> m_iconMap = new();

        private void SetText(string text)
        {
            this.m_text.text = text;
        }

        private void PlaySound()
        {
            this.m_source.Post(TEVENTS.PLAY_DICE_ACTIVATE);
        }

        private void SetVisibility(float percent)
        {
            this.m_panel.gameObject.SetActive(percent >= Mathf.Epsilon);
            this.m_panel.localScale = new(percent, percent, percent);
        }

        private void Start()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.m_iconMap.Clear();
            foreach (string name in Enum.GetNames(typeof(DiceTier)))
            {
                DiceTier tier = (DiceTier)Enum.Parse(typeof(DiceTier), name);
                this.m_iconMap.Add(tier, IconManager.GetDiceIconForTier(tier));
            }
            this.m_source = new CellSoundPlayer();
        }

        private void DoQueueAnimation(IDiceEvent eventActivated, string activatedBy, float displayTime)
        {
            this.m_queuedEvents.Enqueue(new QueuedDiceEvent(this, displayTime, eventActivated, activatedBy));
        }

        public static void QueueAnimation(IDiceEvent eventActivated, string activatedBy, float displayTime = 5f)
        {
            if (eventActivated == null)
            {
                throw new ArgumentNullException(nameof(eventActivated));
            }
            if (activatedBy == null)
            {
                throw new ArgumentNullException(nameof(activatedBy));
            }
            if (displayTime <= Mathf.Epsilon)
            {
                throw new ArgumentOutOfRangeException(nameof(displayTime), "Time must be greater than 0.");
            }

            Current.DoQueueAnimation(eventActivated, activatedBy, displayTime);
        }

        private void Update()
        {
            if (this.m_queuedEvents.TryPeek(out QueuedDiceEvent? currentEvent))
            {
                float delta = Time.deltaTime;
                if (!currentEvent.Activated)
                {
                    currentEvent.Activate();
                }

                if (!currentEvent.Tick(delta))
                {
                    this.m_queuedEvents.Dequeue();
                }
            }
        }

        public static DiceActivationAnimator Current
        {
            get
            {
                if (s_current == null)
                {
                    GameObject rootGO = new("DiceActivationAnimator");
                    s_current = rootGO.AddComponent<DiceActivationAnimator>();
                    rootGO.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    GameObject uiCanvasGO = new("UI Canvas");
                    uiCanvasGO.transform.SetParent(rootGO.transform);

                    Canvas canvas = uiCanvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.pixelPerfect = false;
                    canvas.sortingOrder = 999;
                    canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;

                    CanvasScaler canvasScaler = uiCanvasGO.AddComponent<CanvasScaler>();
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    canvasScaler.scaleFactor = 1f;
                    canvasScaler.referencePixelsPerUnit = 100f;

                    GameObject activationPanelGO = new("Activation Panel");
                    activationPanelGO.transform.SetParent(uiCanvasGO.transform);

                    GameObject iconGO = new("Icon");
                    iconGO.transform.SetParent(activationPanelGO.transform);

                    Image icon = iconGO.AddComponent<Image>();
                    icon.raycastTarget = false;

                    GameObject infoTextGO = new("Info Text");
                    infoTextGO.transform.SetParent(activationPanelGO.transform);

                    TextMeshProUGUI text = infoTextGO.AddComponent<TextMeshProUGUI>();
                    text.text = "";
                    text.fontSize = 24f;

                    RectTransform panel = activationPanelGO.GetOrAddComponent<RectTransform>()
                        .SetAnchor(0.5f, 0.8f)
                        .SetAnchoredPosition(0f, 0f)
                        .SetPivot(0.5f, 0.5f)
                        .SetSizeDelta(350f, 250f);

                    iconGO.GetOrAddComponent<RectTransform>()
                        .SetAnchor(0.5f, 1f)
                        .SetAnchoredPosition(0f, 0f)
                        .SetPivot(0.5f, 1f)
                        .SetSizeDelta(94f, 107.25f);

                    infoTextGO.GetOrAddComponent<RectTransform>()
                        .SetAnchorMin(0f, 0f)
                        .SetAnchorMax(1f, 0f)
                        .SetAnchoredPosition(0f, 0f)
                        .SetPivot(0.5f, 0f)
                        .SetSizeDelta(0f, 125f);

                    activationPanelGO.SetActive(false);
                    s_current.m_icon = icon;
                    s_current.m_text = text;
                    s_current.m_panel = panel;

                    DontDestroyOnLoad(rootGO);
                }

                return s_current;
            }
        }
    }
}
