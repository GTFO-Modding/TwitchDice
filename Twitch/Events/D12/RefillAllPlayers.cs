using Gear;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D12
{
    public class RefillAllPlayers : DiceEvent<RAP>
    {
        public override string EventName => "Resupply All";
        public override string EventDescription => "Refills Tool, Ammo, and Health of all players.";
        public override string EventID => "refillall";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override void ReceiveClient(ulong sender, RAP packet)
        {
            TriggerCommon();
        }

        private static void TriggerCommon()
        {
            PlayerUtil.LocalPlayerAgent.GiveAmmoRel(PlayerUtil.LocalPlayerAgent, 1f, 1f, 1f);
            PlayerUtil.LocalPlayerAgent.GiveHealth(PlayerUtil.LocalPlayerAgent, 1f);

            var weapons = GameObject.FindObjectsOfType<BulletWeapon>();
            foreach (var weapon in weapons)
            {
                weapon.m_clip = weapon.GetMaxClip();
            }
        }

        public override void TriggerHost()
        {
            this.TriggerClient();
            TriggerCommon();
        }
    }

    public struct RAP
    { }
}
