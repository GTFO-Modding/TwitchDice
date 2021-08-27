using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D4
{
    public class TooMuchDisinfectJuice : OldDiceEvent<TargetPlayer>
    {
        public override bool RequireNetworking => true;

        public override bool HasNetworkData => true;

        public override string EventName => "Too Much Juice";

        public override string EventId => "d4";

        public override DiceTier Tier => DiceTier.D4;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(TargetPlayer NetworkInfo)
        {
            if (NetworkInfo.TargetedPlayer == LocalPlayer.PlayerSlotIndex)
            {
                Splat();
            }
        }

        protected override TargetPlayer TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player);

            if (player.IsLocallyOwned)
            {
                Splat();
            }

            return new TargetPlayer(player);
        }

        private void Splat()
        {
            for (int i = 0; i < 40; i++)
            {
                ScreenLiquidManager.DirectApply(ScreenLiquidSettingName.disinfectionPack_Apply, new Vector2(0.5f, 0.5f), Vector2.zero);
            }

        }
    }
}
