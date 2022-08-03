using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreFire : DiceEvent<NMF>
    {
        public override string EventName => "Finger Cramp";
        public override string EventDescription => "Disables firing of all player's gun";
        public override string EventID => "disableFire";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to disable firing of players' guns", 20);
            return cfg;
        }

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
            PlayerControlManager.DisableFireForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct NMF
    { }
}



