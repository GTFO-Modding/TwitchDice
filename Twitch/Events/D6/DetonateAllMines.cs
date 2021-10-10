using System;

namespace TwitchDice.Twitch.Events.D6
{
    public class DetonateAllMines : DiceEvent
    {
        public override string EventName => "Boom!";

        public override string EventID => "dam";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override void TriggerHost()
        {
            var mines = UnityEngine.Object.FindObjectsOfType<MineDeployerInstance>();
            foreach (var mine in mines)
            {
                mine.m_detonation.TriggerDetonate(5f);
            }
        }
    }
}
