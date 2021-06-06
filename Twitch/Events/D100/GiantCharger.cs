using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Twitch.Events.D100
{
    public class GiantCharger : DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Unkillable Giant Charger";

        public override string EventId => "d100_spawnGiantCharger";

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

            return new NoNetworkData();
        }
    }
}
