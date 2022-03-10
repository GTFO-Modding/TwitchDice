using LevelGeneration;
using System;
using System.Collections.Generic;
using TwitchDice.Extensions;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D100
{
    public class PullRandomAlarm : DiceEvent
    {
        public override string EventName => "Pull Random Alarm";

        public override string EventID => "pullalarm";

        protected override DiceTier DiceTier => DiceTier.D100;

        private List<LG_SecurityDoor> m_doors;

        public override bool CanBeTriggered()
        {
            this.m_doors = FetchSecurityDoors();
            return this.m_doors.Count > 0;
        }

        public override void TriggerHost()
        {
            if (this.m_doors == null)
            {
                this.m_doors = FetchSecurityDoors();
            }

            this.m_doors.GetRandomElement<LG_SecurityDoor>()
                .m_sync.AttemptDoorInteraction(eDoorInteractionType.ActivateChainedPuzzle);
        }

        #region Door Fetching

        private static List<LG_SecurityDoor> FetchSecurityDoors()
        {
            var doors = new List<LG_SecurityDoor>();
            foreach (var zone in Builder.CurrentFloor.GetAllZones())
            {
                FetchSecurityDoors(zone, doors);
            }
            return doors;
        }

        private static void FetchSecurityDoors(LG_Zone zone, List<LG_SecurityDoor> doors)
        {
            var door = zone.m_sourceGate?.SpawnedDoor?.TryCast<LG_SecurityDoor>();
            if (IsValidDoor(door) && !doors.Contains(door))
            {
                doors.Add(door);
            }
        }

        private static bool IsValidDoor(LG_SecurityDoor door)
        {
            if (door == null)
                return false;

            var status = door.m_sync.GetCurrentSyncState().status;
            return status == eDoorStatus.Closed_LockedWithChainedPuzzle || status == eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm;
        }

        #endregion
    }
}
