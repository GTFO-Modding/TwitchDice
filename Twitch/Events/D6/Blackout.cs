using AK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D6
{
    public class Blackout : DiceEvent<bo>
    {
        public override string EventName => "Power Malfunction";

        public override string EventID => "blackout";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => 20;

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
            TimedEvents.StartTimedEvent(TriggerBlackout(), this);
            TriggerCommon();
            TriggerClient();
        }

        private void TriggerCommon()
        {
            PlayerUtil.LocalPlayerAgent.Sound.Post(EVENTS.LIGHTS_OFF_GLOBAL);
        }

        private IEnumerator TriggerBlackout()
        {
            EnvironmentStateManager.AttemptSetExpeditionLightMode(false);
            yield return new WaitForSeconds(Time);
            EnvironmentStateManager.AttemptSetExpeditionLightMode(true);
            yield break;
        }
    }

    public struct bo
    {

    }
}
