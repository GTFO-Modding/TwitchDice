

using GameData;
using LevelGeneration;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class BloodifySecurityDoor : DiceEvent<BSD>
    {
        public override string EventName => "Bloody Door";

        public override string EventID => "bloodifydoor";

        protected override DiceTier DiceTier => DiceTier.D50;

        private List<LG_SecurityDoor> m_fetchedDoors;
        private List<EnemyGroupDataBlock> m_groupBlocks;

        public override bool CanBeTriggered()
        {
            this.m_fetchedDoors = FetchSecurityDoors();
            this.m_groupBlocks = new List<EnemyGroupDataBlock>(EnemyGroupDataBlock.GetAllBlocks())
                .Filter((block) => block.Type == eEnemyGroupType.Hunter);
            return this.m_fetchedDoors.Count > 0 && this.m_groupBlocks.Count > 0;
        }

        public override void ReceiveClient(ulong sender, BSD packet)
        {
            FetchDoor(packet.layer, packet.zone)?.SetupActiveEnemyWaveData(new ActiveEnemyWaveData()
            {
                HasActiveEnemyWave = true,
                EnemyGroupInfrontOfDoor = packet.EnemyGroupInfrontOfDoor,
                EnemyGroupInArea = packet.EnemyGroupInArea,
                EnemyGroupsInArea = packet.EnemyGroupsInArea
            });
        }

        public override void TriggerHost()
        {
            if (this.m_fetchedDoors == null)
            {
                this.m_fetchedDoors = FetchSecurityDoors();
            }

            if (this.m_groupBlocks == null)
            {
                this.m_groupBlocks = new List<EnemyGroupDataBlock>(EnemyGroupDataBlock.GetAllBlocks())
                    .Filter((block) => block.Type == eEnemyGroupType.Hunter);
            }

            var randomDoor = this.m_fetchedDoors.GetRandomElement<LG_SecurityDoor>();
            this.GetRandomActiveWaveInfo(out uint enemyGroup, out uint enemyGroupInArea, out int enemyGroupInAreaCount);

            this.TriggerClient(new BSD(randomDoor, enemyGroup, enemyGroupInArea, enemyGroupInAreaCount));

            randomDoor.SetupActiveEnemyWaveData(new ActiveEnemyWaveData()
            {
                HasActiveEnemyWave = true,
                EnemyGroupInfrontOfDoor = enemyGroup,
                EnemyGroupInArea = enemyGroupInArea,
                EnemyGroupsInArea = enemyGroupInAreaCount
            });
        }

        private void GetRandomActiveWaveInfo(out uint enemyGroup, out uint enemyGroupInArea, out int enemyGroupInAreaCount)
        {
            enemyGroup = this.m_groupBlocks.GetRandomElement<EnemyGroupDataBlock>().persistentID;

            if (Main.rnd.NextDouble() < 0.1) // 10% chance
            {
                enemyGroupInArea = this.m_groupBlocks.GetRandomElement<EnemyGroupDataBlock>().persistentID;
                enemyGroupInAreaCount = Main.rnd.Next(1, 3);
            }
            else
            {
                enemyGroupInArea = 0U;
                enemyGroupInAreaCount = 0;
            }
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
            if (door != null && door.ActiveEnemyWaveData == null && door.m_sync.GetCurrentSyncState().status != eDoorStatus.Open && !doors.Contains(door))
            {
                doors.Add(door);
            }
        }

        private static LG_SecurityDoor FetchDoor(LG_LayerType layer, eLocalZoneIndex index)
        {
            return FetchDoor(Builder.CurrentFloor, layer, index);
        }

        private static LG_SecurityDoor FetchDoor(LG_Floor floor, LG_LayerType layer, eLocalZoneIndex index)
        {
            foreach (var lgLayer in floor.GetAllLayers())
            {
                var result = FetchDoor(lgLayer, layer, index);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static LG_SecurityDoor FetchDoor(LG_Layer lgLayer, LG_LayerType layer, eLocalZoneIndex index)
        {
            foreach (var lgZone in lgLayer.m_zones)
            {
                var result = FetchDoor(lgZone, layer, index);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static LG_SecurityDoor FetchDoor(LG_Zone lgZone, LG_LayerType layer, eLocalZoneIndex index)
        {
            var door = lgZone.m_sourceGate?.SpawnedDoor?.TryCast<LG_SecurityDoor>();
            if (door != null && door.LinksToLayerType == layer && door.LinkedToZoneData.LocalIndex == index)
            {
                return door;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BSD
    {
        public LG_LayerType layer;
        public eLocalZoneIndex zone;
        public uint EnemyGroupInfrontOfDoor;
        public uint EnemyGroupInArea;
        public int EnemyGroupsInArea;

        public BSD(LG_SecurityDoor door, uint enemyGroupInfrontOfDoor, uint enemyGroupInArea, int enemyGroupsInArea)
        {
            this.EnemyGroupInfrontOfDoor = enemyGroupInfrontOfDoor;
            this.EnemyGroupInArea = enemyGroupInArea;
            this.EnemyGroupsInArea = enemyGroupsInArea;
            this.zone = door.LinkedToZoneData.LocalIndex;
            this.layer = door.m_linksToLayerType;
        }
    }
}
