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
    /*public class RandomFireWeapon : global::DiceEvent<TargetPlayer>
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

            switch(wieldedItem.GetIl2CppType().Name)
            {
                case "MeleeWeaponFirstPerson":
                    MeleeWeaponFirstPerson melee = wieldedItem as MeleeWeaponFirstPerson;
                    melee.ChangeState(eMeleeWeaponState.AttackMissRight);
                    break;

                //case "BulletWeapon":
                //    BulletWeapon weapon = wieldedItem as BulletWeapon;
                //    if (weapon.m_wasOutOfAmmo)
                //    {
                //        weapon.TryTriggerReloadSequence();
                //        return;
                //    }
                //    weapon.m_archeType.m_firePressed = true;
                //    break;
                //
                //case "Shotgun":
                //    Shotgun shotgun = wieldedItem as Shotgun;
                //    shotgun.m_archeType.m_firePressed = true;
                //    break;
            }
        }
    }*/
}
