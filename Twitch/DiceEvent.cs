using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SNetwork;
using TwitchLib.Client.Models;

namespace TwitchDice.Twitch
{
    public abstract class DiceEvent<T> : IDiceEvent
    {
        public abstract bool RequireNetworking { get; }
        public abstract bool HasNetworkData { get; }
        public abstract string EventName { get; }
        public abstract string EventId { get; }
        public abstract DiceTier Tier { get; }
        protected bool IsHost
        {
            get
            {
                if (SNet.Core.TryGetLobbyOwner(out SNet_Player player))
                {
                    return player.IsLocal;
                }
                Log.Error("Couldn't get lobby host :(");
                return false;
            }
        }

        public string Activate(EventInfo eventInfo)
        {
            if (eventInfo.EventNetworkData != "" && RequireNetworking && HasNetworkData)
            {
                try
                {
                    var incomingNetworkData = JsonConvert.DeserializeObject<T>(eventInfo.EventNetworkData);
                    TriggerClient(incomingNetworkData);
                    return null;
                } catch(Exception e) { Log.Error($"-- Issue while trying to handle networked event! D: -- \n{e}"); }
            }

            T outgoingNetworkData = TriggerHost();

            if (RequireNetworking && HasNetworkData)
            {
                return JsonConvert.SerializeObject(outgoingNetworkData);
            }

            return null;
        }
        protected abstract T TriggerHost();

        protected abstract void TriggerClient(T NetworkInfo);

        public abstract bool CanBeTriggered();

        public override string ToString()
        {
            return $"RequireNetworking: {RequireNetworking}\nEventName: {EventName}\nEventId:{EventId}\nTier:{Tier}";
        }

        public DiceTier GetDicetier()
        {
            return Tier;
        }
        public string GetEventName()
        {
            return EventName;
        }
        public string GetId()
        {
            return EventId;
        }
        public bool GetRequireNetworking()
        {
            return RequireNetworking;
        }
    }

    public interface IDiceEvent
    {
        public string Activate(EventInfo eventInfo);
        public DiceTier GetDicetier();
        public string GetEventName();
        public string GetId();
        public bool CanBeTriggered();
        public bool GetRequireNetworking();
    }

    public struct EventInfo
    {
        public string EventNetworkData;
        public ChatMessage ChatMessage;
    }

    public struct NetworkedNoData
    {

    }

    public struct NoNetworkData
    {

    }
}
