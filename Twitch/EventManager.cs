using SNetwork;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch
{
    public static class EventManager
    {
        public static bool TryActivateEvent(string eventID, string networkData)
        {
            Log.Debug($"Trying to activate event with ID: {eventID}");
            IDiceEvent diceEvent = EventList.Events.Find(diceEvent => { return diceEvent.GetId() == eventID; });
            if (diceEvent == null) return false;
            try
            {
                if (!ActivateEvent(diceEvent, new EventInfo() { EventNetworkData = networkData, ActivatorUsername = "DEBUG", Tier = diceEvent.GetDicetier() }))
                    Log.Warning($"Trigger requirements not met for ID {eventID}");
                else return true;
                return false;
            }
            catch (Exception e)
            {
                Log.Warning("Exception while trying to activate event!");
                Log.Error(e);
                return false;
            }
        }
        public static bool TryActivateEventOfTier(EventInfo eventInfo)
        {
            try
            {
                var possibleEvents = EventList.Events.FindAll(dice => { return dice.GetDicetier() == eventInfo.Tier; });
                if (possibleEvents.Count == 0) return false;

                var diceEvent = possibleEvents.GetRandomElement<IDiceEvent>();

                return ActivateEvent(diceEvent, eventInfo);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return false;
        }

        private static bool ActivateEvent(IDiceEvent diceEvent, EventInfo eventInfo)
        {
            Log.Debug($"-- Triggering Event --\n{diceEvent}");

            if (!diceEvent.CanBeTriggered()) return false;

            string networkInfo = diceEvent.Activate(eventInfo);
            if (diceEvent.GetRequireNetworking() && PlayerUtil.IsHost)
            {
                Log.Debug($"Updating Lobby Data with event {diceEvent.GetEventName()}");
                var lobby = new CSteamID(SNet.Lobby.Identifier.ID);

                SteamMatchmaking.SetLobbyData(lobby, Main.EVENT_ID_KEY, diceEvent.GetId());
                SteamMatchmaking.SetLobbyData(lobby, Main.EVENT_INFO_KEY, networkInfo);
            }

            ChatUtil.EventSpeak(eventInfo, diceEvent.GetEventName());
            return true;
        }
    }
}
