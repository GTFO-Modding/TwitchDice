
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
        
        private static CoroutineHandler.IRoutine _activeForceCrouch; // cache in case another force crouch is called.
        private static CoroutineHandler.IRoutine _activeInvertControls; // cache in case of another force invert controls is called.
        private static CoroutineHandler.IRoutine _activeDisableInteractions;
        private static CoroutineHandler.IRoutine _activeDisableMovement;
        private static CoroutineHandler.IRoutine _activeEnableGlobalPlayerDamage;

        public static void EnableGlobalPlayerDamage(float seconds)
        {
            _activeEnableGlobalPlayerDamage?.Stop();

            _activeEnableGlobalPlayerDamage = TimedEvents.Start(DoEnableGlobalPlayerDamage(seconds));
        }

        internal static void EnableGlobalPlayerDamageEvent(float seconds, string eventName)
        {
            _activeEnableGlobalPlayerDamage?.Stop();

            _activeEnableGlobalPlayerDamage = TimedEvents.StartTimedEvent(DoEnableGlobalPlayerDamage(seconds), eventName, seconds);
        }

        public static void DisableMovementForSeconds(float seconds)
        {
            _activeDisableMovement?.Stop();

            _activeDisableMovement = TimedEvents.Start(DoDisableMovementForSeconds(seconds));
        }

        internal static void DisableMovementForSecondsEvent(float seconds, string eventName)
        {
            _activeDisableMovement?.Stop();

            _activeDisableMovement = TimedEvents.StartTimedEvent(DoDisableMovementForSeconds(seconds), eventName, seconds);
        }

        public static void DisableInteractionsForSeconds(float seconds)
        {
            _activeDisableInteractions?.Stop();

            _activeDisableInteractions = TimedEvents.Start(DoDisableInteractionsForSeconds(seconds));
        }

        internal static void DisableInteractionsForSecondsEvent(float seconds, string eventName)
        {
            _activeDisableInteractions?.Stop();

            _activeDisableInteractions = TimedEvents.StartTimedEvent(DoDisableInteractionsForSeconds(seconds), eventName, seconds);
        }

        public static void InvertControlsForSeconds(float seconds)
        {
            _activeInvertControls?.Stop();

            _activeInvertControls = TimedEvents.Start(DoInvertControlsForSeconds(seconds));
        }

        internal static void InvertControlsForSecondsEvent(float seconds, string eventName)
        {
            _activeInvertControls?.Stop();

            _activeInvertControls = TimedEvents.StartTimedEvent(DoInvertControlsForSeconds(seconds), eventName, seconds);
        }

        public static void ForceCrouchForSeconds(float seconds)
        {
            _activeForceCrouch?.Stop();

            _activeForceCrouch = TimedEvents.Start(DoForceCrouchForSeconds(seconds));
        }

        internal static void ForceCrouchForSecondsEvent(float seconds, string eventName)
        {
            _activeForceCrouch?.Stop();

            _activeForceCrouch = TimedEvents.StartTimedEvent(DoInvertControlsForSeconds(seconds), eventName, seconds);
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
