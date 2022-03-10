using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice.Extensions
{
    public static class LG_FloorExtensions
    {
        public static IEnumerable<Dimension> GetAllDimensions(this LG_Floor floor)
        {
            for (int index = 0, count = floor.m_dimensions.Count; index < count; index++)
            {
                yield return floor.m_dimensions[index];
            }
        }
        public static IEnumerable<LG_Layer> GetAllLayers(this LG_Floor floor)
        {
            foreach (var dimension in floor.GetAllDimensions())
            {
                foreach (var layer in dimension.GetAllLayers())
                {
                    yield return layer;
                }
            }
        }
        public static IEnumerable<LG_Zone> GetAllZones(this LG_Floor floor)
        {
            foreach (var dimension in floor.GetAllDimensions())
            {
                foreach (var zone in dimension.GetAllZones())
                {
                    yield return zone;
                }
            }
        }
    }
}
