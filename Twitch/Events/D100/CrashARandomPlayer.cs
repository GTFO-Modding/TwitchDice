using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Util;
using Player;

namespace TwitchDice.Twitch.Events.D100
{
    public class CrashARandomPlayer : DiceEvent<PCrashARandomPlayer>
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
            if (NetworkInfo.PlayerName == PlayerManager.GetLocalPlayerAgent().PlayerName)
            {
                //LOL
                UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
            }
        }

        protected override PCrashARandomPlayer TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player, false))
            {
                return new PCrashARandomPlayer() { PlayerName = player.PlayerName  };
            }

            return new PCrashARandomPlayer();
        }
    }

    public struct PCrashARandomPlayer
    {
        public string PlayerName;
    }
}
