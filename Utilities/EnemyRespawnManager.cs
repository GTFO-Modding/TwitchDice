

using AIGraph;
using Enemies;
using SNetwork;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class EnemyRespawnManager
    {
        private class DeadEnemy
        {
            public uint m_id;
            public float m_x;
            public float m_y;
            public float m_z;
            public AIG_CourseNode m_node;

            public DeadEnemy(EnemyAgent enemy)
            {
                this.m_id = enemy.EnemyDataID;
                this.m_x = enemy.Position.x;
                this.m_y = enemy.Position.y;
                this.m_z = enemy.Position.z;
                this.m_node = enemy.CourseNode;
            }

            public void Spawn()
            {
                EnemyAllocator.Current.SpawnEnemy(this.m_id, this.m_node, Agents.AgentMode.Agressive, new Vector3(this.m_x, this.m_y, this.m_z), Quaternion.identity);
            }
        }

        private static List<DeadEnemy> s_enemies;

        public static void AddEnemy(EnemyAgent enemy)
        {
            s_enemies.Add(new DeadEnemy(enemy));
        }

        public static void RespawnAll()
        {
            while (s_enemies.Count > 0)
            {
                s_enemies[0].Spawn();
                s_enemies.RemoveAt(0);
            }
        }

        internal static void Init()
        {
            Hooks.Cleanup += Cleanup;
            s_enemies = new List<DeadEnemy>();
        }

        private static void Cleanup()
        {
            s_enemies.Clear();
        }
    }
}
