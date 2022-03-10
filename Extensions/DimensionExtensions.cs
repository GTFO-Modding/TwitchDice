using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;

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
            foreach (var layer in dimension.GetAllLayers())
            {
                foreach (var zone in layer.GetAllZones())
                {
                    yield return zone;
                }
            }
        }
    }
}
