using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TwitchDice.Utilities;
using TwitchDice.Twitch.Events;
using TwitchLib.Client.Models;
using Steamworks;
using SNetwork;
using UnhollowerBaseLib;
using TwitchDice.Twitch.Events.D3;
using TwitchDice.Twitch.Events.D4;
using TwitchDice.Twitch.Events.D6;
using TwitchDice.Twitch.Events.D12;
using TwitchDice.Twitch.Events.D8;
using TwitchDice.Twitch.Events.D100;
using Player;

namespace TwitchDice.Twitch
{
    public static class EventList
    {
        //public static List<iDiceEvent> Events;
        public static List<IDiceEvent> Events = new List<IDiceEvent>();
        static EventList()
        {
            //D3
            Events.Add(new RandomizeZoneLighting());
            Events.Add(new ToggleRandomWeakDoor());
            Events.Add(new ToggleFlashlights());

            //D4
            Events.Add(new MakePlayersJump());
            Events.Add(new FireShooterProjectile());
            //Events.Add(new FogCloud());

            //D6
            Events.Add(new RandomFireWeapon());

            //D8
            Events.Add(new MineScatterShot());

            //D12
            Events.Add(new BirthEffectOnPlayer());
            Events.Add(new FullyInfectAPlayer());

            //D100
            Events.Add(new EnableErrorAlarm());
            Events.Add(new GiantCharger());
            Events.Add(new ToggleAllDoors());
            Events.Add(new CrashARandomPlayer());


#if DEBUG
            Log.Debug("Events");
            foreach (var item in Events)
            {
                Log.Debug(item.GetId());
            }
#endif
        }
    }
}
