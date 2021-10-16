using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class NoMoreFire : DiceEvent<NMF>
    {
        public override string EventName => "No Guns";

        public override string EventID => "disableFire";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override int Time => 20;
        
        public override void ReceiveClient(ulong sender, NMF packet)
        {
            PlayerControlManager.DisableFireForSeconds(Time);
            this.StartEventTimer();
        }

        public override void TriggerHost()
        {
            this.TriggerClient();


            PlayerControlManager.DisableFireForSeconds(Time);
            this.StartEventTimer();
        }
    }

    public struct NMF
    { }
}



