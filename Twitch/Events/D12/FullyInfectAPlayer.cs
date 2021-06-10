using AK;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class FullyInfectAPlayer : DiceEvent<TargetPlayer>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Fully Infect A Player";

        public override string EventId => "d12_infect";

        public override DiceTier Tier => DiceTier.D12;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(TargetPlayer NetworkInfo)
        {
            if (NetworkInfo.TargetedPlayer == LocalPlayer.PlayerSlotIndex)
            {
                if (ScreenLiquidManager.TryApply(ScreenLiquidSettingName.spitterJizz, LocalPlayer.Position, 2))
                {
                    LocalPlayer.Sound.Post(EVENTS.VISOR_SPLATTER_INFECTION);
                }
            }
        }

        protected override TargetPlayer TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target))
            {
                target.Damage.ModifyInfection(new pInfection() { amount = 10, mode = pInfectionMode.Add }, true, true);
            }

            return new TargetPlayer(target);
        }
    }
}
