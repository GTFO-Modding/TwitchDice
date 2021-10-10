using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D4
{
    public class ShuffleResources : DiceEvent<ShuffleRes>
    {
        public override string EventName => "Shuffle Resources";

        public override string EventID => "shuffleRes";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override void ReceiveClient(ulong sender, ShuffleRes packet)
        {
            Shuffle();
        }

        public override void TriggerHost()
        {
            Shuffle();
            TriggerClient();
        }

        private void Shuffle()
        {
            PlayerBackpack backpack = PlayerBackpackManager.LocalBackpack;
            float main = backpack.AmmoStorage.StandardAmmo.AmmoInPack;
            float special = backpack.AmmoStorage.SpecialAmmo.AmmoInPack;
            float tool = backpack.AmmoStorage.ClassAmmo.AmmoInPack;

            backpack.AmmoStorage.StandardAmmo.AmmoInPack = special;
            backpack.AmmoStorage.StandardAmmo.AmmoInPack = tool;
            backpack.AmmoStorage.ClassAmmo.AmmoInPack = main;

            backpack.AmmoStorage.UpdateAllAmmoUI();
        }
    }

    public struct ShuffleRes
    {

    }
}
