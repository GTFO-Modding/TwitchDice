

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

        public override string EventID => "nodetect";

        protected override DiceTier DiceTier => DiceTier.D20;

        private static float GetRandomActivationTime() // 15sec - 1min
        {
            return (float)(Math.Floor(Main.rnd.NextDouble() * 45) + 15);
        }

        private IEnumerator DoTriggerEvent(float seconds)
        {
            Global.EnemyPlayerDetectionEnabled = false;
            yield return new WaitForSeconds(seconds);
            Global.EnemyPlayerDetectionEnabled = true;
        }

        public override void TriggerHost()
        {
            TimedEvents.Start(this.DoTriggerEvent(GetRandomActivationTime()));
        }
    }
}
