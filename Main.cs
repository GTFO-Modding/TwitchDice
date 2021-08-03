using System;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CellMenu;
using HarmonyLib;
using TwitchDice.Components;
using TwitchDice.Twitch;
using TwitchDice.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace TwitchDice
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Main : BasePlugin
    {
        public const string
            NAME = "TwitchDice",
            AUTHOR = "dak",
            VERSION = "1.0.0",
            GUID = "com." + AUTHOR + "." + NAME,
            OVERRIDE_NAME = "dakkhuza",
            EVENT_ID_KEY = "eventId",
            EVENT_INFO_KEY = "eventInfo";

        public static ManualLogSource log;
        public static GameObject DiceMasterObject;
        public static System.Random rnd = new System.Random();

        public override void Load()
        {
            CrashReportHandler.SetUserMetadata("Modded", "true");
            log = Log;
            RegisterMonobehavior();

            var harmony = new Harmony(GUID);
            var chatManagerEntryPoint = typeof(GS_Offline).GetMethod("Enter");
            var chatManager = typeof(Main).GetMethod("CreateChatManager");
            harmony.Patch(chatManagerEntryPoint, null, new HarmonyMethod(chatManager));

            harmony.PatchAll();
            Hooks.OnLobbyStart += Hooks_OnLobbyStart;
        }

        private void Hooks_OnLobbyStart()
        {
            CreateDiceMaster();
        }

        public static void RegisterMonobehavior()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DiceMaster>();
            ClassInjector.RegisterTypeInIl2Cpp<ChatManager>();
            ClassInjector.RegisterTypeInIl2Cpp<DestroyOnCleanUp>();
        }

        public static void CreateDiceMaster()
        {
            if (DiceMasterObject == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.AddComponent<DiceMaster>();
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                DiceMasterObject = gameObject;
                TwitchDice.Log.Message("Created DiceMaster");
            } else
            {
                TwitchDice.Log.Error("Trying to create a DiceMaster when one already exists!");
            }
        }

        public static void CreateChatManager()
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<ChatManager>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            TwitchDice.Log.Message("Created Chat Manager!");
        }
    }
}
