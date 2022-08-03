using SNetwork;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchDice.Extensions;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch
{
    public class EventManager
    {
        public Dictionary<string, IDiceEvent> Events;
        public Lookup<DiceTier, IDiceEvent> TierLookup;
        public EventManager()
        {
            Type type = typeof(DiceEvent);
            IEnumerable<Type> types = GetType().Assembly.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(type));

            Log.Debug($"Found {types.Count()} events");

            this.Events = new Dictionary<string, IDiceEvent>();

            Log.Debug("Created events dictionary");
            foreach (Type item in types)
            {
                string diceEventID = null;
                try
                {
                    if (item.GetConstructors().Length == 0)
                    {
                        Log.Error("No constructor????");
                    }

                    object diceType = Activator.CreateInstance(item);

                    if (diceType == null) { Log.Warning("Unable to instantiate type"); return; }

                    IDiceEvent diceEvent = diceType as IDiceEvent;
                    diceEventID = diceEvent.EventID;

                    if (!diceEvent.Enabled)
                    {
                        continue;
                    }

                    this.Events.Add(diceEvent.EventID, diceEvent);
                    diceEvent.Register();
                }
                catch (Exception ex)
                {
                    if (diceEventID == null)
                    {
                        Log.Error("Failed registering a dice event! " + ex);
                    }
                    else
                    {
                        Log.Error($"Failed registering dice event '{diceEventID}': {ex}");
                    }
                }
            }

            this.TierLookup = (Lookup<DiceTier, IDiceEvent>)this.Events.ToLookup(p => p.Value.Tier, p => p.Value);
        }

        public bool TryActivateEvent(string eventID, string activator = null)
        {
            
            if (!this.Events.TryGetValue(eventID, out IDiceEvent diceEvent))
            {
                return false;
            }

            if (!diceEvent.CanBeTriggered())
            {
                Log.Warning($"Trigger requirements not met for ID {eventID}");
                return false;
            }

            ActivateEvent(diceEvent, activator);
            return true;
        }

        public bool TryActivateEventOfTier(DiceTier tier, string activator = null)
        {
            IEnumerable<IDiceEvent> events = this.TierLookup[tier];
            events = events.Where(e => e.CanBeTriggered());
            if (!events.Any())
            {
                Log.Warning($"No valid events found for tier {tier}");
                return false;
            }

            IDiceEvent diceEvent = events.ToList().GetRandomElement<IDiceEvent>();
            ActivateEvent(diceEvent, activator);
            return true;
        }

        private static void ActivateEvent(IDiceEvent diceEvent, string activator = null)
        {
            if (activator == null)
            {
                activator = PlayerUtil.LocalNetAgent.NickName;
            }

            try
            {
                Log.Debug($"Trying to activate event {diceEvent.EventName}");
                diceEvent.TriggerHost();
                ChatUtil.EventSpeak(diceEvent, activator);
            }
            catch (Exception e)
            {
                Log.Warning("Exception while trying to activate event!");
                Log.Error(e);
            }
        }



        //struct DEvent
        //{
        //    DiceTier Tier;
        //    IDiceEvent Event;
        //}


        //public static bool TryActivateEvent(string eventID, string networkData)
        //{
        //    Log.Debug($"Trying to activate event with ID: {eventID}");
        //    IDiceEvent diceEvent = EventList.Events.Find(diceEvent => { return diceEvent.GetId() == eventID; });
        //    if (diceEvent == null) return false;
        //    try
        //    {
        //        if (!ActivateEvent(diceEvent, new EventInfo() { EventNetworkData = networkData, ActivatorUsername = "DEBUG", Tier = diceEvent.GetDicetier() }))
        //            Log.Warning($"Trigger requirements not met for ID {eventID}");
        //        else return true;
        //        return false;
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Warning("Exception while trying to activate event!");
        //        Log.Error(e);
        //        return false;
        //    }
        //}
        //public static bool TryActivateEventOfTier(EventInfo eventInfo)
        //{
        //    try
        //    {
        //        var possibleEvents = EventList.Events.FindAll(dice => { return dice.GetDicetier() == eventInfo.Tier; });
        //        if (possibleEvents.Count == 0) return false;
        //
        //        var diceEvent = possibleEvents.GetRandomElement<IDiceEvent>();
        //
        //        return ActivateEvent(diceEvent, eventInfo);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e);
        //    }
        //    return false;
        //}
        //
        //private static bool ActivateEvent(IDiceEvent diceEvent, EventInfo eventInfo)
        //{
        //    Log.Debug($"-- Triggering Event --\n{diceEvent}");
        //
        //    if (!diceEvent.CanBeTriggered()) return false;
        //
        //    string networkInfo = diceEvent.Activate(eventInfo);
        //    if (diceEvent.GetRequireNetworking() && PlayerUtil.IsHost)
        //    {
        //        Log.Debug($"Updating Lobby Data with event {diceEvent.GetEventName()}");
        //        var lobby = new CSteamID(SNet.Lobby.Identifier.ID);
        //
        //        SteamMatchmaking.SetLobbyData(lobby, Main.EVENT_ID_KEY, diceEvent.GetId());
        //        SteamMatchmaking.SetLobbyData(lobby, Main.EVENT_INFO_KEY, networkInfo);
        //    }
        //
        //    ChatUtil.EventSpeak(eventInfo, diceEvent.GetEventName());
        //    return true;
        //}
    }
}
