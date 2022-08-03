using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreSprint : DiceEvent<NMS>
    {
        public override string EventName => "Take a Break";
        public override string EventDescription => "Disables sprinting for all players for a set number of time.";
        public override string EventID => "noSprint";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to disable crouching", 30);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, NMS packet)
        {
            PlayerControlManager.DisableRunningForSeconds(this.Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.DisableRunningForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct NMS
    { }
}
