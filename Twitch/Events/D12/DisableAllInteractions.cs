

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
            PlayerControlManager.DisableInteractionsForSecondsEvent(30f, this.EventName);
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            PlayerControlManager.DisableInteractionsForSecondsEvent(30f, this.EventName);
        }
    }

    public struct DAI
    { }
}
