using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using TwitchDice.Components;
using TwitchDice.Twitch;
using TwitchDice.Utilities;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using System.Runtime.InteropServices;
using BepInEx.Configuration;
using GTFO.API;
using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Injection;
using System.Linq;

namespace TwitchDice
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Main : BasePlugin
    {
        #region String consts
        public const string
            NAME = "TwitchDice",
            AUTHOR = "dak",
            VERSION = "1.0.0",
            GUID = "com." + AUTHOR + "." + NAME,
            OVERRIDE_NAME = "DakKhuza",

            CONFIG_TWITCH_SECTION = "Twitch",
            CONFIG_TWITCH_CHANNEL_KEY = "Channel",
            CONFIG_TWITCH_CHANNEL_DESC = "The name of the twitch channel to connect to",

            CONFIG_TWITCH_USERNAME_KEY = "Username",
            CONFIG_TWITCH_USERNAME_DESC = "The username to login as",

            CONFIG_TWITCH_IMPLICITOAUTH_KEY = "OAuth",
            CONFIG_TWITCH_IMPLICITOAUTH_DESC = "The OAuth token for the user",

            CONFIG_TWITCH_ENABLED_KEY = "Enable Twitch Connection",
            CONFIG_TWITCH_ENABLED_DESC = "Toggles if the mod should connect to twitch, used for testing events",

            CONFIG_TWITCH_DICETIER_SECTION = "Dice tier costs & rewards",

            CONFIG_TWITCH_DICETIER_BITS_KEY_FORMAT = "{0} bit cost",
            CONFIG_TWITCH_DICETIER_CHANNELPOINTS_KEY_FORMAT = "{0} channel reward",

            CONFIG_TWITCH_DICETIER_BITS_DESC_FORMAT = "The cost in bits for events of tier {0} to activate",
            CONFIG_TWITCH_DICETIER_CHANNELPOINTS_DESC_FORMAT = "The name of the channel reward that should trigger an event of tier {0}",

            CONFIG_EVENTS_SECTION = "Enabled Events",
            CONFIG_DICE_SECTION = "Event Tiers",
            
            COLOR_D3 = "white",
            COLOR_D4 = "#ffe0e0",
            COLOR_D6 = "#ffc1c1",
            COLOR_D8 = "#ffa2a2",
            COLOR_D12 = "#ff8383",
            COLOR_D20 = "#ff6464",
            COLOR_D50 = "#ff4545",
            COLOR_D100 = "red";
        #endregion

        public static ManualLogSource log;
        public static Main Instance;
        public static GameObject DiceMasterObject;
        public static EventManager EventManager;
        public static System.Random rnd = new System.Random();
        public static Secrets Secret;

        public string Channel => configChannel.Value;
        public string Username => configUsername.Value;
        public string ImplicitOAuth => configImplicitOAuth.Value;
        public bool TwitchEnabled => configTwitchEnabled.Value;

        //Config
        private ConfigEntry<string> configChannel;
        private ConfigEntry<string> configUsername;
        private ConfigEntry<string> configImplicitOAuth;
        private ConfigEntry<bool> configTwitchEnabled;
        private List<DiceTierConfigEntry> TierConfigs = new List<DiceTierConfigEntry>();

        public override void Load()
        {
            CrashReportHandler.SetUserMetadata("Modded", "true");
            Instance = this;
            log = Log;

            RegisterMonoBehavior();
            SetupConfig();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            Hooks.OnLobbyStart += Hooks_OnLobbyStart;
            NetworkAPI.RegisterEvent<ChatMsg>(typeof(ChatMsg).Name, OnMessage);

            EventManager = new EventManager();
            EnemyRespawnManager.Init();
            ResourceLoader.Init();
        }

        public bool TryGetDiceTier(int bit, out DiceTierConfigEntry config)
        {
            Log.LogDebug($"Getting event tier for {bit} bits...");
            config = null;
            foreach (DiceTierConfigEntry entry in TierConfigs)
            {
                if (config == null)
                {
                    Log.LogDebug("Tier unset");
                } else
                {
                    Log.LogDebug($"Tier {config.Tier}");
                }
                if (bit <= entry.BitAmount)
                {
                    continue; // continue if the bit amount is to big
                }

                config = entry;
            }
            return config != null;
        }

        public bool TryGetDiceTier(string channelReward, out DiceTierConfigEntry config)
        {
            config = TierConfigs.Find(t => t.ChannelReward == channelReward);
            return config != null;
        }

        private void SetupConfig()
        {
            configChannel = Config.Bind(CONFIG_TWITCH_SECTION, CONFIG_TWITCH_CHANNEL_KEY, "", CONFIG_TWITCH_CHANNEL_DESC);
            configUsername = Config.Bind(CONFIG_TWITCH_SECTION, CONFIG_TWITCH_USERNAME_KEY, "", CONFIG_TWITCH_USERNAME_DESC);
            configImplicitOAuth = Config.Bind(CONFIG_TWITCH_SECTION, CONFIG_TWITCH_IMPLICITOAUTH_KEY, "", CONFIG_TWITCH_IMPLICITOAUTH_DESC);
            configTwitchEnabled = Config.Bind(CONFIG_TWITCH_SECTION, CONFIG_TWITCH_ENABLED_KEY, true, CONFIG_TWITCH_ENABLED_DESC);
            Secret = new Secrets()
            {
                Channel = configChannel.Value,
                Username = configUsername.Value,
                ImplicitOAuth = configImplicitOAuth.Value
            };

            foreach (var _tier in Enum.GetValues(typeof(DiceTier)))
            {
                DiceTier tier = (DiceTier)_tier;
                TierConfigs.Add(new DiceTierConfigEntry(tier, Config));
            }

            // Sort by dice tier
            TierConfigs = TierConfigs.OrderByDescending(x => (int)x.Tier).ToList();
        }

        private void OnMessage(ulong sender, ChatMsg message)
        {
            ChatUtil.Send(message.Message, (eGameEventChatLogType)message.LogType);
        }

        private void RegisterMonoBehavior()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DiceMaster>();
            ClassInjector.RegisterTypeInIl2Cpp<ChatManager>();
            ClassInjector.RegisterTypeInIl2Cpp<DestroyOnCleanUp>();
            ClassInjector.RegisterTypeInIl2Cpp<EventTimerManager>();
            ClassInjector.RegisterTypeInIl2Cpp<EventTimer>();
            ClassInjector.RegisterTypeInIl2Cpp<NoiseMaker>();

            InterfaceInjector.InjectWithInterface<SnowmanAI>();

            CoroutineHandler.Init();
        }


        private void Hooks_OnLobbyStart()
        {
            CreateDiceMaster();
            CreateChatManager();
            Hooks.OnLobbyStart -= Hooks_OnLobbyStart;
        }

        public void CreateDiceMaster()
        {
            if (DiceMasterObject == null)
            {
                DiceMasterObject = new GameObject
                {
                    name = "DICE MASTER"
                };
                UnityEngine.Object.DontDestroyOnLoad(DiceMasterObject);

                DiceMasterObject.AddComponent<DiceMaster>();
                DiceMasterObject.AddComponent<EventTimerManager>();
                DiceMasterObject.AddComponent<NoiseMaker>();


                TwitchDice.Log.Message("Created DiceMaster");
            } else
            {
                TwitchDice.Log.Error("Trying to create a DiceMaster when one already exists!");
            }
        }

        public void CreateChatManager()
        {
            GameObject gameObject = new GameObject
            {
                name = "CHAT MANAGER"
            };
            gameObject.AddComponent<ChatManager>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            TwitchDice.Log.Message("Created Chat Manager!");
        }

        public class DiceTierConfigEntry
        {
            public DiceTierConfigEntry(DiceTier tier, ConfigFile config)
            {
                string bitsKey = string.Format(CONFIG_TWITCH_DICETIER_BITS_KEY_FORMAT, tier);
                string bitsDesc = string.Format(CONFIG_TWITCH_DICETIER_BITS_DESC_FORMAT, tier);

                string rewardKey = string.Format(CONFIG_TWITCH_DICETIER_CHANNELPOINTS_KEY_FORMAT, tier);
                string rewardDesc = string.Format(CONFIG_TWITCH_DICETIER_CHANNELPOINTS_DESC_FORMAT, tier);

                _bitAmount = config.Bind(CONFIG_TWITCH_DICETIER_SECTION, bitsKey, (int)tier * 10, bitsDesc);
                _channelReward = config.Bind(CONFIG_TWITCH_DICETIER_SECTION, rewardKey, "", rewardDesc);
                Tier = tier;
            }
            public int BitAmount { get => _bitAmount.Value; }
            public string ChannelReward { get => _channelReward.Value; }
            public DiceTier Tier { get; private set; }

            private readonly ConfigEntry<int> _bitAmount;
            private readonly ConfigEntry<string> _channelReward;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ChatMsg
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string Message;
        public int LogType;
    }
}
