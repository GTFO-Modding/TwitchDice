using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class AllAttacksInstaKill : DiceEvent<AAIK>
    {
        public override string EventName => "Instant Kill";
        public override string EventDescription => "All attacks instantly kill enemies.";
        public override string EventID => "instantkill";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) that {EventName} is active.", 20);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, AAIK packet)
        {
            PlayerControlManager.EnableInstantKillForSeconds(packet.time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient(new AAIK() { time = this.Time });
            PlayerControlManager.EnableInstantKillForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct AAIK
    {
        public float time;
    }
}
