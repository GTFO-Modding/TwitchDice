using HarmonyLib;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D6
{
    [HarmonyPatch]
    public class ScreenFlip : DiceEvent<SF>
    {
        public override string EventName => "Australian Vision";

        public override string EventID => "flipScreen";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => 15;

        public static bool Active;

        public override void ReceiveClient(ulong sender, SF packet)
        {
            TimedEvents.Start(Flip(Time, PlayerUtil.LocalPlayerAgent), this);
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
            if (target.IsLocallyOwned)
            {
                TimedEvents.Start(Flip(Time, target), this);
            } else
            {
                TriggerClient(new SF(), target.Owner);
            }
        }

        private IEnumerator Flip(int time, PlayerAgent target)
        {
            Active = true;
            var go = target.FPSCamera.m_holder.gameObject;
            go.transform.Rotate(0, 0, 180);
            yield return new WaitForSeconds(time);
            Active = false;
            target.FPSCamera.m_holder.gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
            yield break;
        }

        [HarmonyPatch(typeof(FPSCamera), nameof(FPSCamera.ResetHolderRotation))]
        [HarmonyPrefix]
        public static bool ResetHolderRotation()
        {
            return !Active;
        }
    }

    public struct SF
    {

    }
}
