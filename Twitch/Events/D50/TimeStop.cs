using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class TimeStop : DiceEvent<TS>
    {
        public override string EventName => "ZA WARUDO!!";

        public override string EventID => "timeStop";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override int Time => 5;

        public override void ReceiveClient(ulong sender, TS packet)
        {
            TriggerCommon();
        }

        public override void TriggerHost()
        {
            TriggerCommon();
            TriggerClient();
        }

        private void TriggerCommon()
        {
            TimedEvents.Start(StopTime(), this);
        }

        private IEnumerator StopTime()
        {
            var previousTime = UnityEngine.Time.timeScale;
            UnityEngine.Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(Time);

            UnityEngine.Time.timeScale = previousTime;

            yield break;
        }
    }

    public struct TS
    {

    }
}
