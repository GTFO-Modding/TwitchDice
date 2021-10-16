using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreFire : DiceEvent<NMF>
    {
        public override string EventName => "Finger Cramp";

        public override string EventID => "disableFire";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => 20;
        
        public override void ReceiveClient(ulong sender, NMF packet)
        {
            this.TriggerCommon();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            this.TriggerCommon();
        }

        private void TriggerCommon()
        {
            PlayerControlManager.DisableFireForSeconds(Time);
            this.StartEventTimer();
        }
    }

    public struct NMF
    { }
}



