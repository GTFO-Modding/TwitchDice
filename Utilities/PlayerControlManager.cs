
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
        public static bool DisableRunning { get; set; }
        public static bool DisableCrouching { get; set; }
        public static bool DoSelfDamage { get; set; }
        public static bool InstantKill { get; set; }
        public static bool DisableFire { get; set; }

        private static CoroutineHandler.IRoutine _activeForceCrouch; // cache in case another force crouch is called.
        private static CoroutineHandler.IRoutine _activeInvertControls; // cache in case of another force invert controls is called.
        private static CoroutineHandler.IRoutine _activeDisableInteractions;
        private static CoroutineHandler.IRoutine _activeDisableMovement;
        private static CoroutineHandler.IRoutine _activeDisableFire;
        private static CoroutineHandler.IRoutine _activeEnableGlobalPlayerDamage;
        private static CoroutineHandler.IRoutine _activeDisableRunning;
        private static CoroutineHandler.IRoutine _activeNoCrouch;
        private static CoroutineHandler.IRoutine _activeDoSelfDamage;
        private static CoroutineHandler.IRoutine _activeInstantKill;

        public static void EnableInstantKillForSeconds(float seconds)
        {
            _activeInstantKill?.Stop();

            _activeInstantKill = TimedEvents.Start(DoEnableInstantKillForSeconds(seconds));
        }

        public static void EnableSelfDamageForSeconds(float seconds)
        {
            _activeDoSelfDamage?.Stop();

            _activeDoSelfDamage = TimedEvents.Start(DoEnableSelfDamageForSeconds(seconds));
        }

        public static void DisableCrouchingForSeconds(float seconds)
        {
            _activeNoCrouch?.Stop();

            _activeNoCrouch = TimedEvents.Start(DoDisableCrouchingForSeconds(seconds));
        }

        public static void DisableRunningForSeconds(float seconds)
        {
            _activeDisableRunning?.Stop();

            _activeDisableRunning = TimedEvents.Start(DoDisableRunningForSeconds(seconds));
        }

        public static void EnableGlobalPlayerDamageForSeconds(float seconds)
        {
            _activeEnableGlobalPlayerDamage?.Stop();

            _activeEnableGlobalPlayerDamage = TimedEvents.Start(DoEnableGlobalPlayerDamageForSeconds(seconds));
        }

        public static void DisableMovementForSeconds(float seconds)
        {
            _activeDisableMovement?.Stop();

            _activeDisableMovement = TimedEvents.Start(DoDisableMovementForSeconds(seconds));
        }

        public static void DisableInteractionsForSeconds(float seconds)
        {
            _activeDisableInteractions?.Stop();

            _activeDisableInteractions = TimedEvents.Start(DoDisableInteractionsForSeconds(seconds));;
        }

        public static void InvertControlsForSeconds(float seconds)
        {
            _activeInvertControls?.Stop();

            _activeInvertControls = TimedEvents.Start(DoInvertControlsForSeconds(seconds));
        }

        public static void ForceCrouchForSeconds(float seconds)
        {
            _activeForceCrouch?.Stop();

            _activeForceCrouch = TimedEvents.Start(DoForceCrouchForSeconds(seconds));
        }

        public static void DisableFireForSeconds(float seconds)
        {
            _activeDisableFire?.Stop();

            _activeDisableFire = TimedEvents.Start(DoDisableFireForSeconds(seconds));
        }

        private static IEnumerator DoEnableSelfDamageForSeconds(float seconds)
        {
            DoSelfDamage = true;
            yield return new WaitForSeconds(seconds);
            DoSelfDamage = false;
            _activeDoSelfDamage = null;
        }

        private static IEnumerator DoEnableInstantKillForSeconds(float seconds)
        {
            InstantKill = true;
            yield return new WaitForSeconds(seconds);
            InstantKill = false;
            _activeInstantKill = null;
        }

        private static IEnumerator DoDisableCrouchingForSeconds(float seconds)
        {
            DisableCrouching = true;
            yield return new WaitForSeconds(seconds);
            DisableCrouching = false;
            _activeNoCrouch = null;
        }

        private static IEnumerator DoDisableRunningForSeconds(float seconds)
        {
            DisableRunning = true;
            yield return new WaitForSeconds(seconds);
            DisableRunning = false;
            _activeDisableRunning = null;
        }

        private static IEnumerator DoEnableGlobalPlayerDamageForSeconds(float seconds)
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

        private static IEnumerator DoDisableFireForSeconds(float seconds)
        {
            DisableFire = true;
            yield return new WaitForSeconds(seconds);
            DisableFire = false;
            _activeDisableFire = null;
        }
    }
}
