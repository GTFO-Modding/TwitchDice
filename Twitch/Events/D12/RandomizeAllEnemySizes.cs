using Enemies;
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
        public override string EventDescription => "Randomizes all enemy sizes.";
        public override string EventID => "randomizeSizes";

        protected override DiceTier DiceTier => DiceTier.D12;

        protected override bool ForceDisable => true;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time the enemy sizes will be random", 60);
            return cfg;
        }

        public override void ReceiveClient(ulong sender, RAES packet)
        {
            this.TriggerCommon(packet.Seed);
        }

        private static IEnumerator ApplyScale(EnemyAgent enemy, float seconds, float multiplier)
        {
            Vector3 oldScale = enemy.transform.localScale;
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

            foreach (EnemyAgent? enemy in enemies)
            {
                TimedEvents.StartTimedEvent(ApplyScale(enemy, this.Time, random.Next(75, 125) / 100f), this);
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
