using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class DisableAllInteractions : DiceEvent<DAI>
    {
        public override string EventName => "E Machine Broke";
        public override string EventDescription => "Disables all interactions for a set time.";
        public override string EventID => "disableinteract";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time that interactions are disabled", 30);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, DAI packet)
        {
            PlayerControlManager.DisableInteractionsForSeconds(this.Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.DisableInteractionsForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct DAI
    { }
}
