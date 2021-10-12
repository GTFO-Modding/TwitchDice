using System.Collections;
using System.Runtime.InteropServices;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D50
{
    public class VoidFog : DiceEvent<VF>
    {
        public override string EventName => "The Darkness Consumes You";

        public override string EventID => "voidfog";

        protected override DiceTier DiceTier => DiceTier.D50;
        
        public override int Time => 30;
        
        public override void ReceiveClient(ulong sender, VF packet)
        {
            TimedEvents.StartTimedEvent(this.DoTriggerEvent(packet.seconds, packet.fogColor, packet.fogDensity), this);
        }
        
        public override void TriggerHost()
        {
            float seconds = Time;
            var fogColor = Color.black;
            float fogDensity = 0.2f;

            this.TriggerClient(new VF(seconds, fogColor, fogDensity));
            TimedEvents.StartTimedEvent(this.DoTriggerEvent(seconds, fogColor, fogDensity), this);
        }
        
        private IEnumerator DoTriggerEvent(float seconds, Color fogColor, float fogDensity)
        {
            initialColor = PreLitVolume.Current.m_fogColor;
            initialDensity = PreLitVolume.Current.m_fogDensity;

            PreLitVolume.Current.m_fogColor = fogColor;
            PreLitVolume.Current.m_fogDensity = fogDensity;
            
            yield return new WaitForSeconds(seconds);

            if (PreLitVolume.Current != null)
            {
                PreLitVolume.Current.m_fogColor = initialColor;
                PreLitVolume.Current.m_fogDensity = initialDensity;
            }
        }

        private Color initialColor;
        private float initialDensity;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct VF
    {
        public float seconds;
        public Color fogColor;
        public float fogDensity;

        public VF(float seconds, Color fogColor, float fogDensity)
        {
            this.seconds = seconds;
            this.fogColor = fogColor;
            this.fogDensity = fogDensity;
        }
    }
}