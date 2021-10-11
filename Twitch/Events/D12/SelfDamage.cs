

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class SelfDamage : DiceEvent<SE>
    {
        public override string EventName => "Enemy Thorns";

        public override string EventID => "selfDamage";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override int Time => 30;

        public override void ReceiveClient(ulong sender, SE packet)
        {
            PlayerControlManager.EnableSelfDamageForSeconds(this.Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.EnableSelfDamageForSeconds(this.Time);
            this.StartEventTimer();
        }
    }

    public struct SE
    { }
}
