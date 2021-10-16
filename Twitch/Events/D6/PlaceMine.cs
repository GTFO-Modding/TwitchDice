using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D6
{
    public class PlaceMine : DiceEvent
    {
        public override string EventName => "Materialize Mine";

        public override string EventID => "mineLook";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
            var mine = new pItemData
            {
                itemID_gearCRC = 125U
            };

            Ray ray = new Ray(target.EyePosition, target.TargetLookDir);
            Physics.Raycast(ray, out RaycastHit hit, 100000, LayerManager.MASK_CAMERA_RAY);
            Vector3 direction = (hit.point - target.EyePosition).normalized;
            var rot = Quaternion.LookRotation(direction, Vector3.up);
            ItemReplicationManager.SpawnItem(mine, null, ItemMode.Instance, hit.point, rot, target.CourseNode, target);
        }
    }
}
