

using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class PlayerPain : DiceEvent<PP>
    {
        public override string EventName => "Player Pain";

        public override string EventID => "pain";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override void ReceiveClient(ulong sender, PP packet)
        {
            if (packet.PlayerID == PlayerUtil.LocalPlayerAgent.Owner.Lookup)
            {
                this.TriggerCommon();
            }
        }

        private void TriggerCommon()
        {
            var player = PlayerUtil.LocalPlayerAgent;
            player.Damage.NoAirDamage(player.Damage.Health * 0.1f);
        }

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    this.TriggerCommon();
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
