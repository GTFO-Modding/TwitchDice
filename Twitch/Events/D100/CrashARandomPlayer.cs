using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using Player;

namespace TwitchDice.Twitch.Events.D100
{
    public class CrashARandomPlayer : OldDiceEvent<PCrashARandomPlayer>
    {
        private static bool CrashedPlayer = false;
        public override bool RequireNetworking => true;

        public override bool HasNetworkData => true;

        public override string EventName => "Crash Player";

        public override string EventId => "d100_crashPlayer";

        public override DiceTier Tier => DiceTier.D100;

        public override bool CanBeTriggered()
        {
            if (PlayerUtil.PlayerCount > 1 && CrashedPlayer == false)
            {
                CrashedPlayer = true;
                return true;
            }
            return false;
        }

        protected override void TriggerClient(PCrashARandomPlayer NetworkInfo)
        {
            if (NetworkInfo.PlayerSlot == LocalPlayer.PlayerSlotIndex)
            {
                //LOL
                UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
            }
        }

        protected override PCrashARandomPlayer TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player, false))
            {
                return new PCrashARandomPlayer() { PlayerSlot = player.PlayerSlotIndex  };
            }

            return new PCrashARandomPlayer();
        }
    }

    public struct PCrashARandomPlayer
    {
        public int PlayerSlot;
    }
}
