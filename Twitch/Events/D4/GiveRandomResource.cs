using AK;
using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D4
{
    public class GiveRandomResource : DiceEvent<GiveRanRes>
    {
        public override string EventName => "Random Resource";
        public override string EventDescription => "Gives a random player a random resource.";
        public override string EventID => "ranRes";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override void ReceiveClient(ulong sender, GiveRanRes packet)
        {
            GiveResource(PlayerUtil.LocalPlayerAgent);
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player);
            if (player.IsLocallyOwned)
            {
                GiveResource(player);
            } else
            {
                TriggerClient(new GiveRanRes(), player.Owner);
            }
        }

        private void GiveResource(PlayerAgent player)
        {
            int res = Main.rnd.Next(0, 4);
            switch (res)
            {
                case 0:
                    player.GiveHealth(player, 0.2f);
                    player.Sound.Post(EVENTS.MEDPACK_APPLY);
                    break;
                case 1:
                    player.GiveAmmoRel(player, 0.2f, 0.2f, 0.0f);
                    player.Sound.Post(EVENTS.FOLEY_USE_AMMO_PACK_FINISHED);
                    player.Sound.Post(EVENTS.AMMOPACK_APPLY);
                    break;
                case 2:
                    player.GiveAmmoRel(player, 0, 0, 0.2f);
                    player.Sound.Post(EVENTS.FOLEY_USE_AMMO_PACK_FINISHED);
                    player.Sound.Post(EVENTS.AMMOPACK_APPLY);
                    break;
                case 3:
                    player.GiveDisinfection(player, 0.2f);
                    player.Sound.Post(EVENTS.DISINFECTION_SPRAY_ON_VISOR);
                    break;
            }
        }
    }

    public struct GiveRanRes
    {

    }
}
