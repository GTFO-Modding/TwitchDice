using AssetShards;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TwitchDice.Utilities
{
    public static class ResourceLoader
    {
        internal static byte[] s_vineBoomSFXBank;

        internal static void Init()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var vineBoomBankStream = assembly.GetManifestResourceStream("TwitchDice.Assets.VineBOOMSFX.bnk"))
            {
                s_vineBoomSFXBank = new byte[vineBoomBankStream.Length];
                vineBoomBankStream.Read(s_vineBoomSFXBank);
            }

            AssetShardManager.add_OnStartupAssetsLoaded((System.Action)OnStartupAssetsLoaded);
        }

        private static void OnStartupAssetsLoaded()
        {
            if (LoadBNK(s_vineBoomSFXBank, out uint _))
            {
                Log.Debug("Successfully loaded Vine Boom Bank");
            }
            else
            {
                Log.Error("Failed to load Vine Boom Bank!");
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

                var result = AkSoundEngine.LoadBank(ptr, size, out bnkID);
                return result == AKRESULT.AK_Success || result == AKRESULT.AK_BankAlreadyLoaded;
            }
            catch (Exception)
            {
                bnkID = 0;
                return false;
            }
        }
    }
}
