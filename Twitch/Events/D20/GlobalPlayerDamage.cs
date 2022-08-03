

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D20
{
    public class GlobalPlayerDamage : DiceEvent<GPD>
    {
        public override string EventName => "Global Damage";
        public override string EventDescription => "Enables global player damage.";
        public override string EventID => "globdam";

        protected override DiceTier DiceTier => DiceTier.D20;
        protected override bool ForceDisable => true;

        public override int Time => 30;

        public override void ReceiveClient(ulong sender, GPD packet)
        {
            StartEventTimer();
            PlayerControlManager.EnableGlobalPlayerDamageForSeconds(Time);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            StartEventTimer();
            PlayerControlManager.EnableGlobalPlayerDamageForSeconds(Time);
        }
    }

    public struct GPD
    {
    }
}
