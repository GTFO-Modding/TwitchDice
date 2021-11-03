using BepInEx.Configuration;
using GTFO.API;
using SNetwork;
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
                    $"Enable {EventName.Replace(" ", "")}",
                    true,
                    $"Set if {EventName} can be activated"
                    );
                return enabled.Value;
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
        public virtual void Register() { }

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
        bool _registered = false;
        string _networkEvent;

        public override void Register()
        {
            if (_registered) return;
            _networkEvent = $"TwitchDice_DiceEvent_{typeof(T).Name}";
            NetworkAPI.RegisterEvent<T>(_networkEvent, ReceiveClient);
            _registered = true;
            Log.Debug($"Registered {EventName} with ID {EventID} and network name of {_networkEvent}");
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
