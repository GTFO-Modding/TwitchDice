using System;
using UnityEngine;

namespace TwitchDice.Components
{
    public class DestroyOnCleanUp : MonoBehaviour
    {
        public DestroyOnCleanUp(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            Hooks.Cleanup += Hooks_Cleanup;
        }

        private void Hooks_Cleanup()
        {
            Destroy(this);
        }
    }
}
