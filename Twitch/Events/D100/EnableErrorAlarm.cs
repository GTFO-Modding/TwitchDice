using System.Collections.Generic;
using AIGraph;
using GameData;
using Player;
using TwitchDice.Extensions;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D100
{
#warning TODO: Add config for alarm restrictions.
    public class EnableErrorAlarm : DiceEvent
    {
        public override string EventName => "Alarm Malfunction";

        public override string EventDescription => "Activates a random error alarm.";

        public override string EventID => "errorAlarm";

        protected override DiceTier DiceTier => DiceTier.D100;

        public override void TriggerHost()
        {
            var errorAlarms = new List<ChainedPuzzleDataBlock>();
            foreach (ChainedPuzzleDataBlock? chainedPuzzle in ChainedPuzzleDataBlock.GetAllBlocks())
            {
                if (!chainedPuzzle.DisableSurvivalWaveOnComplete)
                {
                    errorAlarms.Add(chainedPuzzle);
                }
            }

            Log.Debug($"Found {errorAlarms.Count} alarms to chose from...");

            ChainedPuzzleDataBlock alarm = errorAlarms.GetRandomElement<ChainedPuzzleDataBlock>();

            Log.Debug($"Chose alarm '{alarm.name}' with persistenID {alarm.persistentID}");

            PlayerAgent localPlayer = PlayerManager.GetLocalPlayerAgent();
            AIG_CourseNode node = localPlayer.CourseNode;
            Vector3 pos = node.GetRandomPositionInside();

            Mastermind.Current.TriggerSurvivalWave(
                localPlayer.CourseNode,
                alarm.SurvivalWaveSettings,
                alarm.SurvivalWavePopulation,
                out _,
                spawnType: SurvivalWaveSpawnType.InRelationToClosestAlivePlayer,
                spawnDelay: 5,
                playScreamOnSpawn: true);
        }
    }

    /*public class EnableErrorAlarm : global::DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Trigger Error Alarm";

        public override string EventId => "d100_errorAlarm";

        public override DiceTier Tier => DiceTier.D100;

        public override bool CanBeTriggered()
        {
            return true;
        }

        protected override void TriggerClient(NoNetworkData NetworkInfo)
        {
            
        }

        protected override NoNetworkData TriggerHost()
        {
            var errorAlarms = new List<ChainedPuzzleDataBlock>();
            foreach (var chainedPuzzle in ChainedPuzzleDataBlock.GetAllBlocks())
            {
                if (chainedPuzzle.PublicAlarmName.Contains("ERROR"))
                {
                    errorAlarms.Add(chainedPuzzle);
                }
            }

            Log.Debug($"Found {errorAlarms.Count} alarms to chose from...");

            var alarm = errorAlarms.GetRandomElement<ChainedPuzzleDataBlock>();

            Log.Debug($"Chose alarm '{alarm.name}' with persistenID {alarm.persistentID}");

            var localPlayer = PlayerManager.GetLocalPlayerAgent();
            var node = localPlayer.CourseNode;
            var pos = node.GetRandomPositionInside();

            Mastermind.Current.TriggerSurvivalWave(
                localPlayer.CourseNode, 
                alarm.SurvivalWaveSettings, 
                alarm.SurvivalWavePopulation, 
                out _, 
                SurvivalWaveSpawnType.InRelationToClosestAlivePlayer, 
                5,
                true, 
                false);
            
            return new NoNetworkData();
        }
    }*/
}
