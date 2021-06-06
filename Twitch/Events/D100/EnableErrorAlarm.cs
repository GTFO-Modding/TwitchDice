using System;
using System.Collections.Generic;
using System.Text;
using GameData;
using Player;
using TwitchDice.Util;

namespace TwitchDice.Twitch.Events.D100
{
    public class EnableErrorAlarm : DiceEvent<NoNetworkData>
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
                if (!chainedPuzzle.DisableSurvivalWaveOnComplete)
                {
                    errorAlarms.Add(chainedPuzzle);
                }
            }

            Log.Debug($"Found {errorAlarms.Count} alarms to chose from...");

            var alarm = errorAlarms.GetRandomElement<ChainedPuzzleDataBlock>();

            Log.Debug($"Chose alarm {alarm.persistentID}, name {alarm.name}...");

            var localPlayer = PlayerManager.GetLocalPlayerAgent();

            var soundPlayer = new CellSoundPlayer(localPlayer.Position);
            soundPlayer.Post(alarm.AlarmSoundStop);

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
    }
}
