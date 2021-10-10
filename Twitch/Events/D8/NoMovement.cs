

using Player;
using System;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMovement : DiceEvent<NM>
    {
        public override string EventName => "Leg Glue";

        public override string EventID => "nomove";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override void ReceiveClient(ulong sender, NM packet)
        {
            if (packet.PlayerID == PlayerUtil.LocalPlayerAgent.GlobalID)
            {
                PlayerControlManager.DisableMovementForSeconds(packet.seconds);
            }
        }

        private static float GetRandomActivationTime()
        {
            return (float)(Math.Floor(Main.rnd.NextDouble() * 10) + 10);
        }

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    PlayerControlManager.DisableMovementForSeconds(GetRandomActivationTime());
                }
                else
                {
                    this.TriggerClient(new NM(player, GetRandomActivationTime()));
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NM
    {
        public float seconds;
        public ushort PlayerID;

        public NM(PlayerAgent player, float seconds)
        {
            this.seconds = seconds;
            this.PlayerID = player.GlobalID;
        }
    }
}
