using Player;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class LaunchPlayers : DiceEvent<Jump>
    {
        public override string EventName => "Blastoff!";

        public override string EventID => "launch";

        protected override DiceTier DiceTier => DiceTier.D6;

        private const int Magnitude = 5;

        public override void TriggerHost()
        {
            Jump();
        }

        public override void ReceiveClient(ulong sender, Jump packet)
        {
            Jump();
        }

        private void Jump()
        {
            PlayerUtil.LocalPlayerAgent.Locomotion.m_verticalVelocity = Vector3.up * Magnitude;
        }
    }

    public struct Jump { }
}
