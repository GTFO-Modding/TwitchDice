using Player;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class LaunchPlayers : DiceEvent<Launch>
    {
        public override string EventName => "Blastoff!";

        public override string EventID => "launch";

        protected override DiceTier DiceTier => DiceTier.D6;

        private const int Magnitude = 5;

        public override void TriggerHost()
        {
            Jump();
            TriggerClient(new Launch());
        }

        public override void ReceiveClient(ulong sender, Launch packet)
        {
            Jump();
        }

        private void Jump()
        {
            PlayerUtil.LocalPlayerAgent.PlayerCharacterController.Move(Vector3.up * Magnitude);
        }
    }

    public struct Launch { }
}
