

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreCrouching : DiceEvent<NMC>
    {
        public override string EventName => "No Crouch";

        public override string EventID => "disableCrouch";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => 20;

        public override bool CanBeTriggered()
        {
            return !PlayerControlManager.ForceCrouchEnabled;
        }

        public override void ReceiveClient(ulong sender, NMC packet)
        {
            PlayerControlManager.DisableCrouchingForSeconds(Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();


            PlayerControlManager.DisableCrouchingForSeconds(Time);
            this.StartEventTimer();
        }
    }

    public struct NMC
    { }
}
