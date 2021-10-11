

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class AllAttacksInstaKill : DiceEvent<AAIK>
    {
        public override string EventName => "Instant Kill";

        public override string EventID => "instantkill";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => 20;

        public override void ReceiveClient(ulong sender, AAIK packet)
        {
            PlayerControlManager.EnableInstantKillForSeconds(this.Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.EnableInstantKillForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct AAIK
    { }
}
