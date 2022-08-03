using System.Collections;
using TwitchDice.CustomSounds.TAK;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class TimeStop : DiceEvent<TS>
    {
        public override string EventName => "ZA WARUDO!!";
        public override string EventDescription => "Forces time to stop.";
        public override string EventID => "timeStop";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override int Time => 15;

        public override void ReceiveClient(ulong sender, TS packet)
        {
            this.TriggerCommon();
        }

        public override void TriggerHost()
        {
            this.TriggerCommon();
            this.TriggerClient();
        }

        private void TriggerCommon()
        {
            TimedEvents.Start(this.StopTime(), this);
        }

        private IEnumerator StopTime()
        {
            CellSound.Post(TEVENTS.PLAY_TIMESTOP);

            var previousTime = UnityEngine.Time.timeScale;

            yield return new WaitForSecondsRealtime(0.5f);
            UnityEngine.Time.timeScale = previousTime * 0.8f;

            yield return new WaitForSecondsRealtime(0.5f);
            UnityEngine.Time.timeScale = previousTime * 0.6f;

            yield return new WaitForSecondsRealtime(0.5f);
            UnityEngine.Time.timeScale = previousTime * 0.4f;

            yield return new WaitForSecondsRealtime(0.5f);
            UnityEngine.Time.timeScale = previousTime * 0.2f;

            yield return new WaitForSecondsRealtime(0.5f);
            UnityEngine.Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(Time - 3.0f);

            UnityEngine.Time.timeScale = previousTime;

            yield break;
        }
    }

    public struct TS
    {

    }
}
