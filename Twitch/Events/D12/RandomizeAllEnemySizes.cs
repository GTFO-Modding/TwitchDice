

using Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D12
{
    public class RandomizeAllEnemySizes : DiceEvent<RAES>
    {
        public override string EventName => "Randomize All Enemy Size";

        public override string EventID => "randomizeSizes";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override int Time => 60;

        public override void ReceiveClient(ulong sender, RAES packet)
        {
            this.TriggerCommon(packet.Seed);
        }

        private IEnumerator ApplyScale(EnemyAgent enemy, float seconds, float multiplier)
        {
            var oldScale = enemy.transform.localScale;
            enemy.transform.localScale = enemy.transform.localScale * multiplier;
            yield return new WaitForSeconds(seconds);
            if (enemy != null)
            {
                enemy.transform.localScale = oldScale;
            }
        }

        private void TriggerCommon(int seed)
        {
            var random = new System.Random(seed);
            var enemies = new List<EnemyAgent>(GameObject.FindObjectsOfType<EnemyAgent>());
            enemies.Sort((a, b) => a.GlobalID - b.GlobalID);

            foreach (var enemy in enemies)
            {
                TimedEvents.StartTimedEvent(ApplyScale(enemy, Time, random.Next(75, 125) / 100f), this);
                enemy.transform.localScale = enemy.transform.localScale * (random.Next(75, 125) / 100f);
            }
        }

        public override void TriggerHost()
        {
            int randomSeed = Main.rnd.Next(int.MinValue, int.MaxValue);

            this.TriggerCommon(randomSeed);

            this.TriggerClient(new RAES(randomSeed));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAES
    {
        public int Seed;

        public RAES(int seed)
        {
            this.Seed = seed;
        }
    }
}
