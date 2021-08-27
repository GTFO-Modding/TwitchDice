using Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Twitch.Events.D4
{
    public class MakePlayersJump : OldDiceEvent<NetworkedNoData>
    {
        public override bool RequireNetworking => true;

        public override bool HasNetworkData => false;

        public override string EventName => "All players jump";

        public override string EventId => "d4_jump";

        public override DiceTier Tier => DiceTier.D4;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override NetworkedNoData TriggerHost()
        {
            MakePlayerJump();
            return new NetworkedNoData();
        }

        protected override void TriggerClient(NetworkedNoData NetworkInfo)
        {
            MakePlayerJump();
        }

        private void MakePlayerJump()
        {
            if (PlayerManager.TryGetLocalPlayerAgent(out PlayerAgent agent))
            {
                agent.Locomotion.ChangeState(PlayerLocomotion.PLOC_State.Jump, true);
            }
        }
    }
}
