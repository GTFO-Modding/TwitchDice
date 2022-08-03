using AK;
using Enemies;
using GameData;
using Player;
using SNetwork;
using System.Collections.Generic;
using System.Linq;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class Birth : DiceEventWithConfig<Birth.RundownConfig>
    {
        public override string EventName => "Birth";
        public override string EventDescription => "Spawns a bunch of enemies on the player.";
        public override string EventID => "birth";

        protected override DiceTier DiceTier => DiceTier.D12;

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public List<LevelSettings> Levels { get; set; } = new()
            {
                new LevelSettings()
                {
                    BirthIDs = new()
                    {
                        37U
                    },
                    SpawnCounts = new()
                    {
                        10
                    },
                    ExpeditionIndex = 0,
                    Tier = eRundownTier.TierA
                }
            };
            public uint DefaultBirthID { get; set; } = 37U;
            public int DefaultAmount { get; set; } = 10;

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

            public void GetBirthSettings(out uint birthID, out int birthAmount)
            {
                LevelSettings[] settings = this.Levels.Where((level) => level.IsActive).ToArray();
                System.Random random = new();

                if (settings.Length == 0)
                {
                    birthAmount = this.DefaultAmount;
                    birthID = this.DefaultBirthID;
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

                birthID = GetRandomEntry(setting.BirthIDs, random, this.DefaultBirthID);
                birthAmount = GetRandomEntry(setting.SpawnCounts, random, this.DefaultAmount);
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
            public List<uint> BirthIDs { get; set; } = new();
            public List<int> SpawnCounts { get; set; } = new();
            public eRundownTier Tier { get; set; }
            public int ExpeditionIndex { get; set; }

            public void Init()
            {
                if (this.BirthIDs == null)
                {
                    this.BirthIDs = new();
                }
                if (this.SpawnCounts == null)
                {
                    this.SpawnCounts = new();
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

        public override void TriggerHost()
        {
            if (!PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target))
            {
                return;
            }

            this.RundownCfg.GetBirthSettings(out uint birthID, out int birthAmount);


            EnemyGroupDataBlock data = EnemyGroupDataBlock.GetBlock(birthID);
            CellSound.Post(EVENTS.BIRTHER_BABY_DROP, target.Position);
            for (int i = 0; i < birthAmount; i++)
            {
                Mastermind.Current.SpawnGroup(
                target.Position,
                target.CourseNode,
                EnemyGroupType.Hunters,
                eEnemyGroupSpawnType.Position,
                data,
                0);
            }
        }
    }

    /*public class BirthEffectOnPlayer : global::DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Birth";

        public override string EventId => "d12_birth";

        public override DiceTier Tier => DiceTier.D12;

        public override bool CanBeTriggered()
        {
            return true;
        }

        private int birthCount = 10;

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target))
            {
                EnemyGroupDataBlock data = GameDataBlockBase<EnemyGroupDataBlock>.GetBlock(37U);
                CellSound.Post(EVENTS.BIRTHER_BABY_DROP, target.Position);
                for (int i = 0; i < birthCount; i++)
                {
                    Mastermind.Current.SpawnGroup(
                    target.Position,
                    target.CourseNode,
                    EnemyGroupType.Hunters,
                    eEnemyGroupSpawnType.Position,
                    data,
                    0);
                }
            }


            return NoNetworkData;
        }
    }*/
}
