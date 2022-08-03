using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D8
{
    public class InfectAPlayer : DiceEvent
    {
        public override string EventName => "Rand-o-Infect";
        public override string EventDescription => "Infects a random player.";
        public override string EventID => "infectaplayer";

        protected override DiceTier DiceTier => DiceTier.D8;

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player))
            {
                player.Damage.ModifyInfection(new pInfection()
                {
                    amount = 0.05f,
                    mode = pInfectionMode.Add
                }, true, true);
            }
        }
    }
}
