using AIGraph;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D20
{
    public class TeleportPlayerToExplored : DiceEvent<TPE>
    {
        public override string EventName => "Woosh!";
        public override string EventDescription => "Teleports the player to a random explored area.";
        public override string EventID => "tpe";

        protected override DiceTier DiceTier => DiceTier.D20;

        public override void ReceiveClient(ulong sender, TPE packet)
        {
            TpToRandomExploredLocation();
        }

        public override void TriggerHost()
        {
            TpToRandomExploredLocation();
            TriggerClient();
        }

        private void TpToRandomExploredLocation()
        {
            AIG_CourseNode chosenNode = NodeUtil.GetReachableNodes(PlayerUtil.LocalPlayerAgent.CourseNode, 100).GetRandomElement<AIG_CourseNode>();
            Vector3 randomPosInNode = chosenNode.GetRandomPositionInside();
            PlayerUtil.TeleportToPosition(PlayerUtil.LocalPlayerAgent, randomPosInNode);
        }
    }

    public struct TPE { }
}
