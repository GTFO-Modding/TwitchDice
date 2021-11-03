using Enemies;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D3
{
    public class EnemyPulse : DiceEvent
    {
        public override string EventName => "Pulse";

        public override string EventID => "pulse";

        protected override DiceTier DiceTier => DiceTier.D3;

        private readonly int range = 200;

        public override bool CanBeTriggered()
        {
            return EnemyUpdateManager.Summary.m_closeEnemyCount > 0 && DramaManager.CurrentStateEnum == DRAMA_State.Sneaking;
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);

            var noiseData = new NM_NoiseData
            {
                position = target.EyePosition,
                radiusMin = 0,
                radiusMax = range,
                node = target.CourseNode,
                type = NM_NoiseType.PulseOnly,
                raycastFirstNode = false,
                includeToNeightbourAreas = true
            };

            NoiseManager.MakeNoise(noiseData);
        }
    }

    /*public class EnemyPulse : global::DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Pulse";

        public override string EventId => "d3_pulse";

        public override DiceTier Tier => DiceTier.D3;

        private readonly int range = 200;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);

            var noiseData = new NM_NoiseData
            {
                position = target.EyePosition,
                radiusMin = 0,
                radiusMax = range,
                node = target.CourseNode,
                type = NM_NoiseType.PulseOnly,
                raycastFirstNode = false,
                includeToNeightbourAreas = true
            };

            NoiseManager.MakeNoise(noiseData);

            return new NoNetworkData();
        }
    }*/
}
