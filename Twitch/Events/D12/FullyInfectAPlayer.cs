using AK;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D12
{
    public class FullyInfectAPlayer : DiceEvent<Splat>
    {
        public override string EventName => "Fully Infect A Player";

        public override string EventID => "infect";

        protected override DiceTier DiceTier => DiceTier.D12;

        public override void ReceiveClient(ulong sender, Splat packet)
        {
            PlaySplat();
        }

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target))
            {
                target.Damage.ModifyInfection(new pInfection() { amount = 10, mode = pInfectionMode.Add }, true, true);
            }

            if (target.IsLocallyOwned)
                PlaySplat();
            else
                TriggerClient(new Splat(), target.Owner);
        }

        private void PlaySplat()
        {
            if (ScreenLiquidManager.TryApply(ScreenLiquidSettingName.spitterJizz, PlayerUtil.LocalPlayerAgent.Position, 10))
            {
                PlayerUtil.LocalPlayerAgent.Sound.Post(EVENTS.VISOR_SPLATTER_INFECTION);
            }
        }
    }
    public struct Splat
    {

    }



    /*
    public class FullyInfectAPlayer : global::DiceEvent<TargetPlayer>
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
            PlaySplat(NetworkInfo);
        }

        protected override TargetPlayer TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target))
            {
                target.Damage.ModifyInfection(new pInfection() { amount = 10, mode = pInfectionMode.Add }, true, true);
            }

            TargetPlayer targetedPlayer = new TargetPlayer(target);
            PlaySplat(targetedPlayer);

            return targetedPlayer;
        }

        private void PlaySplat(TargetPlayer NetworkInfo)
        {
            if (NetworkInfo.TargetedPlayer == LocalPlayer.PlayerSlotIndex)
            {
                if (ScreenLiquidManager.TryApply(ScreenLiquidSettingName.spitterJizz, LocalPlayer.Position, 10))
                {
                    LocalPlayer.Sound.Post(EVENTS.VISOR_SPLATTER_INFECTION);
                }
            }
        }
    }*/
}
