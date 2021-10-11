

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class DisableAllInteractions : DiceEvent<DAI>
    {
        public override string EventName => "E Machine Broke";

        public override string EventID => "disableinteract";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override void ReceiveClient(ulong sender, DAI packet)
        {
            PlayerControlManager.DisableInteractionsForSeconds(30f);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.DisableInteractionsForSeconds(30f);
        }
    }

    public struct DAI
    { }
}
