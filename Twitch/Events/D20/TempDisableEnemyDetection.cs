using Globals;
using System;
using System.Collections;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D20
{
    public class TempDisableEnemyDetection : DiceEvent
    {
        public override string EventName => "Silent and Sneaky";
        public override string EventDescription => "Temporarily disabled enemy detection.";
        public override string EventID => "nodetect";

        protected override DiceTier DiceTier => DiceTier.D20;

        private int _time = 0;
        public override int Time => base.Time;

        private static float GetRandomActivationTime() // 15sec - 30sec
        {
            return (float)(Math.Floor(Main.rnd.NextDouble() * 15) + 15);
        }

        private IEnumerator DoTriggerEvent(float seconds)
        {
            Global.EnemyPlayerDetectionEnabled = false;
            yield return new WaitForSeconds(seconds);
            Global.EnemyPlayerDetectionEnabled = true;
        }

        public override void TriggerHost()
        {
            float time = GetRandomActivationTime();
            _time = (int)time;
            TimedEvents.StartTimedEvent(this.DoTriggerEvent(Time), this);
        }
    }
}
