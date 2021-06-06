using AIGraph;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Util;

namespace TwitchDice.Twitch.Events
{
    public class ToggleRandomWeakDoor : DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Toggle Weak Door";

        public override DiceTier Tier => DiceTier.D3;

        public override string EventId => "d3_weakOpen";

        public override bool CanBeTriggered()
        {
            if (PlayerManager.GetLocalPlayerAgent().m_courseNode.m_portals == null) return false;
            try
            {
                foreach (var portal in PlayerManager.GetLocalPlayerAgent().m_courseNode.m_portals)
                {
                    if (portal.m_hasGate)
                    {
                        if (portal.m_door == null) continue;
                        if (portal.m_door.DoorType == LevelGeneration.eLG_DoorType.Weak && portal.m_door.LastStatus != LevelGeneration.eDoorStatus.Closed_BrokenCantOpen)
                        {
                            return true;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }

            return false;
        }

        protected override NoNetworkData TriggerHost()
        {
            List<AIG_CoursePortal> cards = new List<AIG_CoursePortal>();
            foreach (var item in PlayerManager.GetLocalPlayerAgent().m_courseNode.m_portals)
            {
                cards.Add(item);
            }
            cards.Shuffle();

            foreach (var card in cards)
            {
                if (card.m_hasGate)
                {
                    if (card.m_door == null) continue;
                    if (card.m_door.DoorType == LevelGeneration.eLG_DoorType.Weak && card.m_door.LastStatus != LevelGeneration.eDoorStatus.Closed_BrokenCantOpen)
                    {
                        card.m_door.AttemptOpenCloseInteraction(false);
                        return new NoNetworkData();
                    }
                }
            }
            return new NoNetworkData();
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }
    }
}
