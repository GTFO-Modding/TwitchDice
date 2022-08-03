using LevelGeneration;
using System.Collections.Generic;

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
            foreach (Dimension dimension in floor.GetAllDimensions())
            {
                foreach (LG_Layer layer in dimension.GetAllLayers())
                {
                    yield return layer;
                }
            }
        }
        public static IEnumerable<LG_Zone> GetAllZones(this LG_Floor floor)
        {
            foreach (Dimension dimension in floor.GetAllDimensions())
            {
                foreach (LG_Zone zone in dimension.GetAllZones())
                {
                    yield return zone;
                }
            }
        }
    }
}
