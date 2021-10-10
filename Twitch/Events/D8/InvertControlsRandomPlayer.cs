

using Player;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class InvertControlsRandomPlayer : DiceEvent<ICR>
    {
        public override string EventName => "Random Inverted";

        public override string EventID => "invertctrlsRan";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override void ReceiveClient(ulong sender, ICR packet)
        {
            if (packet.PlayerID == PlayerUtil.LocalPlayerAgent.Owner.Lookup)
            {
                PlayerControlManager.InvertControlsForSeconds(30f);
            }
        }

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    PlayerControlManager.InvertControlsForSeconds(30f);
                }
                else
                {
                    this.TriggerClient(new ICR(player));
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICR
    {
        public ulong PlayerID;

        public ICR(PlayerAgent player)
        {
            this.PlayerID = player.Owner.Lookup;
        }
    }
}
