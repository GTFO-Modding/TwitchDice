
using Agents;
using Player;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D50
{
    public class DownReviveRandomPlayer : DiceEvent
    {
        public override string EventName => "Bone Hurting Juice";

        public override string EventID => "bhj";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override void TriggerHost()
        {
            if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target))
            {
                if (target.Alive)
                {
                    target.Damage.NoAirDamage(float.PositiveInfinity);
                }
                else
                {
                    AgentReplicatedActions.PlayerReviveAction(target, PlayerManager.GetLocalPlayerAgent(), target.Position);
                }
            }
        }
    }
}
