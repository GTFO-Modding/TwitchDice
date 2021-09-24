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
        DiceTier Tier { get; }
        bool CanBeTriggered();
        void Register();
        void TriggerHost();
    }

    public abstract class DiceEvent : IDiceEvent
    {
        public abstract string EventName { get; }
        public abstract string EventID { get; }
        protected abstract DiceTier DiceTier { get; }
        private ConfigEntry<DiceTier> configEntry;

        public DiceEvent()
        {
        }

        public DiceTier Tier
        {
            get
            {
                configEntry = Main.Instance.Config.Bind(
                    Main.CONFIG_DICE_SECTION,
                    EventID,
                    DiceTier,
                    $"Set the tier for {EventName}"
                    );
                return configEntry.Value;
            }
        }

        public virtual bool CanBeTriggered()
        {
            return true;
        }

        public virtual void Register() { }
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
