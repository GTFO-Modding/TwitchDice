

using Enemies;
using System;
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
            int randomSeed = Main.rnd.Next(int.MinValue, int.MaxValue);

            var random = new System.Random(randomSeed);

            foreach (var enemy in GameObject.FindObjectsOfType<EnemyAgent>())
            {
                enemy.transform.localScale = enemy.transform.localScale * (random.Next(75, 125) / 100f);
            }
        }

        public override void TriggerHost()
        {
            int randomSeed = Main.rnd.Next(int.MinValue, int.MaxValue);

            var random = new System.Random(randomSeed);

            foreach (var enemy in GameObject.FindObjectsOfType<EnemyAgent>())
            {
                enemy.transform.localScale = enemy.transform.localScale * (random.Next(20, 150) / 100f);
            }

            this.TriggerClient();
        }
    }

    public struct RAES
    { }
}
