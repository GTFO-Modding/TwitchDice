using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D4
{
    public class Player180 : DiceEvent<P80>
    {
        public override string EventName => "go back";
        public override string EventDescription => "Makes all players turn around.";
        public override string EventID => "180";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override void ReceiveClient(ulong sender, P80 packet)
        {
            PlayerUtil.LocalPlayerAgent.FPSCamera.m_yaw += 180;
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
            if (target.IsLocallyOwned)
            {
                target.FPSCamera.m_yaw += 180;
            }
            else
            {
                this.TriggerClient(new P80(), target.Owner);
            }
        }
    }

    public struct P80
    {

    }
}
