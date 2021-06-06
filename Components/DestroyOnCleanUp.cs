using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TwitchDice.Components
{
    public class DestroyOnCleanUp : MonoBehaviour
    {
        public DestroyOnCleanUp(IntPtr intPtr) : base(intPtr) { }

        void Awake()
        {
            Hooks.Cleanup += Hooks_Cleanup;
        }

        private void Hooks_Cleanup()
        {
            Destroy(this);
        }
    }
}
