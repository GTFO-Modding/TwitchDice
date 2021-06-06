using System;
using System.Collections.Generic;
using System.Text;
using AIGraph;
using Enemies;
using Player;
using TwitchDice.Util;

namespace TwitchDice.Twitch.Events.D100
{
    public class GiantCharger : DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Unkillable Giant Charger";

        public override string EventId => "d100_spawnGiantCharger";

        public override DiceTier Tier => DiceTier.D100;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            var localPlayer = PlayerManager.GetLocalPlayerAgent();
            var spawnCenter = localPlayer.CourseNode;

            var potentialSpawns = new List<AIG_CourseNode>();
            foreach (var item in spawnCenter.m_portals)
            {
                potentialSpawns.Add(item.GetOppositeNode(spawnCenter));
            }
            
            var spawnNode = potentialSpawns.GetRandomElement<AIG_CourseNode>();
            var spawnPosition = spawnNode.GetRandomPositionInside();


            var enemy = EnemyAllocator.Current.SpawnEnemy(
                Config.GIANT_CHARGER_ID,
                spawnNode,
                Agents.AgentMode.Agressive,
                spawnPosition, 
                default);

            enemy.Damage.HealthMax = float.MaxValue;
            enemy.Damage.Health = float.MaxValue;

            return new NoNetworkData();
        }
    }
}
