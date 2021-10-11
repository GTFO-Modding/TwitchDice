

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

        private int _time;
        public override int Time => _time;

        public override void ReceiveClient(ulong sender, NM packet)
        {
            _time = (int)packet.seconds;
            StartEventTimer();
            PlayerControlManager.DisableMovementForSeconds(packet.seconds);
        }

        private static float GetRandomActivationTime()
        {
            return (float)(Math.Floor(Main.rnd.NextDouble() * 10) + 10);
        }

        public override void TriggerHost()
        {
            float time = GetRandomActivationTime();
            _time = (int)time;
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    StartEventTimer();
                    PlayerControlManager.DisableMovementForSeconds(time);
                }
                else
                {
                    TriggerClient(new NM(time), player.Owner);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NM
    {
        public float seconds;

        public NM(float seconds)
        {
            this.seconds = seconds;
        }
    }
}
