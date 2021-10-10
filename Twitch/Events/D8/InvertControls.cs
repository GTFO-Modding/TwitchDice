
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
            PlayerControlManager.ForceInvertControlsForSeconds(10f);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();

            PlayerControlManager.ForceInvertControlsForSeconds(10f);
        }
    }

    public struct IC
    { }
}
