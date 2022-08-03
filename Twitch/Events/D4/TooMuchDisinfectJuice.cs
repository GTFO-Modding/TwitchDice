using Player;
using TwitchDice.Utilities;
using UnityEngine;
using AK;

namespace TwitchDice.Twitch.Events.D4
{
    public class TooMuchDisinfect : DiceEvent<TMD>
    {
        public override string EventName => "Too Much Juice";
        public override string EventDescription => "Applies a lot of disinfection juice to a random player.";
        public override string EventID => "juice";

        protected override DiceTier DiceTier => DiceTier.D4;

        const int splatAmount = 10;

        public override void ReceiveClient(ulong sender, TMD packet)
        {
            PlaySplat();
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
            if (target.IsLocallyOwned)
            {
                PlaySplat();
            }
            else
            {
                this.TriggerClient(new TMD(), target.Owner);
            }
        }

        private static void PlaySplat()
        {
            for (int i = 0; i < splatAmount; i++)
            {
                ScreenLiquidManager.DirectApply(ScreenLiquidSettingName.disinfectionPack_Apply, new Vector2(0.5f, 0.5f), Vector2.zero);
                ScreenLiquidManager.DirectApply(ScreenLiquidSettingName.disinfectionPack_Apply, new Vector2(0.5f, 0.5f), Vector2.zero);
                ScreenLiquidManager.DirectApply(ScreenLiquidSettingName.disinfectionPack_Apply, new Vector2(0.5f, 0.5f), Vector2.zero);
            }
            PlayerUtil.LocalPlayerAgent.Sound.Post(EVENTS.DISINFECTION_SPRAY_ON_VISOR);
        }
    }

    public struct TMD
    {
    }
    //public class TooMuchDisinfectJuice : global::DiceEvent<TargetPlayer>
    //{
    //    public override bool RequireNetworking => true;
    //
    //    public override bool HasNetworkData => true;
    //
    //    public override string EventName => "Too Much Juice";
    //
    //    public override string EventId => "d4";
    //
    //    public override DiceTier Tier => DiceTier.D4;
    //
    //    public override bool CanBeTriggered()
    //    {
    //        return true;
    //    }
    //
    //    protected override void TriggerClient(TargetPlayer NetworkInfo)
    //    {
    //        if (NetworkInfo.TargetedPlayer == LocalPlayer.PlayerSlotIndex)
    //        {
    //            Splat();
    //        }
    //    }
    //
    //    protected override TargetPlayer TriggerHost()
    //    {
    //        PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player);
    //
    //        if (player.IsLocallyOwned)
    //        {
    //            Splat();
    //        }
    //
    //        return new TargetPlayer(player);
    //    }
    //
    //    private void Splat()
    //    {
    //        for (int i = 0; i < 40; i++)
    //        {
    //            ScreenLiquidManager.DirectApply(ScreenLiquidSettingName.disinfectionPack_Apply, new Vector2(0.5f, 0.5f), Vector2.zero);
    //        }
    //
    //    }
    //}
}
