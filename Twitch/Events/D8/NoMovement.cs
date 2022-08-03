

using Player;
using System;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMovement : DiceEvent<NM>
    {
        public override string EventName => "Leg Glue";
        public override string EventDescription => "Disables a random player from moving for 10-20 seconds.";
        public override string EventID => "nomove";

        protected override DiceTier DiceTier => DiceTier.D8;

        private int _time;
        public override int Time => _time;

        public override void ReceiveClient(ulong sender, NM packet)
        {
            this._time = (int)packet.seconds;
            this.StartEventTimer();
            PlayerControlManager.DisableMovementForSeconds(packet.seconds);
        }

        private static float GetRandomActivationTime()
        {
            return (float)(Math.Floor(Main.rnd.NextDouble() * 10) + 10);
        }

        public override void TriggerHost()
        {
            float time = GetRandomActivationTime();
            this._time = (int)time;
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    this.StartEventTimer();
                    PlayerControlManager.DisableMovementForSeconds(time);
                }
                else
                {
                    this.TriggerClient(new NM(time), player.Owner);
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
