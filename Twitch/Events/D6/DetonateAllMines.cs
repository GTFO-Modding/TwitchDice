using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TwitchDice.Twitch.Events.D6
{
    public class DetonateAllMines : DiceEvent
    {
        public override string EventName => "Boom!";
        public override string EventDescription => "Detonates all mines on the map.";
        public override string EventID => "dam";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override bool CanBeTriggered()
        {
            Il2CppArrayBase<MineDeployerInstance> mines = UnityEngine.Object.FindObjectsOfType<MineDeployerInstance>();
            return mines.Count > 0;
        }

        public override void TriggerHost()
        {
            Il2CppArrayBase<MineDeployerInstance> mines = UnityEngine.Object.FindObjectsOfType<MineDeployerInstance>();
            foreach (MineDeployerInstance mine in mines)
            {
                mine.m_detonation.TriggerDetonate(5f);
            }
        }
    }
}
