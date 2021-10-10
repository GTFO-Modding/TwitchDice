using System;
using System.Collections.Generic;
using System.Text;
using AIGraph;
using Enemies;
using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D100
{
    public class Bob : DiceEvent<BobData>
    {
        public override string EventName => "Bob";

        public override string EventID => "bob";

        protected override DiceTier DiceTier => DiceTier.D100;

        public override void ReceiveClient(ulong sender, BobData packet)
        {
            ushort enemyID = packet.GlobalID;
            int nodeID = packet.NodeID;

            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                var spawnCenter = player.CourseNode;
                foreach (var portal in spawnCenter.m_portals)
                {
                    var oppositeNode = portal.GetOppositeNode(spawnCenter);
                    if (oppositeNode.NodeID == nodeID)
                    {
                        foreach (var enemy in oppositeNode.m_enemiesInNode)
                        {
                            if (enemy.GlobalID == enemyID)
                            {
                                enemy.Damage.Health = float.MaxValue;
                            }
                        }
                    }
                }
            }
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent localPlayer);
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

            enemy.Damage.Health = float.MaxValue;

            TriggerClient(new BobData() { GlobalID = enemy.GlobalID, NodeID = spawnNode.NodeID });
        }
    }

    public struct BobData
    {
        public ushort GlobalID;
        public int NodeID; 
    }
    /*public class GiantCharger : global::DiceEvent<NoNetworkData>
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

            enemy.Damage.IsImortal = true;

            return new NoNetworkData();
        }
    }*/
}
