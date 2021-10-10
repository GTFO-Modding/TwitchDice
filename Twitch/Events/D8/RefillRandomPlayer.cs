

using Gear;
using Player;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D8
{
    public class RefillRandomPlayer : DiceEvent<RRP>
    {
        public override string EventName => "Resupply Random";

        public override string EventID => "refillrandom";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override void ReceiveClient(ulong sender, RRP packet)
        {
            if (PlayerUtil.LocalPlayerAgent.Owner.Lookup == packet.PlayerID)
            {
                this.TriggerCommon();
            }
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
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                if (player.Owner.IsMaster)
                {
                    this.TriggerCommon();
                }
                else
                {
                    this.TriggerClient(new RRP(player));
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RRP
    {
        public ulong PlayerID;

        public RRP(PlayerAgent player)
        {
            this.PlayerID = player.Owner.Lookup;
        }
    }
}
