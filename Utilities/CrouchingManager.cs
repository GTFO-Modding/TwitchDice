
using System.Collections;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class CrouchingManager
    {
        public static bool ForceCrouchEnabled { get; set; }
        private static IEnumerator _activeForceCrouch; // cache in case another force crouch is called.

        public static void ForceCrouchForSeconds(float seconds)
        {
            if (_activeForceCrouch != null)
            {
                TimedEvents.Stop(_activeForceCrouch);
            }

            _activeForceCrouch = DoForceCrouchForSeconds(seconds);
            TimedEvents.Start(_activeForceCrouch);
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
