
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class InvertControls : DiceEvent<IC>
    {
        public override string EventName => "Inverted";

        public override string EventID => "invertctrls";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override void ReceiveClient(ulong sender, IC packet)
        {
            PlayerControlManager.InvertControlsForSecondsEvent(10f, this.EventName);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();

            PlayerControlManager.InvertControlsForSecondsEvent(10f, this.EventName);
        }
    }

    public struct IC
    { }
}
