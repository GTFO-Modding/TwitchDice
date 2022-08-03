using LevelGeneration;
using System.Collections.Generic;

namespace TwitchDice.Extensions
{
    public static class DimensionExtensions
    {
        public static IEnumerable<LG_Layer> GetAllLayers(this Dimension dimension)
        {
            for (int index = 0; index < dimension.Layers.Count; index++)
            {
                yield return dimension.Layers[index];
            }
        }

        public static IEnumerable<LG_Zone> GetAllZones(this Dimension dimension)
        {
            foreach (LG_Layer layer in dimension.GetAllLayers())
            {
                foreach (LG_Zone zone in layer.GetAllZones())
                {
                    yield return zone;
                }
            }
        }
    }
}
