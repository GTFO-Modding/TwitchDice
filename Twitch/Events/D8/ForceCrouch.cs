
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class ForceCrouch : DiceEvent<FC>
    {
        public override string EventName => "Tired Legs";

        public override string EventID => "forcecrouch";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override void ReceiveClient(ulong sender, FC packet)
        {
            PlayerControlManager.ForceCrouchForSecondsEvent(10f, this.EventName);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.ForceCrouchForSecondsEvent(10f, this.EventName);
        }
    }

    public struct FC
    { }
}
