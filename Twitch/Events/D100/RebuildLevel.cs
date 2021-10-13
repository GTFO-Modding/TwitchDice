using System.Collections;
using System.Runtime.InteropServices;
using LevelGeneration;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D100
{
    public class RebuildLevel : DiceEvent<RL>
    {
        public override string EventName => "RL";

        public override string EventID => "rebuildlevel";

        protected override DiceTier DiceTier => DiceTier.D100;

        public override void ReceiveClient(ulong sender, RL packet)
        {
            TimedEvents.Start(this.DoTriggerEvent());
        }
        
        public override void TriggerHost()
        {
            this.TriggerClient(new RL());
            TimedEvents.Start(this.DoTriggerEvent());
        }
        
        private IEnumerator DoTriggerEvent()
        {   
            Builder.Current.OnLevelCleanup();
            yield return new WaitForEndOfFrame();
            Builder.Current.Build();
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RL
    {
        
    }
}