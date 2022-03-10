using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Extensions
{
    public static class LG_LayerExtensions
    {
        public static IEnumerable<LG_Zone> GetAllZones(this LG_Layer layer)
        {
            for (int index = 0, count = layer.m_zones.Count; index < count; index++)
            {
                yield return layer.m_zones[index];
            }
        }
    }
}
