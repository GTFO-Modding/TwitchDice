using Player;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class InvertControlsRandomPlayer : DiceEvent<ICR>
    {
        public override string EventName => "Random Inverted";
        public override string EventDescription => "Inverts the controls of a random player.";
        public override string EventID => "invertctrlsRan";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to invert a player's controls.", 30);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, ICR packet)
        {
            if (packet.PlayerID == PlayerUtil.LocalPlayerAgent.Owner.Lookup)
            {
                PlayerControlManager.InvertControlsForSeconds(this.Time);
                this.StartEventTimer();
            }
        }

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    PlayerControlManager.InvertControlsForSeconds(this.Time);
                    this.StartEventTimer();
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
