using LevelGeneration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TwitchDice.Extensions;

namespace TwitchDice.Twitch.Events.D100
{
    public class PullRandomAlarm : DiceEventWithConfig<PullRandomAlarm.RundownConfig>
    {
        public override string EventName => "Pull Random Alarm";
        public override string EventDescription => "Triggers a random door alarm on the level.";
        public override string EventID => "pullalarm";

        protected override DiceTier DiceTier => DiceTier.D100;

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public List<uint> ExcludedPuzzles { get; set; } = new();

            protected override void InitImpl(IDiceEvent diceEvent)
            {
                if (this.ExcludedPuzzles == null)
                {
                    this.ExcludedPuzzles = new();
                }
            }

            public bool Excludes(uint id)
            {
                return this.ExcludedPuzzles?.Contains(id) ?? false;
            }
        }

        private readonly List<LG_SecurityDoor> m_doors = new();

        public override bool CanBeTriggered()
        {
            this.FetchSecurityDoors();
            return this.m_doors.Count > 0;
        }

        public override void TriggerHost()
        {
            this.FetchSecurityDoors();

            this.m_doors.GetRandomElement<LG_SecurityDoor>()
                .m_sync.AttemptDoorInteraction(eDoorInteractionType.ActivateChainedPuzzle);
        }

        #region Door Fetching

        private void FetchSecurityDoors()
        {
            this.m_doors.Clear();
            foreach (LG_Zone zone in Builder.CurrentFloor.GetAllZones())
            {
                this.FetchSecurityDoors(zone);
            }
        }

        private void FetchSecurityDoors(LG_Zone zone)
        {
            LG_SecurityDoor? door = zone.m_sourceGate?.SpawnedDoor?.TryCast<LG_SecurityDoor>();
            if (!IsValidDoor(door) || this.m_doors.Contains(door))
            {
                return;
            }

            if (this.RundownCfg.Excludes(door.m_locks.Cast<LG_SecurityDoor_Locks>().ChainedPuzzleToSolve.Data.persistentID))
            {
                return;
            }

            this.m_doors.Add(door);
        }

        private static bool IsValidDoor([NotNullWhen(true)] LG_SecurityDoor? door)
        {
            if (door == null)
            {
                return false;
            }

            eDoorStatus status = door.m_sync.GetCurrentSyncState().status;
            return status == eDoorStatus.Closed_LockedWithChainedPuzzle || status == eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm;
        }

        #endregion
    }
}
