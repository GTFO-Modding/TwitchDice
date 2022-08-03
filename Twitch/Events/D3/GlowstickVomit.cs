using Player;
using System.Collections;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D3
{
    public class GlowstickVomit : DiceEventWithConfig<GlowstickVomit.RundownConfig>
    {
        public override string EventName => "Glowstick Vomit";
        public override string EventDescription => "Vomits glowsticks from the players mouth.";
        public override string EventID => "glowV";

        protected override DiceTier DiceTier => DiceTier.D3;

        public override int Time => this.Config.ClientConfig.GetValue<int>(nameof(this.Time));
        public int SpawnCount => this.Config.ClientConfig.GetValue<int>(nameof(this.SpawnCount));
        public int ThrowForce => this.Config.ClientConfig.GetValue<int>(nameof(this.ThrowForce));

        public sealed class RundownConfig : DiceEventRundownConfig
        {
            public uint GlowstickID { get; set; } = 114U;
        }

        protected override DiceEventConfig<RundownConfig> GetConfig()
        {
            DiceEventConfig<RundownConfig> cfg = base.GetConfig();
            cfg.ClientConfig.Add(nameof(this.Time), "The time (in seconds) to vomit out glowsticks", 5);
            cfg.ClientConfig.Add(nameof(this.SpawnCount), "The number of glowsticks to throw", 20);
            cfg.ClientConfig.Add(nameof(this.ThrowForce), "The force to throw the glowsticks", 10);
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
            data.itemID_gearCRC = this.RundownCfg.GlowstickID;

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
