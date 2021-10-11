using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace TwitchDice.Utilities
{
    internal static class TimedEvents
    {
        internal static CoroutineHandler.IRoutine StartTimedEvent(IEnumerator routine, string eventName, float time)
        {
            return CoroutineHandler.Add(routine);
        }

        internal static CoroutineHandler.IRoutine Start(IEnumerator routine)
        {
            return CoroutineHandler.Add(routine);
        }
    }
}
