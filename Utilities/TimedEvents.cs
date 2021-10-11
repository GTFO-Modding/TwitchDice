using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TwitchDice.Twitch;
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

        private static readonly List<IEnumerator> tempList = new List<IEnumerator>();

        internal static object StartTimedEvent(IEnumerator routine, IDiceEvent @event)
        {
            if (routine != null)
            {
                ProcessNextOfCoroutine(routine);
                EventTimerManager.Instance.AddTimedInstance(@event);
            }
            return routine;
        }

        internal static object Start(IEnumerator routine)
        {
            if (routine != null) ProcessNextOfCoroutine(routine);
            return routine;
        }

        internal static void Stop(IEnumerator enumerator)
        {
            if (ourNextFrameCoroutines.Contains(enumerator)) // the coroutine is running itself
                ourNextFrameCoroutines.Remove(enumerator);
            else
            {
                int coroTupleIndex = ourCoroutinesStore.FindIndex(c => c.Coroutine == enumerator);
                if (coroTupleIndex != -1) // the coroutine is waiting for a subroutine
                {
                    object waitCondition = ourCoroutinesStore[coroTupleIndex].WaitCondition;
                    if (waitCondition is IEnumerator waitEnumerator)
                        Stop(waitEnumerator);

                    ourCoroutinesStore.RemoveAt(coroTupleIndex);
                }
            }
        }

        private static void ProcessCoroList(List<IEnumerator> target)
        {
            if (target.Count == 0) return;

            // use a temp list to make sure waits made during processing are not handled by same processing invocation
            // additionally, a temp list reduces allocations compared to an array
            tempList.AddRange(target);
            target.Clear();
            foreach (var enumerator in tempList) ProcessNextOfCoroutine(enumerator);
            tempList.Clear();
        }

        internal static void Process()
        {
            return CoroutineHandler.Add(routine);
        }
    }
}
