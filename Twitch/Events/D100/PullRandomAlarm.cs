using LevelGeneration;
using System;
using System.Collections.Generic;
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
            return FetchSecurityDoors(new List<LG_SecurityDoor>());
        }

        private static List<LG_SecurityDoor> FetchSecurityDoors(List<LG_SecurityDoor> doors)
        {
            FetchSecurityDoors(Builder.CurrentFloor, doors);
            return doors;
        }

        private static void FetchSecurityDoors(LG_Floor floor, List<LG_SecurityDoor> doors)
        {
            for (int index = 0, length = floor.m_layers.Count; index < length; index++)
            {
                FetchSecurityDoors(floor.m_layers[index], doors);
            }
        }

        private static void FetchSecurityDoors(LG_Layer layer, List<LG_SecurityDoor> doors)
        {
            for (int index = 0, length = layer.m_zones.Count; index < length; index++)
            {
                FetchSecurityDoors(layer.m_zones[index], doors);
            }
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
