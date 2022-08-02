using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TwitchDice.Twitch;
using UnityEngine;

namespace TwitchDice.Utilities
{
    internal static class TimedEvents
    {
        internal static CoroutineHandler.IRoutine Start(IEnumerator routine)
        {
            return CoroutineHandler.Add(routine);
        }

        internal static CoroutineHandler.IRoutine Start(IEnumerator routine, IDiceEvent @event)
        {
            return StartTimedEvent(routine, @event);
        }

        internal static CoroutineHandler.IRoutine StartTimedEvent(IEnumerator routine, IDiceEvent @event)
        {
            if (routine != null)
            {
                EventTimerManager.Instance.AddTimedInstance(@event);
                return CoroutineHandler.Add(routine);
            }
            else
            {
                return null;
            }
        }
    }
}
