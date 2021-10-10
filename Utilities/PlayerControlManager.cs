
using System.Collections;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class PlayerControlManager
    {
        public static bool ForceCrouchEnabled { get; set; }
        public static bool InvertControls { get; set; }
        
        private static IEnumerator _activeForceCrouch; // cache in case another force crouch is called.
        private static IEnumerator _activeInvertControls; // cache in case of another force invert controls is called.

        public static void ForceInvertControlsForSeconds(float seconds)
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
