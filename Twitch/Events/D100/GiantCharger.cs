using System.Collections.Generic;
using AIGraph;
using Enemies;
using Player;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D100
{
    public class Bob : DiceEventWithConfig<BobData, Bob.RundownConfig>
    {
        public override string EventName => "Bob";
        public override string EventDescription => "Spawns an invicible giant charger";
        public override string EventID => "bob";

        protected override DiceTier DiceTier => DiceTier.D100;

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public uint GiantChargerID { get; set; } = 39U;
        }

        public override void ReceiveClient(ulong sender, BobData packet)
        {
            ushort enemyID = packet.GlobalID;
            int nodeID = packet.NodeID;

            foreach (PlayerAgent player in PlayerManager.PlayerAgentsInLevel)
            {
                AIG_CourseNode spawnCenter = player.CourseNode;
                foreach (AIG_CoursePortal portal in spawnCenter.m_portals)
                {
                    AIG_CourseNode oppositeNode = portal.GetOppositeNode(spawnCenter);
                    if (oppositeNode.NodeID == nodeID)
                    {
                        foreach (EnemyAgent enemy in oppositeNode.m_enemiesInNode)
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
            AIG_CourseNode spawnCenter = localPlayer.CourseNode;

            var potentialSpawns = new List<AIG_CourseNode>();
            foreach (AIG_CoursePortal item in spawnCenter.m_portals)
            {
                potentialSpawns.Add(item.GetOppositeNode(spawnCenter));
            }

            AIG_CourseNode spawnNode = potentialSpawns.GetRandomElement<AIG_CourseNode>();
            Vector3 spawnPosition = spawnNode.GetRandomPositionInside();


            EnemyAgent? enemy = EnemyAllocator.Current.SpawnEnemy(
                this.RundownCfg.GiantChargerID,
                spawnNode,
                Agents.AgentMode.Agressive,
                spawnPosition,
                default);

            enemy.Damage.Health = float.MaxValue;

            this.TriggerClient(new BobData() { GlobalID = enemy.GlobalID, NodeID = spawnNode.NodeID });
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
