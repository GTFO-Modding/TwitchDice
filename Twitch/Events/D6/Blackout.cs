using AK;
using System.Collections;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D6
{
    public class Blackout : DiceEvent<bo>
    {
        public override string EventName => "Power Malfunction";
        public override string EventDescription => "All lights temporarily get turned off.";
        public override string EventID => "blackout";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) all lights get turned off", 20);
            return cfg;
        }

        public override bool CanBeTriggered()
        {
            // todo: re-implement check
            return true;
        }

        public override void ReceiveClient(ulong sender, bo packet)
        {
            TriggerCommon();
        }

        public override void TriggerHost()
        {
            TimedEvents.StartTimedEvent(this.TriggerBlackout(), this);
            TriggerCommon();
            this.TriggerClient();
        }

        private static void TriggerCommon()
        {
            PlayerUtil.LocalPlayerAgent.Sound.Post(EVENTS.LIGHTS_OFF_GLOBAL);
        }

        private IEnumerator TriggerBlackout()
        {
            EnvironmentStateManager.AttemptSetExpeditionLightMode(false);
            yield return new WaitForSeconds(this.Time);
            EnvironmentStateManager.AttemptSetExpeditionLightMode(true);
            yield break;
        }
    }

    public struct bo
    { }
}
