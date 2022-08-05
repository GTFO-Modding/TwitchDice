using UnityEngine;

namespace TwitchDice.Extensions
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject obj)
            where T : Component
        {
            T? comp = obj.GetComponent<T>();
            if (comp == null)
            {
                return obj.AddComponent<T>();
            }
            else
            {
                return comp;
            }
        }
    }
}
