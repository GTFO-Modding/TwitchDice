
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
            CrouchingManager.ForceCrouchForSeconds(10f);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            CrouchingManager.ForceCrouchForSeconds(10f);
        }
    }

    public struct FC
    { }
}
