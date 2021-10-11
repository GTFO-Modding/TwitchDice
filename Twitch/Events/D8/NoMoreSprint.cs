

using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreSprint : DiceEvent<NMS>
    {
        public override string EventName => "Take a Break";

        public override string EventID => "noSprint";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => 30;

        public override void ReceiveClient(ulong sender, NMS packet)
        {
            PlayerControlManager.DisableRunningForSeconds(Time);
            StartEventTimer();
        }

        public override void TriggerHost()
        {
            TriggerClient();
            PlayerControlManager.DisableRunningForSeconds(Time);
            StartEventTimer();
        }
    }

    public struct NMS
    { }
}
