using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch
{
    public interface IDiceEventConfig
    {
        DiceEventClientConfig ClientConfig { get; }
        DiceEventRundownConfig? RundownConfig { get; }
        IDiceEvent DiceEvent { get; }

        void Init();
    }

    public abstract class DiceEventRundownConfig
    {
        [JsonIgnore]
        private bool m_initialized;

        public void Init(IDiceEvent diceEvent)
        {
            if (this.m_initialized)
            {
                return;
            }

            this.InitImpl(diceEvent);
            this.m_initialized = true;
        }

        protected virtual void InitImpl(IDiceEvent diceEvent)
        {

        }
    }

    public sealed class DiceEventClientConfig
    {
        private const string ENABLED = "Enabled";
        private const string DICE_TIER = "DiceTier";

        private readonly ConfigEntry<bool> m_enabledEntry;
        private readonly ConfigEntry<DiceTier> m_diceTierEntry;
        private readonly Dictionary<string, IConfigEntry> m_entries = new();
        private bool m_initialized;

        public DiceEventClientConfig()
        {
            this.m_enabledEntry = new(ENABLED, "Set if {EventName} can be activated", true);
            this.m_diceTierEntry = new(DICE_TIER, "Set the tier for {EventName}", default);

            this.m_entries.Add(ENABLED, this.m_enabledEntry);
            this.m_entries.Add(DICE_TIER, this.m_diceTierEntry);
            this.m_initialized = false;
        }

        public bool Initialized => this.m_initialized;

        public bool Enabled => this.Initialized && this.m_enabledEntry.GetValue();
        public DiceTier DiceTier => this.Initialized ? this.m_diceTierEntry.GetValue() : default;

        /// <summary>
        /// Adds a config entry to this config.
        /// <para>
        /// For the description, you can use the following placeholders:
        /// <list type="bullet">
        /// <item>
        /// <term><c>{EventName}</c></term>
        /// <description>
        /// Replaced with <see cref="IDiceEvent.EventName">EventName</see> in the dice
        /// event.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>{EventID}</c></term>
        /// <description>
        /// Replaced with <see cref="IDiceEvent.EventID">EventID</see> in the dice
        /// event.
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of value the entry will hold.</typeparam>
        /// <param name="key">The config entry key.</param>
        /// <param name="description">The description of the config entry.</param>
        /// <param name="defaultValue">The default value of the entry. If not specified in the config, will use
        /// this value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or
        /// <paramref name="description"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if a key <paramref name="key"/>
        /// already exists in this config.</exception>
        public void Add<T>(string key, string description, T defaultValue)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (description is null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (this.m_entries.ContainsKey(key) ||
                key == ENABLED || key == DICE_TIER)
            {
                throw new ArgumentException($"Key '{key}' in config already exists!", nameof(key));
            }

            this.m_entries.Add(key, new ConfigEntry<T>(key, description, defaultValue));
        }

        /// <summary>
        /// Attempts to get the value at the given key.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="key">The key of the entry to get the value of.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/>
        /// is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">No entry with key
        /// <paramref name="key"/> exists.</exception>
        /// <exception cref="InvalidCastException">The value of the entry
        /// cannot be converted to <typeparamref name="T"/>.</exception>
        /// <exception cref="NullReferenceException">The entry with key
        /// <paramref name="key"/> wasn't bound because this wasn't initialized
        /// yet.</exception>
        public T? GetValue<T>(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IConfigEntry entry;
            if (key == ENABLED)
            {
                entry = this.m_enabledEntry;
            }
            else if (key == DICE_TIER)
            {
                entry = this.m_diceTierEntry;
            }
            else if (!this.m_entries.TryGetValue(key, out entry!))
            {
                throw new KeyNotFoundException($"No such config entry with id '{key}' exists!");
            }

            return (T?)entry.GetValue();
        }

        public void Init(IDiceEvent diceEvent)
        {
            if (this.m_initialized)
            {
                return;
            }

            this.m_diceTierEntry.DefaultValue = diceEvent.Tier;

            ConfigFile config = Main.Instance.Config;
            this.m_enabledEntry.Bind(config, diceEvent);
            this.m_diceTierEntry.Bind(config, diceEvent);

            foreach (IConfigEntry entry in this.m_entries.Values)
            {
                try
                {
                    entry.Bind(config, diceEvent);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to bind entry '{entry.Key}' for dice event '{diceEvent.EventName}' (ID: {diceEvent.EventID}): " + ex);
                }
            }
            this.m_initialized = true;
        }

        private interface IConfigEntry
        {
            string Key { get; }
            string Description { get; }

            void Bind(ConfigFile config, IDiceEvent diceEvent);
            object? GetValue();
        }

        private sealed class ConfigEntry<T> : IConfigEntry
        {
            public string Key { get; }
            public string Description { get; }
            public T DefaultValue { get; set; }
            private BepInEx.Configuration.ConfigEntry<T>? m_entry;

            public ConfigEntry(string key, string description, T value)
            {
                this.Key = key;
                this.Description = description;
                this.DefaultValue = value;
                this.m_entry = null;
            }

            private static string FormatStr(string str, IDiceEvent diceEvent)
            {
                return str.Replace("{EventName}", diceEvent.EventName)
                    .Replace("{EventID}", diceEvent.EventID);
            }

            public T GetValue()
            {
                return this.m_entry!.Value;
            }

            public void Bind(ConfigFile config, IDiceEvent diceEvent)
            {
                if (this.m_entry != null)
                {
                    throw new Exception($"Config entry '{this.Key}' is already bound!");
                }

                this.m_entry = config.Bind($"Event {diceEvent.EventName} Options",
                    this.Key,
                    this.DefaultValue,
                    FormatStr(this.Description, diceEvent));
            }

            object? IConfigEntry.GetValue() => this.GetValue();
        }
    }

    /// <summary>
    /// A dice event config with an empty rundown config.
    /// </summary>
    public sealed class DiceEventConfig : IDiceEventConfig
    {
        public DiceEventClientConfig ClientConfig { get; }
        public DiceEventRundownConfig? RundownConfig => null;
        public IDiceEvent DiceEvent { get; }

        public DiceEventConfig(IDiceEvent eventOwner, DiceEventClientConfig? clientConfig = null)
        {
            this.DiceEvent = eventOwner;
            this.ClientConfig = clientConfig ?? new();
        }

        public void Init()
        {
            this.ClientConfig.Init(this.DiceEvent);
        }
    }

    /// <summary>
    /// A dice event config with the given rundown config.
    /// </summary>
    /// <typeparam name="TRundownConfig">The rundown config type.</typeparam>
    public class DiceEventConfig<TRundownConfig> : IDiceEventConfig
        where TRundownConfig : DiceEventRundownConfig, new()
    {
        public DiceEventClientConfig ClientConfig { get; }
        public TRundownConfig RundownConfig { get; set; }
        public IDiceEvent DiceEvent { get; }

        public DiceEventConfig(IDiceEvent eventOwner, TRundownConfig? rundown = default, DiceEventClientConfig? client = null)
        {
            if (rundown is null)
            {
                rundown = new();
            }
            this.DiceEvent = eventOwner;
            this.RundownConfig = rundown;
            this.ClientConfig = client ?? new();
        }

        public void Init()
        {
            this.ClientConfig.Init(this.DiceEvent);
            string path = this.GetRundownConfigPath();

            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(this.RundownConfig, Formatting.Indented));
            }
            else
            {
                this.RundownConfig = JsonConvert.DeserializeObject<TRundownConfig>(File.ReadAllText(path)) ?? this.RundownConfig;
            }

            this.RundownConfig.Init(this.DiceEvent);
            return;
        }

        DiceEventRundownConfig IDiceEventConfig.RundownConfig => this.RundownConfig;
    }
}
