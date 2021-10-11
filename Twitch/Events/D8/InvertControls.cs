
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class InvertControls : DiceEvent<IC>
    {
        public override string EventName => "Inverted";

        public override string EventID => "invertctrls";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => 10;

        public override void ReceiveClient(ulong sender, IC packet)
        {
            PlayerControlManager.InvertControlsForSeconds(Time);
            StartEventTimer();
        }

        public override void TriggerHost()
        {
            TriggerClient();
            PlayerControlManager.InvertControlsForSeconds(Time);
            StartEventTimer();
        }
    }

    public struct IC
    { }
}
