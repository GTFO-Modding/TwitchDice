using Enemies;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class AlertALLEnemies : DiceEvent
    {
        public override string EventName => "Sleepers Mad";

        public override string EventID => "alertEnemies";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override void TriggerHost()
        {
            var noiseMaker = PlayerUtil.LocalPlayerAgent.Cast<INM_NoiseMaker>();
            foreach (var enemy in GameObject.FindObjectsOfType<EnemyAgent>())
            {
                NoiseManager.MakeNoise(new NM_NoiseData()
                {
                    includeToNeightbourAreas = true,
                    node = enemy.CourseNode,
                    noiseMaker = noiseMaker,
                    position = enemy.Position,
                    radiusMax = 5f,
                    radiusMin = 0f,
                    raycastFirstNode = true,
                    type = NM_NoiseType.InstaDetect,
                    yScale = 10f
                });
            }
        }
    }
}
