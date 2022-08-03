using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class ForceCrouch : DiceEvent<FC>
    {
        public override string EventName => "Tired Legs";
        public override string EventDescription => "Forces all players to crouch";
        public override string EventID => "forcecrouch";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to force all players to crouch", 10);
            return cfg;
        }

        public override bool CanBeTriggered()
        {
            return !PlayerControlManager.DisableCrouching;
        }

        public override void ReceiveClient(ulong sender, FC packet)
        {
            PlayerControlManager.ForceCrouchForSeconds(this.Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.ForceCrouchForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct FC
    { }
}
