using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class AltTab : DiceEvent<alttab>
    {
        public override string EventName => "Alt + Tab";

        public override string EventID => "alttab";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override void ReceiveClient(ulong sender, alttab packet)
        {
            this.TriggerCommon();
        }

        public override void TriggerHost()
        {
            this.TriggerCommon();
            this.TriggerClient();
        }

        private void TriggerCommon()
        {
            Application.OpenURL(this.urls.GetRandomElement());
        }

        private readonly string[] urls = new string[]
        {
            "https://media.discordapp.net/attachments/620576156613345310/901513808797913108/IMG_20210227_081726.jpg?width=1092&height=910",
            "https://cdn.discordapp.com/attachments/620576156613345310/901568419609059328/90a78000d93d44d9ca64de04f0aae91d55d0875624e16aa4e67e364d3374521d_1.png",
            "https://cdn.discordapp.com/attachments/423394643879788557/840454870393159710/woods_sus.png",
            "https://cdn.discordapp.com/attachments/423394643879788557/840454864747102208/dauna_sus_.png",
            "https://cdn.discordapp.com/attachments/423394643879788557/840454867050037288/hackett_sus.png",
            "https://cdn.discordapp.com/attachments/423394643879788557/840454863028617237/bishop_sus.png",
            "https://cdn.discordapp.com/attachments/423394643879788557/831852025670795304/Ey4DRPXWQAQOErL.png",
            "https://media.discordapp.net/attachments/423394643879788557/830918939592097832/unknown.png",
            "https://media.discordapp.net/attachments/423394643879788557/817877763112960020/unknown.png",
            "https://media.discordapp.net/attachments/423394643879788557/810074908645523467/image0-46-3-1.jpg",
            "https://cdn.discordapp.com/attachments/423394643879788557/635393740009373716/farmed.png",
            "https://media.discordapp.net/attachments/782467302464815115/900510038081286195/FBbxTV2XIAIMqVL.png",
            "https://media.discordapp.net/attachments/782467302464815115/899935061862928394/image0-15.jpg"
        };
    }

    public struct alttab
    {

    }
}
