using System;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using TwitchDice.Components;
using TwitchDice.Twitch;
using TwitchDice.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using System.Collections.Generic;
using System.Linq;
using Nidhogg.Managers;
using System.Runtime.InteropServices;

namespace TwitchDice
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.kasuromi.nidhogg", BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BasePlugin
    {
        public const string
            NAME = "TwitchDice",
            AUTHOR = "dak",
            VERSION = "1.0.0",
            GUID = "com." + AUTHOR + "." + NAME,
            OVERRIDE_NAME = "dakkhuza",
            EVENT_ID_KEY = "eventId",
            EVENT_INFO_KEY = "eventInfo",
            CONFIG_TWITCH_SECTION = "Twitch",
            CONFIG_DICE_SECTION = "Dice Tiers"
            ;

        public const bool DEBUG = true;

        public static ManualLogSource log;
        public static Main Instance;
        public static GameObject DiceMasterObject;
        public static EventManager EventManager;
        public static System.Random rnd = new System.Random();

        public override void Load()
        {
            CrashReportHandler.SetUserMetadata("Modded", "true");
            Instance = this;
            log = Log;
            RegisterMonobehavior();

            var harmony = new Harmony(GUID);
            //var chatManagerEntryPoint = typeof(GS_Offline).GetMethod("Enter");
            //var chatManager = typeof(Main).GetMethod("CreateChatManager");
            //
            //harmony.Patch(chatManagerEntryPoint, new HarmonyMethod(chatManager));
            harmony.PatchAll();

            Hooks.OnLobbyStart += Hooks_OnLobbyStart;

            NetworkingManager.RegisterEvent<ChatMsg>(typeof(ChatMsg).Name, OnMessage);
            EventManager = new EventManager();
        }

        private void OnMessage(ulong sender, ChatMsg message)
        {
            ChatUtil.Send(message.Message, (eGameEventChatLogType)message.LogType);
        }

        private void RegisterMonobehavior()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DiceMaster>();
            ClassInjector.RegisterTypeInIl2Cpp<ChatManager>();
            ClassInjector.RegisterTypeInIl2Cpp<DestroyOnCleanUp>();
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

        public void CreateChatManager()
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<ChatManager>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            TwitchDice.Log.Message("Created Chat Manager!");
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
