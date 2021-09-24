using Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Twitch.Events.D4
{
    public class AutoJump : DiceEvent<Jump>
    {
        public override string EventName => "yump";

        public override string EventID => "jump";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override void ReceiveClient(ulong sender, Jump packet)
        {
            MakePlayerJump();
        }

        public override void TriggerHost()
        {
            MakePlayerJump();
            TriggerClient(new Jump());
        }

        private void MakePlayerJump()
        {
            if (PlayerManager.TryGetLocalPlayerAgent(out PlayerAgent agent))
            {
                if (agent.Locomotion.m_currentStateEnum != PlayerLocomotion.PLOC_State.Downed)
                    agent.Locomotion.ChangeState(PlayerLocomotion.PLOC_State.Jump, true);
            }
        }
    }

    public struct Jump
    {

    }
    /*public class MakePlayersJump : global::DiceEvent<NetworkedNoData>
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
    }*/
}
