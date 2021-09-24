using Player;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events
{
    //[RegisterEvent("Randomize Zone Lighting", "randomZL", DiceTier.D3)]
    public class RandomZoneLighting : DiceEvent<RandomZLPacket>
    {
        public override string EventName => "Randomize Room Lighting";

        public override string EventID => "randomZL";

        protected override DiceTier DiceTier => DiceTier.D3;

        public override void ReceiveClient(ulong sender, RandomZLPacket packet)
        {
            RandomizeZL(packet.Seed);
        }

        public override void TriggerHost()
        {
            int seed = Main.rnd.Next(0, 255);
            RandomizeZL(seed);
            TriggerClient(new RandomZLPacket(seed));
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RandomZLPacket
    {
        public int Seed;

        public RandomZLPacket(int seed)
        {
            Seed = seed;
        }
    }

    //OLD

    /*
    public class OldRandomizeZoneLighting : global::DiceEvent<PRandomZoneLighting>
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
    */
}
