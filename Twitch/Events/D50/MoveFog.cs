using System.Collections;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class MoveFog : DiceEvent<MF>
    {
        public override string EventName => "Move Fog";
        public override string EventDescription => "Moves the fog up/down";
        public override string EventID => "movefog";

        protected override DiceTier DiceTier => DiceTier.D50;

        public override void ReceiveClient(ulong sender, MF packet)
        {
            TimedEvents.Start(this.DoTriggerEvent(packet.seconds, packet.delta));
        }
        
        public override void TriggerHost()
        {
            float seconds = 5f;
            
            // Random height change in (-10, 10) 
            float delta = (float) Main.rnd.NextDouble() * 20.0f - 10.0f;

            this.TriggerClient(new MF(seconds, delta));
            TimedEvents.Start(this.DoTriggerEvent(seconds, delta));
        }
        
        private IEnumerator DoTriggerEvent(float seconds, float delta)
        {   
            float targetHeight = PreLitVolume.Current.m_densityHeightAltitude + delta;

            while (delta > 0 && PreLitVolume.Current!.m_densityHeightAltitude < targetHeight 
                   || delta < 0 && PreLitVolume.Current!.m_densityHeightAltitude > targetHeight)
            {
                yield return new WaitForEndOfFrame();
                if (PreLitVolume.Current != null)
                {
                    PreLitVolume.Current.m_densityHeightAltitude +=  delta * (UnityEngine.Time.deltaTime / seconds);
                }
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct MF
    {
        public float seconds;
        public float delta;

        public MF(float seconds, float delta)
        {
            this.seconds = seconds;
            this.delta = delta;
        }
    }
}
   
