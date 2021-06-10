using Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class MineScatterShot : DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Mine Scatter Shot";

        public override string EventId => "d8_mineSS";

        public override DiceTier Tier => DiceTier.D8;

        private readonly int MineCount = 10;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            var mine = new pItemData
            {
                itemID_gearCRC = 125U
            };
            var hits = SpawnUtil.GetRandomScatterAround(LocalPlayer.Position, MineCount);


            foreach (var hit in hits)
            {
                ItemReplicationManager.SpawnItem(mine, null, ItemMode.Instance, hit.point, Quaternion.Euler(hit.normal), LocalPlayer.CourseNode, LocalPlayer);
            }
            
            return NoNetworkData;
        }
    }
}
