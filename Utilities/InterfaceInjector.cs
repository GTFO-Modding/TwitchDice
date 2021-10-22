using System;
using System.Collections.Generic;
using System.Text;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerBaseLib.Runtime.VersionSpecific.Class;
using UnhollowerRuntimeLib;

namespace TwitchDice.Utilities
{
    public static class InterfaceInjector
    {
        public static void InjectWithInterface<T>() where T : class
        {
            unsafe
            {
                var interfaceList = new List<INativeClassStruct>();
                var attributes = typeof(T).GetCustomAttributes(typeof(Il2cppInterfaceAttribute), true);
                
                Log.Debug($"Attr count >>> {typeof(T).GetCustomAttributes(true).Length}");
                foreach (var attribute in attributes)
                {
                    var iType = ((Il2cppInterfaceAttribute)attribute).Type;
                    var genericPointerStore = typeof(Il2CppClassPointerStore<>).MakeGenericType(new Type[] { iType });
                    var fieldInfo = genericPointerStore.GetField("NativeClassPtr");

                    var pointer = (IntPtr)fieldInfo.GetValue(null);
                    var nativeStruct = UnityVersionHandler.Wrap((Il2CppClass*)(void*)pointer);
                    interfaceList.Add(nativeStruct);
                }
                ClassInjector.RegisterTypeInIl2Cpp<T>(interfaceList.ToArray());
            }
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class Il2cppInterfaceAttribute : Attribute
    {
        public Type Type;
        public Il2cppInterfaceAttribute(Type type)
        {
            Type = type;
        }
    }
}
