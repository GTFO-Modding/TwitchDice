using UnityEngine;
using TwitchDice.Utilities;
using Player;

namespace TwitchDice.Twitch.Events.D6
{
    public class LaunchPlayers : DiceEvent<Launch>
    {
        public override string EventName => "Blastoff!";
        public override string EventDescription => "Launches the players into the air.";
        public override string EventID => "launch";

        protected override DiceTier DiceTier => DiceTier.D6;

        private const int Magnitude = 5;

        public override void TriggerHost()
        {
            Jump();
            this.TriggerClient();
        }

        public override void ReceiveClient(ulong sender, Launch packet)
        {
            Jump();
        }

        private static void Jump()
        {
            PlayerAgent localPlayer = PlayerUtil.LocalPlayerAgent;
            if (localPlayer.Alive)
            {
                PlayerUtil.LocalPlayerAgent.PlayerCharacterController.Move(Vector3.up * Magnitude);
            }
        }
    }

    public struct Launch { }
}
