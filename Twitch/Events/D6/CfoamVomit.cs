using Player;
using System.Collections;
using TwitchDice.Utilities;
using UnityEngine;


namespace TwitchDice.Twitch.Events.D6
{
    public class CfoamVomit : DiceEventWithConfig<CfoamVomit.RundownConfig>
    {
        public override string EventName => "C-Foam Vomit";
        public override string EventDescription => "Makes a random player vomit cfoam grenades.";
        public override string EventID => "cfoamV";

        protected override DiceTier DiceTier => DiceTier.D6;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));
        public int SpawnCount => this.Config.ClientConfig.GetValue<int>(nameof(this.SpawnCount));
        public int ThrowForce => this.Config.ClientConfig.GetValue<int>(nameof(this.ThrowForce));

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public uint CFoamGrenadeID { get; set; } = 115U;
        }

        protected override DiceEventConfig<RundownConfig> GetConfig()
        {
            DiceEventConfig<RundownConfig> cfg = base.GetConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to vomit out cfoam grenades", 5);
            cfg.ClientConfig.Add(nameof(this.SpawnCount), "The number of cfoam grenades to throw", 20);
            cfg.ClientConfig.Add(nameof(this.ThrowForce), "The force to throw the cfoam grenades", 10);
            return cfg;
        }

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
            TimedEvents.StartTimedEvent(this.Vomit(target), this);
        }

        private IEnumerator Vomit(PlayerAgent target)
        {
            pItemData data = default;
            data.itemID_gearCRC = this.RundownCfg.CFoamGrenadeID;

            for (int i = 0; i < this.SpawnCount; i++)
            {
                RaycastHit hit = SpawnUtil.GetRandomPointAround(target.EyePosition);
                SpawnUtil.ThrowConsumable(target.EyePosition, hit.point, target, data, this.ThrowForce);
                float delay = (float)this.Time / this.SpawnCount;
                yield return new WaitForSecondsRealtime(delay);
            }

            yield break;
        }
    }
}
