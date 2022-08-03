using HarmonyLib;
using Player;
using System.Collections;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D6
{
    [HarmonyPatch]
    public class ScreenFlip : DiceEvent<SF>
    {
        public override string EventName => "Australian Vision";
        public override string EventDescription => "Flips a random player's screen upside-down.";
        public override string EventID => "flipScreen";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));

        protected override IDiceEventConfig FetchConfig()
        {
            IDiceEventConfig cfg = base.FetchConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to flip the player's screen", 15);
            return cfg;
        }

        public static bool Active;

        public override void ReceiveClient(ulong sender, SF packet)
        {
            TimedEvents.Start(Flip(this.Time, PlayerUtil.LocalPlayerAgent), this);
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
            if (target.IsLocallyOwned)
            {
                TimedEvents.Start(Flip(this.Time, target), this);
            }
            else
            {
                this.TriggerClient(new SF(), target.Owner);
            }
        }

        private static IEnumerator Flip(int time, PlayerAgent target)
        {
            Active = true;
            GameObject go = target.FPSCamera.m_holder.gameObject;
            go.transform.Rotate(0, 0, 180);
            yield return new WaitForSeconds(time);
            Active = false;
            go.transform.Rotate(0, 0, -180);
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
