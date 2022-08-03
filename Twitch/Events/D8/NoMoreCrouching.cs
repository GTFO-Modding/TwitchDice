using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreCrouching : DiceEvent<NMC>
    {
        public override string EventName => "No Crouch";
        public override string EventDescription => "Disables crouching for all players.";
        public override string EventID => "disableCrouch";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to disable crouching", 20);
            return cfg;
        }

        public override bool CanBeTriggered()
        {
            return !PlayerControlManager.ForceCrouchEnabled;
        }

        public override void ReceiveClient(ulong sender, NMC packet)
        {
            PlayerControlManager.DisableCrouchingForSeconds(this.Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();


            PlayerControlManager.DisableCrouchingForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct NMC
    { }
}
