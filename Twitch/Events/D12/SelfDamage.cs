using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class SelfDamage : DiceEventWithConfig<SE, SelfDamage.RundownConfig>
    {
        public override string EventName => "Enemy Thorns";
        public override string EventDescription => "All damage done to enemies is inflicted on the player.";
        public override string EventID => "selfDamage";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override DiceEventConfig<RundownConfig> GetConfig()
        {
            DiceEventConfig<RundownConfig> cfg = base.GetConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) that self damage is enabled", 30);
            return cfg;
        }

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public float SelfDamageMultiplier { get; set; } = 0.5f;
        }

        public override void ReceiveClient(ulong sender, SE packet)
        {
            PlayerControlManager.EnableSelfDamageForSeconds(this.Time, packet.multiplier);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            float multiplier = this.RundownCfg.SelfDamageMultiplier;
            PlayerControlManager.EnableSelfDamageForSeconds(this.Time, multiplier);
            this.TriggerClient(new SE() { multiplier = multiplier });
            this.StartEventTimer();
        }
    }

    public struct SE
    {
        public float multiplier;
    }
}
