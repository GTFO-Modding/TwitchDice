
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class ForceCrouch : DiceEvent<FC>
    {
        public override string EventName => "Tired Legs";

        public override string EventID => "forcecrouch";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => 10;

        public override bool CanBeTriggered()
        {
            return !PlayerControlManager.DisableCrouching;
        }

        public override void ReceiveClient(ulong sender, FC packet)
        {
            PlayerControlManager.ForceCrouchForSeconds(Time);
            StartEventTimer();
        }

        public override void TriggerHost()
        {
            TriggerClient();
            PlayerControlManager.ForceCrouchForSeconds(Time);
            StartEventTimer();
        }
    }

    public struct FC
    { }
}
