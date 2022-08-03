using LevelGeneration;
using Player;
using System.Collections.Generic;
using TwitchDice.Extensions;

namespace TwitchDice.Twitch.Events.D6
{
    public class BreakDoorNearPlayer : DiceEvent
    {
        public override string EventName => "Door Machine Broke";
        public override string EventDescription => "Breaks a random door near the player.";
        public override string EventID => "doorbroke";

        protected override DiceTier DiceTier => DiceTier.D6;

        private readonly List<PlayerAgent> m_players = new();

        public override bool CanBeTriggered()
        {
            this.m_players.Clear();
            foreach (PlayerAgent player in PlayerManager.PlayerAgentsInLevel)
            {
                foreach (LG_Gate gate in player.CourseNode.m_area.m_gates)
                {
                    LG_WeakDoor? door = gate.SpawnedDoor?.TryCast<LG_WeakDoor>();
                    if (door != null && door.m_sync.GetCurrentSyncState().status != eDoorStatus.Destroyed)
                    {
                        this.m_players.Add(player);
                        break;
                    }
                }
            }

            return this.m_players.Count > 0;
        }

        public override void TriggerHost()
        {
            PlayerAgent? player = this.m_players?.GetRandomElement<PlayerAgent>();
            if (player != null)
            {
                var weakDoors = new List<LG_WeakDoor>();

                foreach (LG_Gate gate in player.CourseNode.m_area.m_gates)
                {
                    LG_WeakDoor? door = gate.SpawnedDoor?.TryCast<LG_WeakDoor>();
                    if (door != null && door.m_sync.GetCurrentSyncState().status != eDoorStatus.Destroyed)
                    {
                        weakDoors.Add(door);
                    }
                }

                LG_WeakDoor weakDoor = weakDoors.GetRandomElement<LG_WeakDoor>();
                weakDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.DoDamage, 100000f, 10000f);
            }
        }
    }
}
