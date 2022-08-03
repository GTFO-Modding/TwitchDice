using Gear;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Player;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D8
{
    public class RefillRandomPlayer : DiceEvent<RRP>
    {
        public override string EventName => "Resupply Random";
        public override string EventDescription => "Refills a random player's supplies.";
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
            PlayerUtil.LocalPlayerAgent.GiveAmmoRel(PlayerUtil.LocalPlayerAgent, 1f, 1f, 1f);
            PlayerUtil.LocalPlayerAgent.GiveHealth(PlayerUtil.LocalPlayerAgent, 1f);

            Il2CppArrayBase<BulletWeapon> weapons = GameObject.FindObjectsOfType<BulletWeapon>();
            foreach (BulletWeapon? weapon in weapons)
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
