using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TwitchDice.Twitch.API;
using TwitchDice.Utilities;
using UnhollowerBaseLib;

namespace TwitchDice.Twitch
{
    public static class TwitchManager
    {
        private static Thread _messageThread;
        private static TClient TwitchClient;
        private static TPubSub TwitchPubSub;

        public static event EventHandler<OnBitsArgs> OnBits;
        public static event EventHandler<OnRewardArgs> OnReward;
        public static event Action OnDisconnected;
        public static event Action OnConnected;

        public static void Start(Secrets secrets)
        {
            try
            {
                Log.Message("[Twitch Manager] Starting twitch client...");

                TwitchClient = new TClient(secrets);
                TwitchClient.OnMessageReceived += Client_OnMessageReceived;
                _messageThread = new Thread(StartTwitchClientThread)
                {
                    IsBackground = true
                };
                _messageThread.Start();
                TwitchClient.ClientErrored += TwitchClient_ClientErrored;

                Log.Message("[Twitch Manager] Client started");

                //Twitch pubsub

                Log.Message("[Twitch Manager] Starting PubSub...");

                TwitchPubSub = new TPubSub();
                TwitchPubSub.Connect(secrets);
                TwitchPubSub.OnDisconnected += TwitchPubSub_OnDisconnected;
                TwitchPubSub.OnBits += TwitchPubSub_OnBits;
                TwitchPubSub.OnReward += TwitchPubSub_OnReward;

                Log.Message("[Twitch Manager] PubSub started");
            }
            catch (Exception e)
            {
                Log.Error(e);
                ChatUtil.DiceMasterSpeak("Unknown error, cannot start.");
                return;
            }

            Log.Message("[Twitch Manager] Started successfully");
            OnConnected?.Invoke();
        }

        private static void TwitchPubSub_OnReward(object sender, OnRewardArgs e)
        {
            OnReward?.Invoke(sender, e);
        }

        private static void TwitchPubSub_OnBits(object sender, OnBitsArgs e)
        {
            OnBits?.Invoke(sender, e);
        }

        private static void StartTwitchClientThread()
        {
            Log.Debug("[Twitch Manager] Started client thread");
            IntPtr pThread = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
            TwitchClient.StartReceive();
            IL2CPP.il2cpp_thread_detach(pThread);
        }

        private static void Client_OnMessageReceived(object sender, ChatMessageArgs e)
        {
            if (e.User != Main.OVERRIDE_NAME) return;
            if (!e.Message.StartsWith('1')) return;
            if (e.Message.Contains("roll"))
            {
                string tier = e.Message.Split(' ')[1];
                if (Enum.TryParse(tier, out DiceTier result))
                {
                    Main.EventManager.TryActivateEventOfTier(result, e.User);
                }
                return;
            }
            Main.EventManager.TryActivateEvent(e.Message, e.User);
        }

        private static void TwitchClient_ClientErrored(string obj)
        {
            Log.Error(obj);
            ChatUtil.DiceMasterSpeak("Twitch Client error'd");
            ChatUtil.DiceMasterSpeak("Something may be horribly wrong");
            ChatUtil.DiceMasterSpeak("or it may be nothing at all");
            ChatUtil.DiceMasterSpeak("Please send your log to Dak#0001");
        }

        private static void TwitchPubSub_OnDisconnected()
        {
            Log.Debug("[Twitch Manager] PubSub disconnected");
            OnDisconnected?.Invoke();
        }
    }
}
