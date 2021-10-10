

using Enemies;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D12
{
    public class RandomizeAllEnemySizes : DiceEvent<RAES>
    {
        public override string EventName => "Randomize All Enemy Size";

        public override string EventID => "randomizeSizes";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override void ReceiveClient(ulong sender, RAES packet)
        {
            var random = new System.Random(packet.Seed);
            var enemies = new List<EnemyAgent>(GameObject.FindObjectsOfType<EnemyAgent>());
            enemies.Sort((a, b) => a.GlobalID - b.GlobalID);

            foreach (var enemy in enemies)
            {
                enemy.transform.localScale = enemy.transform.localScale * (random.Next(75, 125) / 100f);
            }
        }

        public override void TriggerHost()
        {
            int randomSeed = Main.rnd.Next(int.MinValue, int.MaxValue);

            var enemies = new List<EnemyAgent>(GameObject.FindObjectsOfType<EnemyAgent>());
            enemies.Sort((a, b) => a.GlobalID - b.GlobalID);

            var random = new System.Random(randomSeed);

            foreach (var enemy in enemies)
            {
                enemy.transform.localScale = enemy.transform.localScale * (random.Next(20, 150) / 100f);
            }

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
