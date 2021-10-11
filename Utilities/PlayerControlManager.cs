
using System.Collections;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class PlayerControlManager
    {
        public static bool ForceCrouchEnabled { get; set; }
        public static bool InvertControls { get; set; }
        public static bool DisableInteractions { get; set; }
        public static bool DisableMovement { get; set; }
        public static bool DamageAllPlayers { get; set; }
        
        private static IEnumerator _activeForceCrouch; // cache in case another force crouch is called.
        private static IEnumerator _activeInvertControls; // cache in case of another force invert controls is called.
        private static IEnumerator _activeDisableInteractions;
        private static IEnumerator _activeDisableMovement;
        private static IEnumerator _activeEnableGlobalPlayerDamage;

        public static void EnableGlobalPlayerDamage(float seconds)
        {
            if (_activeEnableGlobalPlayerDamage != null)
            {
                TimedEvents.Stop(_activeEnableGlobalPlayerDamage);
            }

            _activeEnableGlobalPlayerDamage = DoEnableGlobalPlayerDamage(seconds);
            TimedEvents.Start(_activeEnableGlobalPlayerDamage);
        }

        public static void DisableMovementForSeconds(float seconds)
        {
            if (_activeDisableMovement != null)
            {
                TimedEvents.Stop(_activeDisableMovement);
            }

            _activeDisableMovement = DoDisableMovementForSeconds(seconds);
            TimedEvents.Start(_activeDisableMovement);
        }

        public static void DisableInteractionsForSeconds(float seconds)
        {
            if (_activeDisableInteractions != null)
            {
                TimedEvents.Stop(_activeDisableInteractions);
            }

            _activeDisableInteractions = DoDisableInteractionsForSeconds(seconds);
            TimedEvents.Start(_activeDisableInteractions);
        }

        public static void InvertControlsForSeconds(float seconds)
        {
            if (_activeInvertControls != null)
            {
                TimedEvents.Stop(_activeInvertControls);
            }

            _activeInvertControls = DoInvertControlsForSeconds(seconds);
            TimedEvents.Start(_activeInvertControls);
        }

        public static void ForceCrouchForSeconds(float seconds)
        {
            if (_activeForceCrouch != null)
            {
                TimedEvents.Stop(_activeForceCrouch);
            }

            _activeForceCrouch = DoForceCrouchForSeconds(seconds);
            TimedEvents.Start(_activeForceCrouch);
        }

        private static IEnumerator DoEnableGlobalPlayerDamage(float seconds)
        {
            DamageAllPlayers = true;
            yield return new WaitForSeconds(seconds);
            DamageAllPlayers = false;
            _activeEnableGlobalPlayerDamage = null;
        }

        private static IEnumerator DoDisableMovementForSeconds(float seconds)
        {
            DisableMovement = true;
            yield return new WaitForSeconds(seconds);
            DisableMovement = false;
            _activeDisableInteractions = null;
        }

        private static IEnumerator DoDisableInteractionsForSeconds(float seconds)
        {
            DisableInteractions = true;
            yield return new WaitForSeconds(seconds);
            DisableInteractions = false;
            _activeDisableInteractions = null;
        }

        private static IEnumerator DoInvertControlsForSeconds(float seconds)
        {
            InvertControls = true;
            yield return new WaitForSeconds(seconds);
            InvertControls = false;
            _activeInvertControls = null;
        }

        private static IEnumerator DoForceCrouchForSeconds(float seconds)
        {
            ForceCrouchEnabled = true;
            yield return new WaitForSeconds(seconds);
            ForceCrouchEnabled = false;
            _activeForceCrouch = null;
        }
    }
}
