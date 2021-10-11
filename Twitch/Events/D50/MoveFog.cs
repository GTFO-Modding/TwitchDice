using GameData;
using System.Collections;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class MoveFog : DiceEvent<MF>
    {
        public override string EventName => "MoveFog";

        public override string EventID => "movefog";

        protected override DiceTier DiceTier => DiceTier.D50;
        
        public override void ReceiveClient(ulong sender, MF packet)
        {
            TimedEvents.Start(this.DoTriggerEvent(5, packet.delta));
        }
        
        public override void TriggerHost()
        {
            // Random height change from -10 to 10
            float delta = (float) Main.rnd.NextDouble() * 20.0f - 10.0f;
            this.TriggerClient(new MF(delta));
            TimedEvents.Start(this.DoTriggerEvent(5,delta));
        }
        
        private IEnumerator DoTriggerEvent(float seconds, float delta)
        {
            float currentFogBlend = 0.0f;
            
            uint fogSettings = RundownManager.ActiveExpedition.Expedition.FogSettings;
            FogSettingsDataBlock fogBlock = GameDataBlockBase<FogSettingsDataBlock>.GetBlock(fogSettings);

            fogBlock.DensityHeightAltitude += delta;
            LocalPlayerAgentSettings.Current.SetTargetFogSettings(fogBlock);

            while (currentFogBlend < 1.0f)
            {
                yield return null;
                currentFogBlend += Time.deltaTime / seconds;
                LocalPlayerAgentSettings.Current.UpdateBlendTowardsTargetFogSetting(currentFogBlend);
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct MF
    {
        public float delta;

        public MF(float delta)
        {
            this.delta = delta;
        }
    }
}
   
