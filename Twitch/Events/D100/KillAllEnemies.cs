
using Enemies;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D100
{
    public class KillAllEnemies : DiceEvent
    {
        public override string EventName => "The Ugly";

        public override string EventID => "ugly";

        protected override DiceTier DiceTier => DiceTier.D100;

        public override void TriggerHost()
        {
            foreach (var enemy in GameObject.FindObjectsOfType<EnemyAgent>())
            {
                enemy.Damage?.InstantDead(true);
            }
        }
    }
}
