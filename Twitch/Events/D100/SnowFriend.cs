using System;
using System.Collections.Generic;
using System.Text;
using GTFO.API;
using TwitchDice.Components;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D100
{
    public class SnowFriend : DiceEvent<sFriend>
    {
        public override string EventName => "a friend :)";

        public override string EventID => "friend";

        protected override DiceTier DiceTier => DiceTier.D100;

        static bool Triggered = false;

        public override bool CanBeTriggered()
        {
            return !Triggered;
        }

        public override void ReceiveClient(ulong sender, sFriend packet)
        {
            TriggerCommon();
        }

        public override void TriggerHost()
        {
            Triggered = true;
            TriggerCommon();
            TriggerClient();
        }

        private void TriggerCommon()
        {
            GameObject snowman = UnityEngine.Object.Instantiate(AssetAPI.GetLoadedAsset("ASSETS/SNOWMAN.PREFAB")?.TryCast<GameObject>());
            snowman.transform.position = PlayerUtil.LocalPlayerAgent.Position;
            snowman.AddComponent<SnowmanAI>();
        }
    }

    public struct sFriend
    {

    }
}
