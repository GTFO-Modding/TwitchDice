using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D4
{
    public class FireShooterProjectile : DiceEvent
    {
        public override string EventName => "Shooter!";

        public override string EventID => "shooterP";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent sourcePlayer);
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target, false))
            {
                ProjectileManager.WantToFireTargeting(ProjectileType.TargetingSmall, target, sourcePlayer.EyePosition, sourcePlayer.Rotation * Vector3.forward, 1, 0f);
            }
        }
    }

    /*class FireShooterProjectile : global::DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Shooter Projectile";

        public override string EventId => "d4_shooterProj";

        public override DiceTier Tier => DiceTier.D4;

        public override bool CanBeTriggered()
        {
            return PlayerUtil.PlayerCount > 1;
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            var localPlayer = PlayerManager.GetLocalPlayerAgent();
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target, false))
            {
                ProjectileManager.WantToFireTargeting(ProjectileType.TargetingSmall, target, localPlayer.CamPos, localPlayer.Rotation * Vector3.forward, 1, 0f);
            }
            return new NoNetworkData();
        }
    }*/
}
