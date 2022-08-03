using Player;
using UnityEngine;
using TwitchDice.Utilities;
using System.Collections.Generic;

namespace TwitchDice.Twitch.Events.D8
{
    public class MineSS : DiceEvent
    {
        public override string EventName => "Mine Scatter Shot";
        public override string EventDescription => "Places a bunch of mines around a random player.";
        public override string EventID => "mineSS";

        protected override DiceTier DiceTier => DiceTier.D8;

        private int MineCount => this.Config.ClientConfig.GetValue<int>(nameof(this.MineCount));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.MineCount), "The number of mines to place", 10);
            return cfg;
        }

        public override void TriggerHost()
        {
            var mine = new pItemData
            {
                itemID_gearCRC = 125U
            };

            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent LocalPlayer);

            List<RaycastHit> hits = SpawnUtil.GetRandomScatterAround(LocalPlayer.EyePosition, this.MineCount);

            foreach (RaycastHit hit in hits)
            {
                Vector3 direction = (hit.point - LocalPlayer.EyePosition).normalized;
                var rot = Quaternion.LookRotation(direction, Vector3.up);
                ItemReplicationManager.SpawnItem(mine, null, ItemMode.Instance, hit.point, rot, LocalPlayer.CourseNode, LocalPlayer);
            }
        }
    }
    /*public class MineScatterShot : global::DiceEvent<NoNetworkData>
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

            var hits = SpawnUtil.GetRandomScatterAround(LocalPlayer.EyePosition, MineCount);

            foreach (var hit in hits)
            {
                Vector3 direction = (hit.point - LocalPlayer.EyePosition).normalized;
                var rot = Quaternion.LookRotation(direction, Vector3.up);
                ItemReplicationManager.SpawnItem(mine, null, ItemMode.Instance, hit.point, rot, LocalPlayer.CourseNode, LocalPlayer);
            }
            
            return NoNetworkData;
        }
    }*/
}
