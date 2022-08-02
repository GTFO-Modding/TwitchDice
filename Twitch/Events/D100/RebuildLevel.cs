using System;
using System.Collections;
using System.Runtime.InteropServices;
using GameData;
using Globals;
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
            TimedEvents.Start(this.DoTriggerEvent(packet.seed));
        }
        
        public override void TriggerHost()
        {
            int seed = (int) (Main.rnd.NextDouble() * int.MaxValue);
            this.TriggerClient(new RL(seed));
            TimedEvents.Start(this.DoTriggerEvent(seed));
        }
        
        private IEnumerator DoTriggerEvent(int seed)
        {   
            Log.Warning($"{seed}");
            Builder.Current.OnLevelCleanup();

            Global.UseStaticSeed = true;
            Global.StaticSeed = seed;
            
            yield return new WaitForEndOfFrame();
            
            Builder.Current.Build();
            
            Global.UseStaticSeed = false;
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RL
    {
        public int seed;
        
        public RL(int seed)
        {
            this.seed = seed;
        }
    }
}