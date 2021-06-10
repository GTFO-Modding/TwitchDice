using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events
{
    public class RandomizeZoneLighting : DiceEvent<PRandomZoneLighting>
    {
        public override bool RequireNetworking => true;

        public override bool HasNetworkData => true;

        public override string EventName => "Randomize Zone Lighting";

        public override DiceTier Tier => DiceTier.D3;

        public override string EventId => "d3_randomZL";

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override PRandomZoneLighting TriggerHost()
        {
            int seed = Main.rnd.Next(0, 255);
            RandomizeZL(seed);
            return new PRandomZoneLighting() { Seed = seed };
        }

        protected override void TriggerClient(PRandomZoneLighting NetworkInfo)
        {
            RandomizeZL(NetworkInfo.Seed);
        }

        private void RandomizeZL(int seed)
        {
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                System.Random random = new System.Random(seed);
                foreach (var light in player.CourseNode.m_lightsInNode)
                {
                    float r = (float)random.NextDouble();
                    float g = (float)random.NextDouble();
                    float b = (float)random.NextDouble();
                    light.ChangeColor(new Color(r, g, b));
                }
            }
        }
    }

    public struct PRandomZoneLighting
    {
        /// <summary>
        /// Seed
        /// </summary>
        public int Seed;
    }
}
