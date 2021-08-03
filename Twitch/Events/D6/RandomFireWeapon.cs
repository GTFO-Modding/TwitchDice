using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using Player;
using Gear;
using BoosterImplants;
using SNetwork;

namespace TwitchDice.Twitch.Events.D6
{
    public class RandomFireWeapon : DiceEvent<TargetPlayer>
    {
        public override bool RequireNetworking => true;
    
        public override bool HasNetworkData => true;
    
        public override string EventName => "Fire Weapon";
    
        public override string EventId => "d6_fireWeapon";
    
        public override DiceTier Tier => DiceTier.D6;
    
        public override bool CanBeTriggered()
        {
            return true;
        }
    
        protected override void TriggerClient(TargetPlayer NetworkInfo)
        {
            //MakeTargetFire(NetworkInfo.TargetedPlayer);
        }
    
        protected override TargetPlayer TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target, true);

            MakeTargetFire(target);
            return new TargetPlayer(target);
        }

        void MakeTargetFire(PlayerAgent target)
        {
            var wieldedItem = target.Inventory.m_wieldedItem;
            Log.Debug(wieldedItem.GetIl2CppType().Name);
        }
    }
}
