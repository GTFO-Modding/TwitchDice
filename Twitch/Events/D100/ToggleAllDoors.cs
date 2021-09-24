using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Twitch.Events.D100
{
    public class ToggleAllDoors : DiceEvent
    {
        public override string EventName => "Toggle All Doors";

        public override string EventID => "toggleAll";

        protected override DiceTier DiceTier => DiceTier.D100;

        public override void TriggerHost()
        {
            foreach (iLG_Door_Core doorCore in Builder.Current.m_currentFloor.GetComponentsInChildren<iLG_Door_Core>())
            {
                doorCore.AttemptOpenCloseInteraction(false);
            }
        }
    }

    /*public class ToggleAllDoors : global::DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Toggle All Doors";

        public override string EventId => "d100_toggleAllDoors";

        public override DiceTier Tier => DiceTier.D100;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            foreach (iLG_Door_Core doorCore in Builder.Current.m_currentFloor.GetComponentsInChildren<iLG_Door_Core>())
            {
                doorCore.AttemptOpenCloseInteraction(false);
            }
            return new NoNetworkData();
        }
    }*/
}
