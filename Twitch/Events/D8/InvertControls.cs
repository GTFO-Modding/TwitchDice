using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class InvertControls : DiceEvent<IC>
    {
        public override string EventName => "Inverted";
        public override string EventDescription => "Inverts all player's controls. (WASD = SDWA, mouse inverted)";
        public override string EventID => "invertctrls";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to invert all player's controls.", 10);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, IC packet)
        {
            PlayerControlManager.InvertControlsForSeconds(packet.time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.InvertControlsForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct IC
    {
        public float time;
    }
}
