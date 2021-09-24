using AK;
using Enemies;
using GameData;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class Birth : DiceEvent
    {
        public override string EventName => "Birth";

        public override string EventID => "birth";

        protected override DiceTier DiceTier => DiceTier.D12;

        private int birthCount = 10;

        public override void TriggerHost()
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
