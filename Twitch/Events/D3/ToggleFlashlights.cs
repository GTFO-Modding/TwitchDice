using Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Twitch.Events.D3
{
    public class ToggleFlashlights : DiceEvent<Flashlight>
    {
        public override string EventName => "Toggle Flashlights";

        public override string EventID => "toggleF";

        protected override DiceTier DiceTier => DiceTier.D3;

        public override void ReceiveClient(ulong sender, Flashlight packet)
        {
            ToggleFlashlight();
        }

        public override void TriggerHost()
        {
            ToggleFlashlight();
            TriggerClient(new Flashlight());
        }

        private void ToggleFlashlight()
        {
            if (PlayerManager.TryGetLocalPlayerAgent(out PlayerAgent agent))
            {
                agent.Sync.WantsToSetFlashlightEnabled(!agent.Inventory.FlashlightEnabled, false);
            }
        }
    }

    public struct Flashlight
    {

    }

    /*public class ToggleFlashlights : global::DiceEvent<NetworkedNoData>
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
    }*/
}