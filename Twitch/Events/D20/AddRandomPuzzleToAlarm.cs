using ChainedPuzzles;
using GameData;
using LevelGeneration;
using Player;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnhollowerBaseLib;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D20
{
    public class AddRandomPuzzleToAlarm : DiceEvent<ARPTA>
    {
        public override string EventName => "Additional Trouble";

        public override string EventID => "addPuzzle";

        protected override DiceTier DiceTier => DiceTier.D20;

        private List<LG_SecurityDoor> m_fetchedDoors;

        public override bool CanBeTriggered()
        {
            this.m_fetchedDoors = FetchSecurityDoors();
            return this.m_fetchedDoors != null;
        }

        public override void ReceiveClient(ulong sender, ARPTA packet)
        {
            this.TriggerCommon(FetchDoor(packet.m_layer, packet.m_index), packet.m_seed, packet.m_wantedType);
        }

        private void TriggerCommon(LG_SecurityDoor door, int seed, uint wantedType)
        {
            AddToInstance(door.m_locks.ChainedPuzzleToSolve, wantedType, seed);
        }

        public override void TriggerHost()
        {
            int seed = Main.rnd.Next(short.MinValue, short.MaxValue);

            List<uint> ids = new List<uint>();
            foreach (var puzzleComp in ChainedPuzzleManager.Current.m_puzzleComponentPrefabs.Keys)
            {
                ids.Add(puzzleComp);
            }

            uint type = ids.GetRandomElement<uint>();

            var door = this.m_fetchedDoors.GetRandomElement<LG_SecurityDoor>();

            this.TriggerClient(new ARPTA(door, seed, type));
            this.TriggerCommon(door, seed, type);
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
            if (door != null && 
                door.m_locks.ChainedPuzzleToSolve != null // adding chain puzzles aren't allowed 
                && !doors.Contains(door) &&
                door.m_locks.ChainedPuzzleToSolve.Data.TriggerAlarmOnActivate)
            {
                var state = door.m_sync.GetCurrentSyncState().status;

                switch (state)
                {
                    case eDoorStatus.Open:
                        break;
                    default:
                        doors.Add(door);
                        break;
                }
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

        private static void AddToInstance(ChainedPuzzleInstance instance, uint wantedType, int fixedSeed)
        {
            var lastPuzzle = instance.m_chainedPuzzleCores[instance.m_chainedPuzzleCores.Length - 1];

            var lastPuzzleBioscan = lastPuzzle.TryCast<CP_Bioscan_Core>();
            CP_Holopath_Spline spline = null;
            if (lastPuzzleBioscan != null)
            {
                spline = lastPuzzleBioscan.m_spline.TryCast<CP_Holopath_Spline>();
            }
            else
            {
                var lastPuzzleCluster = lastPuzzle.TryCast<CP_Cluster_Core>();
                if (lastPuzzleCluster != null)
                {
                    spline = lastPuzzleCluster.m_spline.TryCast<CP_Holopath_Spline>();
                }
            }

            if (spline == null)
            {
                Log.Error("Failed to fetch spline of last puzzle");
                return;
            }

            Vector3 position = spline.CurvySpline.LastSegment.transform.position;

            GameObject puzzlePrefab = ChainedPuzzleManager.GetPuzzleComponentPrefabForType(wantedType);
            Il2CppSystem.Collections.Generic.List<Vector3> list = null;

            if (puzzlePrefab != null && TryGetPositionsOnRadiusDistancedFromEachother(instance.m_sourceArea, position, 1, instance.Data.WantedDistanceBetweenPuzzleComponents, instance.Data.WantedDistanceBetweenPuzzleComponents, instance.Data.UseRandomPositions, out list, fixedSeed))
            {
                var puzzle = GOUtil.SpawnChildAndGetComp<iChainedPuzzleCore>(puzzlePrefab, list[0], Quaternion.identity, instance.m_parent);

                puzzle.Setup(instance.m_chainedPuzzleCores.Length, instance.Cast<iChainedPuzzleOwner>(), instance.m_sourceArea, true, position, null, instance.Data.TriggerAlarmOnActivate, instance.Data.UseRandomPositions, instance.Data.OnlyShowHUDWhenPlayerIsClose, instance.m_puzzleUID);

                puzzle.add_OnPuzzleDone((System.Action<int>)instance.OnPuzzleDone);
                puzzle.add_Master_OnScanStateChanged((System.Action<float, Il2CppSystem.Collections.Generic.List<PlayerAgent>, int, Il2CppStructArray<bool>>)instance.Master_OnPlayerScanChanged);

                var newArray = new Il2CppReferenceArray<iChainedPuzzleCore>(instance.m_chainedPuzzleCores.Length + 1);
                int index = 0;
                while (index < instance.m_chainedPuzzleCores.Length)
                {
                    newArray[index] = instance.m_chainedPuzzleCores[index];
                    index++;
                }
                newArray[index] = puzzle;
                instance.m_chainedPuzzleCores = newArray;
            }
            else
            {
                Log.Error("Gotten prefab was null, or there are no available nodes in the prefab");
            }
        }

        private static bool TryGetPositionsOnRadiusDistancedFromEachother(LG_Area area, Vector3 sourcePos, int wantedCount, float atRadiusFromSourcePos, float distanceFromEachother, bool useRandomPosition, out Il2CppSystem.Collections.Generic.List<Vector3> positions, int fixedSeed, int maxEval = 0)
        {
            // temp method for now until I figure out how to not have the LG_NodeTools method have a stroke

            var random = new System.Random(fixedSeed);
            positions = new Il2CppSystem.Collections.Generic.List<Vector3>();

            var nodes = area.m_courseNode.m_nodeCluster.m_reachableNodes;

            positions.Add(
                nodes[random.Next(0, nodes.Count - 1)].Position);

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ARPTA
    {
        public eLocalZoneIndex m_index;
        public LG_LayerType m_layer;
        public int m_seed;
        public uint m_wantedType;

        public ARPTA(LG_SecurityDoor door, int seed, uint wantedType)
        {
            this.m_index = door.LinkedToZoneData.LocalIndex;
            this.m_layer = door.m_linksToLayerType;
            this.m_seed = seed;
            this.m_wantedType = wantedType;
        }
    }
}
