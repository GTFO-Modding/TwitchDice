using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SNetwork;
using Steamworks;

namespace TwitchDice
{
    [HarmonyPatch]
    public static class Hooks
    {
        public static event Action OnFail;
        public static event Action OnLobbyStart;
        public static event Action OnLobbyLeave;
        public static event Action<LobbyDataUpdate_t> LobbyDataUpdated;

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
    }
}
