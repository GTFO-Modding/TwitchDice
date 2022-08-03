using GameData;
using LevelGeneration;
using SNetwork;
using System.Collections.Generic;

namespace TwitchDice.Twitch.Events.D100
{
    public class ToggleAllDoors : DiceEventWithConfig<ToggleAllDoors.RundownConfig>
    {
        public override string EventName => "Toggle All Doors";
        public override string EventDescription => "Toggles all weak doors and security doors on the map (Opens doors if closed, closes door if opened). Security doors will stay open.";
        public override string EventID => "toggleAll";

        protected override DiceTier DiceTier => DiceTier.D100;

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            // example config
            public List<LevelRestrictions> Levels { get; set; } = new()
            {
                new LevelRestrictions()
                {
                    ExcludeSecurityDoors = new()
                    {
                        new DoorRestriction()
                        {
                            Dimension = eDimensionIndex.Dimension_20,
                            Layer = LG_LayerType.ThirdLayer,
                            Zone = eLocalZoneIndex.Zone_20
                        }
                    },
                    ExpeditionIndex = 9,
                    Tier = eRundownTier.TierA
                }
            };

            protected override void InitImpl(IDiceEvent diceEvent)
            {
                if (this.Levels == null)
                {
                    this.Levels = new();
                }

                this.Levels.RemoveAll((level) => level == null);

                foreach (LevelRestrictions level in this.Levels)
                {
                    level.Init();
                }
            }

            public bool Allowed(LG_SecurityDoor door)
            {
                if (this.Levels == null)
                {
                    return true;
                }

                foreach (LevelRestrictions level in this.Levels)
                {
                    if (!level.Allowed(door))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public sealed class LevelRestrictions
        {
            public List<DoorRestriction> ExcludeSecurityDoors { get; set; } = new();
            public eRundownTier Tier { get; set; }
            public int ExpeditionIndex { get; set; }

            public void Init()
            {
                if (this.ExcludeSecurityDoors == null)
                {
                    this.ExcludeSecurityDoors = new();
                }

                this.ExcludeSecurityDoors.RemoveAll((door) => door == null);

                foreach (DoorRestriction restriction in this.ExcludeSecurityDoors)
                {
                    restriction.Init();
                }
            }

            public bool Allowed(LG_SecurityDoor door)
            {
                pActiveExpedition exp = SNet.GetLocalCustomData<pActiveExpedition>();
                
                if (exp.expeditionIndex != this.ExpeditionIndex || exp.tier != this.Tier)
                {
                    return true;
                }

                if (this.ExcludeSecurityDoors == null)
                {
                    return true;
                }

                foreach (DoorRestriction restriction in this.ExcludeSecurityDoors)
                {
                    if (!restriction.Allowed(door))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public sealed class DoorRestriction
        {
            public eDimensionIndex Dimension { get; set; }
            public LG_LayerType Layer { get; set; }
            public eLocalZoneIndex Zone { get; set; }

            public void Init()
            { }

            public bool Allowed(LG_SecurityDoor door)
            {
                LG_Zone zone = door.Gate.m_linksTo.m_zone;
                LG_Layer layer = zone.Layer;
                Dimension dimension = layer.m_dimension;

                if (this.Dimension != dimension.DimensionIndex)
                {
                    return true;
                }

                if (this.Layer != layer.m_type)
                {
                    return true;
                }

                if (this.Zone != zone.LocalIndex)
                {
                    return true;
                }

                return false;
            }


        }

        public override void TriggerHost()
        {
            foreach (iLG_Door_Core doorCore in Builder.Current.m_currentFloor.GetComponentsInChildren<iLG_Door_Core>())
            {
                LG_SecurityDoor? securityDoor = doorCore.TryCast<LG_SecurityDoor>();
                if (securityDoor != null)
                {
                    if (!this.RundownCfg.Allowed(securityDoor))
                    {
                        continue;
                    }
                }

                doorCore.AttemptOpenCloseInteraction(false);
            }
        }
    }

    /*public class ToggleAllDoors : global::DiceEvent<NoNetworkData>
    {
        public override bool RequireNetworking => false;

        public override bool HasNetworkData => false;

        public override string EventName => "Toggle All Doors";

        public override string EventId => "d100_toggleAllDoors";

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
            foreach (iLG_Door_Core doorCore in Builder.Current.m_currentFloor.GetComponentsInChildren<iLG_Door_Core>())
            {
                doorCore.AttemptOpenCloseInteraction(false);
            }
            return new NoNetworkData();
        }
    }*/
}
