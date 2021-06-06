using Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TwitchDice.Util;
using LevelGeneration;

namespace TwitchDice.Twitch.Events.D4
{
    public class FogCloud : DiceEvent<PFogCloud>
    {
        public override bool RequireNetworking => true;

        public override bool HasNetworkData => true;

        public override string EventName => "Fog Cloud";

        public override string EventId => "d4_fogSphere";

        public override DiceTier Tier => DiceTier.D4;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override PFogCloud TriggerHost()
        {
            PFogCloud pFogCloud = new PFogCloud();
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                var pos = player.Position;
                CreateFogSphere(pos);
                pFogCloud.Position = new JsonVector(pos);
            } else
            {
                Log.Error("Failed to get a player agent!");
            }


            return pFogCloud;
        }

        protected override void TriggerClient(PFogCloud NetworkInfo)
        {
            CreateFogSphere(NetworkInfo.Position.Vector3());
        }

        private void CreateFogSphere(Vector3 position)
        {
            var fogSphere = new FogSphereAllocator();
            fogSphere.SetPositionRange(position, 5);
            fogSphere.SetDensity(10);
            fogSphere.SetRadiance(Color.green, 1);
            Log.Debug("Created Fog Sphere");
        }
    }

    public struct PFogCloud
    {
        public JsonVector Position;
    }
}
