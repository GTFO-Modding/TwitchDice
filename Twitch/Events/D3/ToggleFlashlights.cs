using Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Twitch.Events.D3
{
    public class ToggleFlashlights : OldDiceEvent<NetworkedNoData>
    {
        public override bool RequireNetworking => true;

        public override bool HasNetworkData => false;

        public override string EventName => "Toggle Flashlights";

        public override string EventId => "d3_toggleF";

        public override DiceTier Tier => DiceTier.D3;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override NetworkedNoData TriggerHost()
        {
            ToggleFlashlight();
            return new NetworkedNoData();
        }

        protected override void TriggerClient(NetworkedNoData NetworkInfo)
        {
            ToggleFlashlight();
        }

        private void ToggleFlashlight()
        {
            if (PlayerManager.TryGetLocalPlayerAgent(out PlayerAgent agent))
            {
                agent.Sync.WantsToSetFlashlightEnabled(!agent.Inventory.FlashlightEnabled, false);
            }
        }
    }
}