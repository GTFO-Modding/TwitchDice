using LevelGeneration;
using Player;
using System;
using System.Collections.Generic;
using TwitchDice.Extensions;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D6
{
    public class BreakDoorNearPlayer : DiceEvent
    {
        public override string EventName => "Door Machine Broke";

        public override string EventID => "doorbroke";

        protected override DiceTier DiceTier => DiceTier.D6;

        private List<PlayerAgent> m_players;

        public override bool CanBeTriggered()
        {
            this.m_players = new List<PlayerAgent>();
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                foreach (var gate in player.CourseNode.m_area.m_gates)
                {
                    var door = gate.SpawnedDoor?.TryCast<LG_WeakDoor>();
                    if (door && door.m_sync.GetCurrentSyncState().status != eDoorStatus.Destroyed)
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
            var player = this.m_players?.GetRandomElement<PlayerAgent>();
            if (player)
            {
                var weakDoors = new List<LG_WeakDoor>();

                foreach (var gate in player.CourseNode.m_area.m_gates)
                {
                    var door = gate.SpawnedDoor?.TryCast<LG_WeakDoor>();
                    if (door && door.m_sync.GetCurrentSyncState().status != eDoorStatus.Destroyed)
                    {
                        weakDoors.Add(door);
                    }
                }

                var weakDoor = weakDoors.GetRandomElement<LG_WeakDoor>();
                weakDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.DoDamage, 100000f, 10000f);
            }
        }
    }
}
