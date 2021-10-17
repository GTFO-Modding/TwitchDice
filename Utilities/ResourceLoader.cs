using AssetShards;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class ResourceLoader
    {
        private static List<(string, byte[])> s_banksToLoad;

        internal static void Init()
        {
            s_banksToLoad = new List<(string, byte[])>();

            InitBankResource("TwitchDice");
            InitAssetBundle("snowman");

            AssetShardManager.add_OnStartupAssetsLoaded((System.Action)OnStartupAssetsLoaded);
        }

        private static void InitBankResource(string bankName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            byte[] result;
            using (var stream = assembly.GetManifestResourceStream($"TwitchDice.Assets.{bankName}.bnk"))
            {
                result = new byte[stream.Length - stream.Position];
                stream.Read(result);
            }

            s_banksToLoad.Add((bankName, result));
        }

        private static void InitAssetBundle(string assetBundleName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            byte[] result;
            using (var stream = assembly.GetManifestResourceStream($"TwitchDice.Assets.Bundle.{assetBundleName}"))
            {
                result = new byte[stream.Length - stream.Position];
                stream.Read(result);
            }
            GTFO.API.AssetAPI.LoadAndRegisterAssetBundle(result);
        }

        private static void OnStartupAssetsLoaded()
        {
            while (s_banksToLoad.Count > 0)
            {
                var bankInfo = s_banksToLoad[0];
                s_banksToLoad.RemoveAt(0);
                if (LoadBNK(bankInfo.Item2, out uint _))
                {
                    Log.Debug($"Successfully loaded {bankInfo.Item1} Bank");
                }
                else
                {
                    Log.Error($"Failed to load {bankInfo.Item1} Bank!");
                }
            }
        }

        private static bool LoadBNK(byte[] bytes, out uint bnkID)
        {
            try
            {
                var size = (uint)bytes.Length;
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var ptr = handle.AddrOfPinnedObject();

                #region Latex's Ass Fix
                if ((ptr.ToInt64() & 15L) != 0L) // gay gay check (I guess) [Re-aligns the bytes by 16 so last 4 bits are 0]
                {
                    byte[] array = new byte[(long)bytes.Length + 16L];
                    IntPtr newPtr = GCHandle.Alloc(array, GCHandleType.Pinned).AddrOfPinnedObject();
                    int offset = 0;
                    if ((newPtr.ToInt64() & 15L) != 0L)
                    {
                        long realignedPointerLoc = (newPtr.ToInt64() + 15L) & -16L; // re-aligns pointer location
                        offset = (int)(realignedPointerLoc - newPtr.ToInt64()); // updates offset
                        newPtr = new IntPtr(realignedPointerLoc); // create new pointer with re-aligned
                    }
                    Array.Copy(bytes, 0, array, offset, bytes.Length);
                    ptr = newPtr;
                    handle.Free(); // free original handle because we have a new one
                }
                #endregion

                switch (AkSoundEngine.LoadBank(ptr, size, out bnkID))
                {
                    case AKRESULT.AK_Success:
                        return true;
                    case AKRESULT.AK_BankAlreadyLoaded:
                        Log.Warning($"Bank with id '{bnkID}' is already loaded");
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                bnkID = 0;
                return false;
            }
        }
    }
}
