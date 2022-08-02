using BepInEx.Configuration;
using GTFO.API;
using SNetwork;
using System;
using System.Collections.Generic;

namespace TwitchDice.Twitch
{
    public interface IDiceEvent
    {
        string EventID { get; }
        string EventName { get; }
        bool Enabled { get; }
        DiceTier Tier { get; }
        int Time { get; }
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
        private bool registered;

        public DiceEvent()
        {
        }

        private void FetchConfig()
        {
            try
            {
                if (this.enabled == null)
                {
                    this.enabled = Main.Instance.Config.Bind(
                        Main.CONFIG_EVENTS_SECTION,
                        $"Enable {this.EventName.Replace(" ", "")}",
                        true,
                        $"Set if {this.EventName} can be activated"
                    );
                }
                if (this.tierConfig == null)
                {
                    this.tierConfig = Main.Instance.Config.Bind(
                        Main.CONFIG_DICE_SECTION,
                        this.EventName,
                        this.DiceTier,
                        $"Set the tier for {this.EventName}"
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Attempted to register dice event '{this.EventName}', but failed to fetch a config entry: {ex}");
            }
        }

        public DiceTier Tier
        {
            get
            {
                this.FetchConfig();
                return this.tierConfig.Value;
            }
        }

        public bool Enabled
        {
            get
            {
                this.FetchConfig();
                return this.enabled.Value;
            }
        }

        virtual public int Time => 0;

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
        public void Register()
        {
            if (this.registered)
            {
                return;
            }

            this.RegisterImpl();
            this.registered = true;

            Log.Debug($"Registered DiceEvent '{this.EventName}' with ID '{this.EventID}'");
        }

        protected virtual void RegisterImpl()
        { }

        /// <summary>
        /// Code that runs on the hosts end when this event is activated
        /// </summary>
        public abstract void TriggerHost();

        protected void StartEventTimer()
        {
            EventTimerManager.Instance.AddTimedInstance(this);
        }
    }

    public abstract class DiceEvent<T> : DiceEvent where T : struct
    {
        string _networkEvent;

        protected override void RegisterImpl()
        {
            _networkEvent = $"TwitchDice_DiceEvent_{typeof(T).Name}";
            NetworkAPI.RegisterEvent<T>(_networkEvent, this.ReceiveClient);
            Log.Debug($"Linked event '{this.EventName}' with ID '{this.EventID}' to network name of {this._networkEvent}");
        }

        /// <summary>
        /// Triggers this event on the client with a default packet
        /// </summary>
        protected void TriggerClient()
        {
            TriggerClient(new T());
        }

        /// <summary>
        /// Triggers this event on the client with a packet
        /// </summary>
        /// <param name="packet">The packet to send</param>
        protected void TriggerClient(T packet)
        {
            NetworkAPI.InvokeEvent(_networkEvent, packet);
        }

        /// <summary>
        /// Triggers this event for the targeted player
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="target">The target player</param>
        protected void TriggerClient(T packet, SNet_Player target)
        {
            NetworkAPI.InvokeEvent(_networkEvent, packet, target);
        }

        /// <summary>
        /// Triggers this event for a list of players
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="players">A list of players to send to</param>
        protected void TriggerClient(T packet, List<SNet_Player> players)
        {
            NetworkAPI.InvokeEvent(_networkEvent, packet, players);
        }

        /// <summary>
        /// Code that runs on the client when it recieves a packet for this event
        /// </summary>
        /// <param name="sender">The host</param>
        /// <param name="packet">The recieved packet</param>
        public abstract void ReceiveClient(ulong sender, T packet);
    }
}
