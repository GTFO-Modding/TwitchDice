using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Player;
using SNetwork;
using Steamworks;
using TwitchDice.Utilities;

namespace TwitchDice
{
    [HarmonyPatch]
    public static class Hooks
    {
        public static event Action OnFail;
        public static event Action OnLobbyStart;
        public static event Action OnLobbyLeave;
        public static event Action<LobbyDataUpdate_t> LobbyDataUpdated;
        public static event Action Cleanup;

        [HarmonyPatch(typeof(GS_ExpeditionFail), "Enter")]
        [HarmonyPostfix]
        public static void GS_ExpeditionFail_Enter()
        {
            OnFail?.Invoke();
        }

        [HarmonyPatch(typeof(SNet_SessionHub), "OnJoinedLobby")]
        [HarmonyPostfix]
        public static void SNet_SessionHub_OnJoinedLobby()
        {
            OnLobbyStart?.Invoke();
        }

        [HarmonyPatch(typeof(GS_NoLobby), "Enter")]
        [HarmonyPostfix]
        public static void GS_NoLobby_Enter()
        {
            OnLobbyLeave?.Invoke();
        }

        [HarmonyPatch(typeof(SNet_Core_STEAM), "OnLobbyDataUpdate")]
        [HarmonyPrefix]
        public static void SNet_Core_STEAM_OnLobbyDataUpdate(LobbyDataUpdate_t data)
        {
            Log.Debug("Got lobby data update");
            if (data.m_bSuccess == 0) return;
            LobbyDataUpdated?.Invoke(data);
        }

        [HarmonyPatch(typeof(GS_AfterLevel), "CleanupAfterExpedition")]
        [HarmonyPrefix]
        public static void GS_AfterLevel()
        {
            Cleanup?.Invoke();
        }

        [HarmonyPatch(typeof(PlayerLocomotion), nameof(PlayerLocomotion.CrouchInput))]
        [HarmonyPrefix]
        public static bool CrouchInput(PlayerAgent owner, ref bool __result)
        {
            if ((owner?.IsLocallyOwned ?? false) && PlayerControlManager.ForceCrouchEnabled)
            {
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(InputMapper), nameof(InputMapper.GetAxisKeyMouseGamepad))]
        [HarmonyPostfix]
        public static void GetAxisKeyMouseGamepad(ref float __result)
        {
            if (PlayerControlManager.InvertControls)
            {
                __result = -__result;
            }
        }
    }
}
