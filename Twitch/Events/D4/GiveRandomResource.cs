using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D4
{
    public class GiveRandomResource : DiceEvent
    {
        public override string EventName => "Random Resource";

        public override string EventID => "ranRes";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player);
            int res = Main.rnd.Next(0, 3);
            switch (res)
            {
                case 0:
                    player.GiveHealth(0.2f);
                    break;
                case 1:
                    player.GiveAmmoRel(0.2f, 0.2f, 0.0f);
                    break;
                case 2:
                    player.GiveAmmoRel(0, 0, 0.2f);
                    break;
                case 3:
                    player.GiveDisinfection(0.2f);
                    break;
            }
        }
    }
}
