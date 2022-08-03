using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class PlayerPain : DiceEvent<PP>
    {
        public override string EventName => "Player Pain";
        public override string EventDescription => "Damages a random player by an amount.";
        public override string EventID => "pain";

        protected override DiceTier DiceTier => DiceTier.D6;

        public float DamagePercent => this.Config.ClientConfig.GetValue<float>(nameof(this.DamagePercent));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.DamagePercent), "The percent health to damage the players by", 0.1f);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, PP packet)
        {
            if (packet.PlayerID == PlayerUtil.LocalPlayerAgent.Owner.Lookup)
            {
                TriggerCommon();
            }
        }

        private static void TriggerCommon()
        {
            PlayerAgent player = PlayerUtil.LocalPlayerAgent;
            player.Damage.NoAirDamage(player.Damage.Health * 0.1f);
        }

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    TriggerCommon();
                }
                else
                {
                    this.TriggerClient(new PP(player));
                }
            }
        }
    }

    public struct PP
    {
        public ulong PlayerID;

        public PP(PlayerAgent player)
        {
            this.PlayerID = player.Owner.Lookup;
        }
    }
}
