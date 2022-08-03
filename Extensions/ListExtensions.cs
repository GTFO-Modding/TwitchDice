using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TwitchDice.Extensions
{
    public static class ListExtensions
    {
        public static int Matches<T>(this IList<T> list, Func<T, bool> matchFN)
        {
            int count = 0;
            foreach (T item in list)
            {
                if (matchFN(item))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> matchFN)
        {
            int count = 0;
            for (int index = 0, length = list.Count; index < length; index++)
            {
                if (matchFN(list[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches<T>(this Il2CppReferenceArray<T> array, Func<T, bool> matchFN)
            where T : Il2CppObjectBase
        {
            int count = 0;
            for (int index = 0, length = array.Length; index < length; index++)
            {
                if (matchFN(array[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches<T>(this Il2CppStructArray<T> array, Func<T, bool> matchFN)
            where T : unmanaged
        {
            int count = 0;
            for (int index = 0, length = array.Length; index < length; index++)
            {
                if (matchFN(array[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches(this Il2CppStringArray array, Func<string, bool> matchFN)
        {
            int count = 0;
            for (int index = 0, length = array.Length; index < length; index++)
            {
                if (matchFN(array[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static Il2CppSystem.Collections.Generic.List<T> Filter<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> filterFN)
        {
            int index = 0;
            while (index < list.Count)
            {
                if (filterFN(list[index]))
                {
                    index++;
                }
                else
                {
                    list.RemoveAt(index);
                }
            }

            return list;
        }

        public static List<T> Filter<T>(this List<T> list, Func<T, bool> filterFN)
        {
            int index = 0;
            while (index < list.Count)
            {
                if (filterFN(list[index]))
                {
                    index++;
                }
                else
                {
                    list.RemoveAt(index);
                }
            }

            return list;
        }

        public static List<T> ToManaged<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            var result = new List<T>();
            for (int index = 0, count = list.Count; index < count; index++)
            {
                result.Add(list[index]);
            }
            return result;
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToUnmanaged<T>(this List<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            for (int index = 0, count = list.Count; index < count; index++)
            {
                result.Add(list[index]);
            }
            return result;
        }

        public static T GetRandomElement<T>(this T[] array)
        {
            return array[Main.rnd.Next(array.Length)];
        }

        public static T GetRandomElement<T>(this IList list)
        {
            return (T)list[Main.rnd.Next(list.Count)];
        }

        public static T GetRandomElement<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            return list[Main.rnd.Next(list.Count)];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Main.rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
