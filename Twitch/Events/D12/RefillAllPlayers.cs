

using Gear;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D12
{
    public class RefillAllPlayers : DiceEvent<RAP>
    {
        public override string EventName => "Resupply All";

        public override string EventID => "refillall";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override void ReceiveClient(ulong sender, RAP packet)
        {
            this.TriggerCommon();
        }

        private void TriggerCommon()
        {
            PlayerUtil.LocalPlayerAgent.GiveAmmoRel(1f, 1f, 1f);
            PlayerUtil.LocalPlayerAgent.GiveHealth(1f);

            var weapons = GameObject.FindObjectsOfType<BulletWeapon>();
            foreach (var weapon in weapons)
            {
                weapon.m_clip = weapon.GetMaxClip();
            }
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            this.TriggerCommon();
        }
    }

    public struct RAP
    { }
}
