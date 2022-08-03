using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AIGraph;
using GameData;
using Player;
using SNetwork;
using TwitchDice.Extensions;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D100
{
    public class EnableErrorAlarm : DiceEventWithConfig<EnableErrorAlarm.RundownConfig>
    {
        public override string EventName => "Alarm Malfunction";
        public override string EventDescription => "Activates a random error alarm.";
        public override string EventID => "errorAlarm";

        protected override DiceTier DiceTier => DiceTier.D100;

        protected override DiceEventConfig<RundownConfig> GetConfig()
        {
            DiceEventConfig<RundownConfig> cfg = base.GetConfig();
            cfg.ExampleRundownConfig = new()
            {
                Levels = new()
                {
                    new LevelSettings()
                    {
                        ErrorAlarms = new()
                        {
                            new ErrorAlarmInfo()
                            {
                                PopulationID = 1,
                                SettingsID = 1
                            }
                        },
                        ExpeditionIndex = 9,
                        Tier = eRundownTier.TierA
                    }
                }
            };
            return cfg;
        }

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public List<LevelSettings> Levels { get; set; } = new();

            protected override void InitImpl(IDiceEvent diceEvent)
            {
                if (this.Levels == null)
                {
                    this.Levels = new();
                }

                this.Levels.RemoveAll((level) => level == null);

                foreach (LevelSettings level in this.Levels)
                {
                    level.Init();
                }
            }

            public void TryAppendErrorAlarms(List<ErrorAlarmInfo> alarms)
            {
                if (this.Levels == null)
                {
                    return;
                }

                foreach (LevelSettings level in this.Levels)
                {
                    level.TryAppendErrorAlarms(alarms);
                }
            }
        }

        public sealed class LevelSettings
        {
            public List<ErrorAlarmInfo> ErrorAlarms { get; set; } = new();
            public eRundownTier Tier { get; set; }
            public int ExpeditionIndex { get; set; }

            public void Init()
            {
                if (this.ErrorAlarms == null)
                {
                    this.ErrorAlarms = new();
                }

                this.ErrorAlarms.RemoveAll((errorAlarm) => errorAlarm == null);

                foreach (ErrorAlarmInfo alarm in this.ErrorAlarms)
                {
                    alarm.Init();
                }
            }

            public void TryAppendErrorAlarms(List<ErrorAlarmInfo> alarms)
            {
                pActiveExpedition exp = SNet.GetLocalCustomData<pActiveExpedition>();

                if (exp.expeditionIndex != this.ExpeditionIndex || exp.tier != this.Tier)
                {
                    return;
                }

                if ((this.ErrorAlarms?.Count ?? 0) == 0)
                {
                    return;
                }
                else
                {
                    alarms.AddRange(this.ErrorAlarms!);
                }
            }
        }

        public sealed class ErrorAlarmInfo
        {
            public uint PopulationID { get; set; }
            public uint SettingsID { get; set; }

            public ErrorAlarmInfo()
            { }

            public ErrorAlarmInfo(ChainedPuzzleDataBlock chainedPuzzle)
            {
                this.PopulationID = chainedPuzzle.SurvivalWavePopulation;
                this.SettingsID = chainedPuzzle.SurvivalWaveSettings;
            }

            public void Init()
            { }
        }

        public override void TriggerHost()
        {
            var errorAlarms = new List<ErrorAlarmInfo>();
            this.RundownCfg.TryAppendErrorAlarms(errorAlarms);
            if (errorAlarms.Count == 0)
            {
                foreach (ChainedPuzzleDataBlock? chainedPuzzle in ChainedPuzzleDataBlock.GetAllBlocks())
                {
                    if (chainedPuzzle.DisableSurvivalWaveOnComplete)
                    {
                        continue;
                    }

                    errorAlarms.Add(new ErrorAlarmInfo(chainedPuzzle));
                }
            }

            ErrorAlarmInfo alarm = errorAlarms.GetRandomElement<ErrorAlarmInfo>();

            PlayerAgent localPlayer = PlayerManager.GetLocalPlayerAgent();
            AIG_CourseNode node = localPlayer.CourseNode;
            Vector3 pos = node.GetRandomPositionInside();

            Mastermind.Current.TriggerSurvivalWave(
                localPlayer.CourseNode,
                alarm.SettingsID,
                alarm.PopulationID,
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
