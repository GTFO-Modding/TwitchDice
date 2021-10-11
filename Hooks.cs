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
        public static event Action InventorySlotsUpdated;

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

        [HarmonyPatch(typeof(PlayerLocomotion), nameof(PlayerLocomotion.RunInput))]
        [HarmonyPrefix]
        public static bool RunInput(PlayerAgent owner, ref bool __result)
        {
            if (owner?.IsLocallyOwned ?? false)
            {
                if (PlayerControlManager.DisableRunning)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(PlayerLocomotion), nameof(PlayerLocomotion.CrouchInput))]
        [HarmonyPrefix]
        public static bool CrouchInput(PlayerAgent owner, ref bool __result)
        {
            if (owner?.IsLocallyOwned ?? false)
            {
                if (PlayerControlManager.ForceCrouchEnabled)
                {
                    __result = true;
                    return false;
                }
                else if (PlayerControlManager.DisableCrouching)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Dam_PlayerDamageBase), nameof(Dam_PlayerDamageBase.OnIncomingDamage))]
        [HarmonyPrefix]
        public static void OnIncomingDamage(Dam_PlayerDamageBase __instance, ref float damage)
        {
            if (PlayerControlManager.DamageAllPlayers)
            {
                if (__instance.Owner.IsLocallyOwned && damage > 0)
                {
                    foreach (var player in PlayerManager.PlayerAgentsInLevel)
                    {
                        if (!player.IsLocallyOwned)
                        {
                            player.Damage.NoAirDamage(-damage);
                        }
                    }
                }
                else
                {
                    damage = -damage;
                }
            }
        }

        [HarmonyPatch(typeof(InputMapper), nameof(InputMapper.GetAxisKeyMouseGamepad))]
        [HarmonyPostfix]
        public static void GetAxisKeyMouseGamepad(InputAction action, ref float __result)
        {
            if (PlayerControlManager.InvertControls)
            {
                __result = -__result;
            }

            if (PlayerControlManager.DisableMovement)
            {
                switch (action)
                {
                    case InputAction.MoveHorizontal:
                    case InputAction.MoveVertical:
                        __result = 0;
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(InputMapper), nameof(InputMapper.GetButtonDownKeyMouseGamepad))]
        [HarmonyPostfix]
        public static void GetButtonDownKeyMouseGamepad(InputAction action, ref bool __result)
        {
            if (PlayerControlManager.DisableMovement)
            {
                switch (action)
                {
                    case InputAction.Jump:
                    case InputAction.Crouch:
                        __result = false;
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(InputMapper), nameof(InputMapper.GetButtonKeyMouseGamepad))]
        [HarmonyPostfix]
        public static void GetButtonKeyMouseGamepad(InputAction action, ref bool __result)
        {
            if (PlayerControlManager.DisableMovement)
            {
                switch (action)
                {
                    case InputAction.Jump:
                    case InputAction.Crouch:
                        __result = false;
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(InputMapper), nameof(InputMapper.GetButtonUpKeyMouseGamepad))]
        [HarmonyPostfix]
        public static void GetButtonUpKeyMouseGamepad(InputAction action, ref bool __result)
        {
            if (PlayerControlManager.DisableMovement)
            {
                switch (action)
                {
                    case InputAction.Jump:
                    case InputAction.Crouch:
                        __result = false;
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerInteraction), nameof(PlayerInteraction.UpdateWorldInteractions))]
        [HarmonyPrefix]
        public static bool UpdateWorldInteractions(PlayerInteraction __instance)
        {
            if (PlayerControlManager.DisableInteractions)
            {
                __instance.UnSelectCurrentBestInteraction();
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(PUI_Inventory), nameof(PUI_Inventory.UpdateSlotPositions))]
        [HarmonyPostfix]
        public static void UpdateSlotPositionsPost()
        {
            InventorySlotsUpdated?.Invoke();
        }
    }
}
