

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D20
{
    public class GlobalPlayerDamage : DiceEvent<GPD>
    {
        public override string EventName => "Global Damage";

        public override string EventID => "globdam";

        protected override DiceTier DiceTier => DiceTier.D20;

        public override void ReceiveClient(ulong sender, GPD packet)
        {
            PlayerControlManager.EnableGlobalPlayerDamage(30f);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();

            PlayerControlManager.EnableGlobalPlayerDamage(30f);
        }
    }

    public struct GPD
    {
    }
}
