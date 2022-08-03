using GTFO.API;
using SNetwork;
using System;
using System.Collections.Generic;

namespace TwitchDice.Twitch
{
    public interface IDiceEvent
    {
        /// <summary>
        /// The ID of the event used to trigger it during testing
        /// </summary>
        string EventID { get; }

        /// <summary>
        /// The name of the event as displayed in chat, e.g. Explode Enemies
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// The description of the event. If not set is just <see cref="EventName">EventName</see>.
        /// </summary>
        string EventDescription { get; }

        /// <summary>
        /// Whether or not this dice event is enabled. A dice event can be disabled if
        /// disabled in the client config, or if an error occurred when loading the dice event.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The configuration for this dice event.
        /// </summary>
        IDiceEventConfig Config { get; }

        /// <summary>
        /// The default tier of the dice. Higher tiers usually cost more and are reserved for more
        /// game-breaking dice events. E.g. pulling a random door alarm is D100 due to
        /// the possibility of it pulling an alarm behind another layer or zone.
        /// <para>
        /// This can be overriden in the client config.
        /// </para>
        /// </summary>
        DiceTier Tier { get; }

        /// <summary>
        /// The time the event is enabled.
        /// </summary>
        int Time { get; }

        /// <summary>
        /// Handle logic to initialize the dice event, including loading the config.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Whether or not this event can be triggered. This is called before the event itself triggers.
        /// <para>
        /// Conditional checks should be done here. For example, <c>PullRandomAlarm</c> checks for
        /// whether any pullable alarm doors exist first.
        /// </para>
        /// </summary>
        /// <returns><see langword="true"/> if this event can be triggered, otherwise <see langword="false"/>.</returns>
        bool CanBeTriggered();

        /// <summary>
        /// Handle logic to register this dice event with networking.
        /// </summary>
        void Register();

        /// <summary>
        /// Called when triggered on the host side, so <c>SNet.IsMaster</c> will return <see langword="true"/>
        /// in this context.
        /// </summary>
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
        /// The description of this event. By default just returns <see cref="EventName"/>.
        /// </summary>
        public virtual string EventDescription => this.EventName;
        /// <summary>
        /// The default tier of this event
        /// </summary>
        protected abstract DiceTier DiceTier { get; }

        /// <summary>
        /// Forces this event to be disabled. Useful for temporarily disabling broken events.
        /// </summary>
        protected virtual bool ForceDisable => false;

        public IDiceEventConfig Config { get; }

        private bool m_initialized;
        private bool m_successfulInit;
        private bool m_registered;

        public DiceEvent()
        {
            try
            {
                this.Config = this.FetchConfig();
            }
            catch (Exception)
            {
                Log.Error($"Failed to initialize config for dice event '{this.EventName}' (ID: {this.EventID})");
                throw;
            }
        }

        protected virtual IDiceEventConfig FetchConfig()
        {
            return new DiceEventConfig(this);
        }

        public void Initialize()
        {
            if (this.m_initialized)
            {
                return;
            }

            try
            {
                this.Config.Init();
            }
            catch (Exception ex)
            {
                Log.Error($"Attempted to initialize config for dice event '{this.EventName}' (ID: {this.EventID}), but an error occurred: {ex}");
                this.m_initialized = true;
                return;
            }

            try
            {
                this.InitializeImpl();
            }
            catch (Exception ex)
            {
                this.m_initialized = true;
                Log.Error($"Attempted to initialize dice event '{this.EventName}' (ID: {this.EventID}), but an error occurred: {ex}");
                return;
            }

            this.m_initialized = true;
            this.m_successfulInit = true;
        }

        protected virtual void InitializeImpl()
        { }

        public DiceTier Tier => this.Config.ClientConfig.DiceTier;
        public bool Enabled => !this.ForceDisable && this.m_successfulInit && this.Config.ClientConfig.Enabled;

        public virtual int Time => 0;

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
            if (this.m_registered)
            {
                return;
            }

            this.RegisterImpl();
            this.m_registered = true;

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

    public abstract class DiceEventWithConfig<TRundown> : DiceEvent
        where TRundown : DiceEventRundownConfig, new()
    {
        new public DiceEventConfig<TRundown> Config => (DiceEventConfig<TRundown>)base.Config;
        public TRundown RundownCfg => this.Config.RundownConfig;

        protected sealed override IDiceEventConfig FetchConfig() => this.GetConfig();

        protected virtual DiceEventConfig<TRundown> GetConfig()
        {
            return new DiceEventConfig<TRundown>(this);
        }
    }

    public abstract class DiceEvent<T> : DiceEvent where T : struct
    {
        private string? m_networkEvent;

        protected override void RegisterImpl()
        {
            this.m_networkEvent = $"TwitchDice_DiceEvent_{typeof(T).Name}";
            NetworkAPI.RegisterEvent<T>(this.m_networkEvent, this.ReceiveClient);
            Log.Debug($"Linked event '{this.EventName}' with ID '{this.EventID}' to network name of {this.m_networkEvent}");
        }

        /// <summary>
        /// Triggers this event on the client with a default packet
        /// </summary>
        protected void TriggerClient()
        {
            this.TriggerClient(new T());
        }

        /// <summary>
        /// Triggers this event on the client with a packet
        /// </summary>
        /// <param name="packet">The packet to send</param>
        protected void TriggerClient(T packet)
        {
            NetworkAPI.InvokeEvent(this.m_networkEvent, packet);
        }

        /// <summary>
        /// Triggers this event for the targeted player
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="target">The target player</param>
        protected void TriggerClient(T packet, SNet_Player target)
        {
            NetworkAPI.InvokeEvent(this.m_networkEvent, packet, target);
        }

        /// <summary>
        /// Triggers this event for a list of players
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="players">A list of players to send to</param>
        protected void TriggerClient(T packet, List<SNet_Player> players)
        {
            NetworkAPI.InvokeEvent(this.m_networkEvent, packet, players);
        }

        /// <summary>
        /// Code that runs on the client when it recieves a packet for this event
        /// </summary>
        /// <param name="sender">The host</param>
        /// <param name="packet">The recieved packet</param>
        public abstract void ReceiveClient(ulong sender, T packet);
    }

    public abstract class DiceEventWithConfig<T, TRundown> : DiceEvent<T>
        where T : struct
        where TRundown : DiceEventRundownConfig, new()
    {
        new public DiceEventConfig<TRundown> Config => (DiceEventConfig<TRundown>)base.Config;
        public TRundown RundownCfg => this.Config.RundownConfig;

        protected sealed override IDiceEventConfig FetchConfig() => this.GetConfig();

        protected virtual DiceEventConfig<TRundown> GetConfig()
        {
            return new DiceEventConfig<TRundown>(this);
        }
    }
}
