using AK;
using Enemies;
using Player;
using SNetwork;
using System.Collections.Generic;
using System.Linq;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D20
{
    public class PlayBossSoundAndSpawnItMaybe : DiceEventWithConfig<SoundTheseBalls, PlayBossSoundAndSpawnItMaybe.RundownConfig>
    {
        public override string EventName => "Surprise!";
        public override string EventDescription => "Maybe spawns a random enemy.";
        public override string EventID => "surpriseme";

        protected override DiceTier DiceTier => DiceTier.D20;

        public float Chance => this.Config.ClientConfig.GetValue<float>(nameof(this.Chance));

        public override void ReceiveClient(ulong sender, SoundTheseBalls packet)
        {
            CellSound.Post(EVENTS.BIRTHERGIVEBIRTH, PlayerUtil.LocalPlayerAgent.Position);
        }

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public List<LevelSettings> Levels { get; set; } = new()
            {
                new LevelSettings()
                {
                    EnemySpawnPool = new()
                    {
                        37U
                    },
                    ExpeditionIndex = 0,
                    Tier = eRundownTier.TierA
                }
            };
            public uint DefaultEnemyID { get; set; } = 37U;

            protected override void InitImpl(IDiceEvent diceEvent)
            {
                if (this.Levels == null)
                {
                    this.Levels = new();
                }

                this.Levels.RemoveAll((level) => level == null);

                foreach (LevelSettings level in this.Levels)
                {
                    level.Init();
                }
            }

            public void GetEnemyID(out uint enemyID)
            {
                LevelSettings[] settings = this.Levels.Where((level) => level.IsActive).ToArray();
                System.Random random = new();

                if (settings.Length == 0)
                {
                    enemyID = this.DefaultEnemyID;
                    return;
                }

                LevelSettings setting;
                if (settings.Length > 1)
                {
                    setting = settings[random.Next(settings.Length)];
                }
                else
                {
                    setting = settings[0];
                }

                enemyID = GetRandomEntry(setting.EnemySpawnPool, random, this.DefaultEnemyID);
            }

            private static T GetRandomEntry<T>(List<T> list, System.Random random, T @default)
            {
                if (list == null || list.Count == 0)
                {
                    return @default;
                }
                else if (list.Count > 1)
                {
                    return list[random.Next(list.Count)];
                }
                else
                {
                    return list[0];
                }
            }
        }

        public sealed class LevelSettings
        {
            public List<uint> EnemySpawnPool { get; set; } = new();
            public eRundownTier Tier { get; set; }
            public int ExpeditionIndex { get; set; }

            public void Init()
            {
                if (this.EnemySpawnPool == null)
                {
                    this.EnemySpawnPool = new();
                }
            }

            public bool IsActive
            {
                get
                {
                    pActiveExpedition exp = SNet.GetLocalCustomData<pActiveExpedition>();

                    return exp.expeditionIndex == this.ExpeditionIndex && exp.tier == this.Tier;
                }
            }
        }

        protected override DiceEventConfig<RundownConfig> GetConfig()
        {
            DiceEventConfig<RundownConfig> cfg = base.GetConfig();
            cfg.ClientConfig.Add(nameof(this.Chance), "The chance to spawn the enemy", 0.5f);
            return cfg;
        }

        public override void TriggerHost()
        {
            CellSound.Post(EVENTS.BIRTHERGIVEBIRTH, PlayerUtil.LocalPlayerAgent.Position);

            float chance = UnityEngine.Random.Range(0f, 1f);
            if (chance < this.Chance)
            {
                this.RundownCfg.GetEnemyID(out uint enemyID);
                // we do a little trolling

                if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent randomPlayer))
                {
                    Il2CppSystem.Collections.Generic.List<AIGraph.AIG_CoursePortal>? portals = randomPlayer.CourseNode.m_portals;
                    AIGraph.AIG_CoursePortal randomPortal = portals[UnityEngine.Random.Range(0, portals.Count)];
                    AIGraph.AIG_CourseNode node = randomPortal.m_nodeB;
                    EnemyAllocator.Current.SpawnEnemy(enemyID, node, Agents.AgentMode.Agressive, node.Position, Quaternion.identity);
                }
                
            }

            this.TriggerClient();

        }
    }

    public struct SoundTheseBalls
    { }
}
