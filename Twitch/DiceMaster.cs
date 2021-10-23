using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using TwitchLib.Client.Models;
using UnityEngine;
using SNetwork;
using Steamworks;

namespace TwitchDice.Twitch
{
    public enum DiceTier
    {
        D3 = 3,
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D12 = 12,
        D20 = 20,
        D50 = 50,
        D100 = 100
    }

    public class DiceMaster : MonoBehaviour
    {
        public DiceMaster(IntPtr intPtr) : base(intPtr) { }

        public TwitchManager TwitchManager;
        private readonly Queue<EventInfo> EventQueue = new Queue<EventInfo>();
        private DiceMasterState _state = DiceMasterState.InLobbySetup;
        private DiceMasterState State
        {
            get
            {
                return _state;
            }
            set
            {
                Log.Debug($"DiceMaster State {_state} => {value}");

                switch(value)
                {
                    case DiceMasterState.InLobby:
                        ChatUtil.DiceMasterSpeak($"READY // {(IsHost ? "<color=orange>HOST</color>" : "<color=orange>CLIENT</color>")} // <color=red>WAITING FOR DROP</color>", false);
                        break;
                    case DiceMasterState.InLevel:
                        ChatUtil.DiceMasterSpeak("IN LEVEL // <color=red>GET READY</color> // ", false);
                        break;
                }

                _state = value;
            }
        }
        private bool? _isHost;
        private bool IsHost
        {
            get
            {
                if (_isHost.HasValue) return _isHost.Value;
                if (SNet.Core.TryGetLobbyOwner(out SNet_Player player))
                {
                    _isHost = player.IsLocal;
                    return _isHost.Value;
                }
                Log.Error("Couldn't get lobby host :(");
                return false;
            }
        }

        void Awake()
        {
            Hooks.OnFail += Hooks_OnFail;
            Hooks.OnLobbyLeave += Hooks_OnLobbyLeave;
            RundownManager.add_OnExpeditionGameplayStarted((Il2CppSystem.Action)RundownManager_OnExpeditionGameplayStarted);

            #region Twitch
            if (IsHost)
            {
                Log.Debug("Player is host, creating twitch connection...");
                if (Main.Instance.TwitchEnabled)
                {
                    StartTwitch();
                } else
                {
                    State = DiceMasterState.InLobby;
                }
            }
            else
            {
                State = DiceMasterState.InLobby;
            }
            #endregion

            #region Debug
            PlayerChatManager.add_OnIncomingChatMessage((Action<SNetwork.SNet_Player, string>)((player, data) =>
            {
                Log.Debug("Incoming chat message");
                if (State != DiceMasterState.InLevel || !IsHost || Main.Instance.TwitchEnabled) return;
                Main.EventManager.TryActivateEvent(data, player.NickName);
            }));
            #endregion
        }

        private void StartTwitch()
        {
            //Check if secret is set
            if (Main.Secret.Channel == string.Empty) ChatUtil.DiceMasterSpeak("<color=red>ERR://</color> Channel name not set");
            if (Main.Secret.Username == string.Empty) ChatUtil.DiceMasterSpeak("<color=red>ERR://</color> Username not set");
            if (Main.Secret.ImplicitOAuth == string.Empty) ChatUtil.DiceMasterSpeak("<color=red>ERR://</color> OAuth not set");
            if (Main.Secret.Channel == string.Empty || Main.Secret.Username == string.Empty || Main.Secret.ImplicitOAuth == string.Empty)
            {
                ChatUtil.DiceMasterSpeak("Unable to start twitch connection");
                ChatUtil.DiceMasterSpeak("Please fix error(s) and restart");
                return;
            }

            try
            {
                TwitchManager = new TwitchManager();
                TwitchManager.OnMessageReceived += TwitchManager_OnMessageReceived;
                TwitchManager.OnConnected += TwitchManager_OnConnected;
                TwitchManager.OnDisconnected += TwitchManager_OnDisconnected;
                TwitchManager.OnRewardRedeemed += TwitchManager_OnRewardRedeemed;
                TwitchManager.OnBitsReceived += TwitchManager_OnBitsReceived;
                TwitchManager.Connect(Main.Secret);
                TwitchManager.OnConnected += TwitchManager_OnConnected;
            } catch(Exception e)
            {
                Log.Error(e);
                ChatUtil.DiceMasterSpeak("Unable to connect to twitch.", true);
            }
        }

        private void Hooks_OnFail()
        {
            EventQueue.Clear();
            State = DiceMasterState.InLobby;
        }

        void Update()
        {
            switch(State)
            {
                case DiceMasterState.InLobby:
                    break;

                case DiceMasterState.InLevel:
                    if (EventQueue.TryDequeue(out EventInfo info))
                    {
                        if (!Main.EventManager.TryActivateEventOfTier(info.Tier, info.ActivatorUsername))
                        {
                            Log.Warning("Failed to activate event!");
                        }
                    }
                    break;

                case DiceMasterState.Disconnect:
                    if (TwitchManager.IsConnected) TwitchManager.Disconnect();
                    ChatUtil.DiceMasterSpeak("NOT ACTIVE // <color=red>DISCONNECTED</color>");
                    Main.DiceMasterObject = null;
                    Destroy(this);
                    break;
            }
        }


        private void Hooks_OnLobbyLeave()
        {
            State = DiceMasterState.Disconnect;
        }

        private void TwitchManager_OnConnected(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            State = DiceMasterState.InLobby;
        }

        private void RundownManager_OnExpeditionGameplayStarted()
        {
            if (State == DiceMasterState.InLobby)
                State = DiceMasterState.InLevel;
        }

        private void TwitchManager_OnRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e)
        {
            Log.Debug("Trying to parse reward into dice...");
            if (Main.Instance.TryGetDiceTier(e.RewardTitle, out Main.DiceTierConfigEntry config))
            {
                EventInfo info = new EventInfo()
                {
                    ActivatorUsername = e.DisplayName,
                    Tier = config.Tier
                };
                EventQueue.Enqueue(info);
                return;
            }
            Log.Debug($"Found no associated tier for reward {e.RewardTitle}");
        }

        private void TwitchManager_OnBitsReceived(object sender, TwitchLib.PubSub.Events.OnBitsReceivedArgs e)
        {
            Log.Debug("Trying to parse reward into dice...");
            if (Main.Instance.TryGetDiceTier(e.BitsUsed, out Main.DiceTierConfigEntry config))
            {
                EventInfo info = new EventInfo()
                {
                    ActivatorUsername = e.Username,
                    Tier = config.Tier
                };
                EventQueue.Enqueue(info);
                return;
            }
            Log.Debug($"{e.BitsUsed} < minimum dice amount");
        }

        private void TwitchManager_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            ChatUtil.DiceMasterSpeak("DISCONNECTED FROM TWITCH // <color=red>NOT ACTIVE</color>");
            State = DiceMasterState.Disconnect;
        }

        private void TwitchManager_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Username != Main.OVERRIDE_NAME) return;

            if (Enum.TryParse(e.ChatMessage.Message, out DiceTier tier))
            {
                EventInfo info = new EventInfo()
                {
                    ActivatorUsername = e.ChatMessage.Username,
                    Tier = tier
                };

                EventQueue.Enqueue(info);
            }
        }

        enum DiceMasterState
        {
            InLobbySetup,
            InLobby,
            InLevel,
            Disconnect
        }

        struct EventInfo
        {
            public string ActivatorUsername;
            public DiceTier Tier;
        }
    }
}
