using System.Text;
using TwitchDice.Extensions;
using TwitchDice.Twitch;
using UnityEngine;

namespace TwitchDice.Components
{
    public partial class DiceActivationAnimator
    {
        private sealed class QueuedDiceEvent
        {
            public readonly float displayTime;
            public readonly IDiceEvent @event;
            public readonly string activatingPlayer;
            public readonly DiceActivationAnimator animator;
            public readonly bool showDescription;

            public DiceTier Tier => this.@event.Tier;
            public int Time => this.@event.Time;
            public string EventName => this.@event.EventName;
            public string EventDescription => this.@event.EventDescription;

            public QueuedDiceEvent(DiceActivationAnimator animator, float displayTime, IDiceEvent @event, string activatingPlayer, bool showDescription = false)
            {
                this.showDescription = showDescription;
                this.animator = animator;
                this.displayTime = displayTime;
                this.@event = @event;
                this.activatingPlayer = activatingPlayer;
                this.m_currentTime = 0f;
                this.m_state = QueuedDiceEventState.None;
            }

            private float m_currentTime;
            private QueuedDiceEventState m_state;
            private bool m_activated;

            public bool Activated => this.m_activated;

            private float EndTime
            {
                get
                {
                    return this.m_state switch
                    {
                        QueuedDiceEventState.FadeIn => this.animator.m_fadeInTime,
                        QueuedDiceEventState.Visible => this.displayTime,
                        QueuedDiceEventState.FadeOut => this.animator.m_fadeOutTime,
                        _ => 0f,
                    };
                }
            }
            private float Progress
            {
                get
                {
                    float endTime = this.EndTime;
                    if (endTime == 0f)
                    {
                        return 1f;
                    }
                    return this.m_currentTime / endTime;
                }
            }

            private bool InFinishedState => this.m_state == QueuedDiceEventState.None || this.m_state == QueuedDiceEventState.Disposed;

            public void Activate()
            {
                this.m_state = QueuedDiceEventState.FadeIn;
                this.m_activated = true;
                this.animator.m_icon.sprite = this.animator.m_iconMap[this.@event.Tier];
                if (ColorUtility.TryParseHtmlString(Utilities.ColorUtil.GetDiceColorForTier(this.@event.Tier), out Color col))
                {
                    this.animator.m_icon.color = col;
                }

                StringBuilder messageBuilder = new();

                messageBuilder.Append("<b><color=#FA8>");
                messageBuilder.Append(this.activatingPlayer.EscapeTags());
                messageBuilder.Append("</color></b> triggered ");
                messageBuilder.Append(this.EventName);

                if (this.Time > 0)
                {
                    messageBuilder.Append(" for ");
                    messageBuilder.Append(this.Time);
                    messageBuilder.Append(" second");
                    if (this.Time != 1)
                    {
                        messageBuilder.Append('s');
                    }
                }
                messageBuilder.Append('!');

                if (this.showDescription)
                {
                    messageBuilder.AppendLine();
                    messageBuilder.Append("<size=75%>");
                    messageBuilder.Append(this.EventDescription);
                    messageBuilder.Append("</size>");
                }

                this.animator.SetText(messageBuilder.ToString());
                this.animator.PlaySound();
            }

            public bool Tick(float delta)
            {
                if (this.InFinishedState)
                {
                    return false;
                }

                this.m_currentTime += delta;

                if (this.m_state == QueuedDiceEventState.FadeIn)
                {
                    this.animator.SetVisibility(this.Progress);
                }
                else if (this.m_state == QueuedDiceEventState.FadeOut)
                {
                    this.animator.SetVisibility(1f - this.Progress);
                }

                if (this.m_currentTime >= this.EndTime)
                {
                    this.m_currentTime -= this.EndTime;
                    this.m_state++;
                }
                return !this.InFinishedState;
            }
        }

        private enum QueuedDiceEventState : byte
        {
            None,
            FadeIn,
            Visible,
            FadeOut,
            Disposed
        }
    }
}
