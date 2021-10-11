using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D50
{
    public class RespawnAllEnemies : DiceEvent
    {
        public override string EventName => "Back from the Dead!";

        public override string EventID => "respawnAll";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override void TriggerHost()
        {
            EnemyRespawnManager.RespawnAll();
        }
    }
}
