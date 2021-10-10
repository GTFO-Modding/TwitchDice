using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Player;
using SNetwork;
using TwitchLib.Client.Models;
using Nidhogg.Managers;
using BepInEx.Configuration;

namespace TwitchDice.Twitch
{
    public interface IDiceEvent
    {
        string EventID { get; }
        string EventName { get; }
        bool Enabled { get; }
        DiceTier Tier { get; }
        bool CanBeTriggered();
        void Register();
        void TriggerHost();
    }

    public abstract class DiceEvent : IDiceEvent
    {
        /// <summary>
        /// The name of the event as displayed in chat, e.g. Explode Enemies
        /// </summary>
        public abstract string EventName { get; }
        /// <summary>
        /// The ID of the event used to trigger it during testing
        /// </summary>
        public abstract string EventID { get; }
        /// <summary>
        /// The default tier of this event
        /// </summary>
        protected abstract DiceTier DiceTier { get; }
        private ConfigEntry<DiceTier> tierConfig;
        private ConfigEntry<bool> enabled;

        public DiceEvent()
        {
        }

        public DiceTier Tier
        {
            get
            {
                tierConfig = Main.Instance.Config.Bind(
                    Main.CONFIG_DICE_SECTION,
                    EventName,
                    DiceTier,
                    $"Set the tier for {EventName}"
                    );
                return tierConfig.Value;
            }
        }

        public bool Enabled
        {
            get
            {
                enabled = Main.Instance.Config.Bind(
                    Main.CONFIG_EVENTS_SECTION,
                    $"Enable {EventName}",
                    true,
                    $"Set if {EventName} can be activated"
                    );
                return enabled.Value;
            }
        }

        /// <summary>
        /// If the conditions are met for this event to be a valid event to trigger
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBeTriggered()
        {
            return true;
        }

        /// <summary>
        /// Used internally to register this event for networking (if required)
        /// </summary>
        public virtual void Register() { }
        /// <summary>
        /// Code that runs on the hosts end when this event is activated
        /// </summary>
        public abstract void TriggerHost();
    }

    public abstract class DiceEvent<T> : DiceEvent where T : struct
    {
        bool _registered = false;

        public override void Register()
        {
            if (_registered) return;
            NetworkingManager.RegisterEvent<T>($"{GetType().Name}_{typeof(T).Name}", ReceiveClient);
            _registered = true;
            Log.Debug($"Registered {EventName} with ID {EventID}");
        }

        /// <summary>
        /// Triggers this event on the client with an optional packet
        /// </summary>
        protected void TriggerClient()
        {
            TriggerClient(new T());
        }
        protected void TriggerClient(T packet)
        {
            NetworkingManager.InvokeEvent(typeof(T).Name, packet);
        }

        protected void TriggerClient(T packet, SNet_Player target)
        {
            NetworkingManager.InvokeEvent(typeof(T).Name, packet, target);
        }

        protected void TriggerClient(T packet, List<SNet_Player> players)
        {
            NetworkingManager.InvokeEvent(typeof(T).Name, packet, players);
        }

        /// <summary>
        /// Code that runs on the client when it recieves a packet for this event
        /// </summary>
        /// <param name="sender">The host</param>
        /// <param name="packet">The recieved packet</param>
        public abstract void ReceiveClient(ulong sender, T packet);
    }

    //OLD

    /*
    public abstract class OldDiceEvent<T> : IDiceEvent where T : struct
    {
        public abstract bool RequireNetworking { get; }
        public abstract bool HasNetworkData { get; }
        /// <summary>
        /// {User} rolled a {DiceTier} :: {EventName}
        /// </summary>
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

        public PlayerAgent LocalPlayer
        {
            get
            {
                return PlayerManager.GetLocalPlayerAgent();
            }
        }

        protected NoNetworkData NoNetworkData = new NoNetworkData();
        protected NetworkedNoData NetworkedNoData = new NetworkedNoData();


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

    //public interface IDiceEvent
    //{
    //    public string Activate(EventInfo eventInfo);
    //    public DiceTier GetDicetier();
    //    public string GetEventName();
    //    public string GetId();
    //    public bool CanBeTriggered();
    //    public bool GetRequireNetworking();
    //}

    public struct EventInfo
    {
        public string EventNetworkData;
        public string ActivatorUsername;
        public DiceTier Tier;
    }

    public struct TargetPlayer
    {
        public int TargetedPlayer;

        public TargetPlayer(PlayerAgent target)
        {
            TargetedPlayer = target.PlayerSlotIndex;
        }
    }

    public struct NetworkedNoData
    {

    }

    public struct NoNetworkData
    {

    }
    */
}
