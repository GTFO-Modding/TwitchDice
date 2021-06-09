using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TwitchDice.Util;
using TwitchDice.Twitch.Events;
using TwitchLib.Client.Models;
using Steamworks;
using SNetwork;
using UnhollowerBaseLib;
using TwitchDice.Twitch.Events.D3;
using TwitchDice.Twitch.Events.D4;

using TwitchDice.Twitch.Events.D100;

namespace TwitchDice.Twitch
{
    public static class EventList
    {
        //public static List<iDiceEvent> Events;
        public static List<IDiceEvent> Events = new List<IDiceEvent>();
        private static bool? _isHost;
        private static bool IsHost
        {
            get
            {
                if (_isHost.HasValue) return _isHost.Value;
                if (SNet.Core.TryGetLobbyOwner(out SNet_Player player))
                {
                    _isHost = player.IsLocal;
                    return _isHost.Value;
                }
                Log.Error("Couldn't get lobby host :(");
                return false;
            }
        }
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

        public static bool TryActivateEvent(string eventID, string networkData)
        {
            Log.Debug($"Trying to activate event with ID: {eventID}");
            IDiceEvent diceEvent = Events.Find(diceEvent => { return diceEvent.GetId() == eventID; });
            if (diceEvent == null) return false;
            try
            {
                if (!ActivateEvent(diceEvent, new EventInfo() { EventNetworkData = networkData }))
                    Log.Warning($"Trigger requirements not met for ID {eventID}");
                else return true;
                return false;
            } catch(Exception e)
            {
                Log.Warning("Exception while trying to activate event!");
                Log.Error(e);
                return false;
            }
        }

        public static bool TryActivateEventOfTier(DiceTier tier, ChatMessage message = null)
        {
            try
            {
                var possibleEvents = Events.FindAll(dice => { return dice.GetDicetier() == tier; });
                if (possibleEvents.Count == 0) return false;

                var diceEvent = possibleEvents.GetRandomElement<IDiceEvent>();

                return ActivateEvent(diceEvent, new EventInfo() { ChatMessage = message });
            } catch(Exception e)
            {
                Log.Error(e);
            }
            return false;
        }

        private static bool ActivateEvent(IDiceEvent diceEvent, EventInfo info)
        {
            Log.Debug($"-- Triggering Event --\n{diceEvent}");

            if (!diceEvent.CanBeTriggered()) return false;
            
            string networkInfo = diceEvent.Activate(info);
            if (diceEvent.GetRequireNetworking() && IsHost)
            {
                Log.Debug($"Updating Lobby Data with event {diceEvent.GetEventName()}");
                var lobby = new CSteamID(SNet.Lobby.Identifier.ID);

                SteamMatchmaking.SetLobbyData(lobby, Main.EVENT_ID_KEY, diceEvent.GetId());
                SteamMatchmaking.SetLobbyData(lobby, Main.EVENT_INFO_KEY, networkInfo);
            }

            if (info.ChatMessage != null)
            {
                ChatUtil.EventSpeak(diceEvent.GetDicetier(), info.ChatMessage.Username, diceEvent.GetEventName());
            }
            return true;
        }
    }
}
