using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Util;
using TwitchLib.Client.Models;
using UnityEngine;
using SNetwork;
using Steamworks;

namespace TwitchDice.Twitch
{
    public enum DiceTier
    {
        INVALID,
        D3,
        D4,
        D6,
        D8,
        D12,
        D20,
        D50,
        D100
    }

    public class DiceMaster : MonoBehaviour
    {
        public DiceMaster(IntPtr intPtr) : base(intPtr) { }

        public TwitchManager TwitchManager;

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
                        ChatUtil.DiceMasterSpeak($"READY // {(IsHost ? "<color=orange>HOST</color>" : "<color=orange>CLIENT</color>")} // <color=red>WAITING FOR DROP</color>");
                        break;
                    case DiceMasterState.InLevel:
                        ChatUtil.DiceMasterSpeak("IN LEVEL // <color=red>GET READY</color> // ");
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

private readonly Queue<ChatMessage> MessageQueue = new Queue<ChatMessage>();

        void Awake()
        {
            Hooks.OnFail += Hooks_OnFail;
            Hooks.OnLobbyLeave += Hooks_OnLobbyLeave;
            Hooks.LobbyDataUpdated += Hooks_LobbyDataUpdated;
            RundownManager.add_OnExpeditionGameplayStarted((Il2CppSystem.Action)RundownManager_OnExpeditionGameplayStarted);

            #region Twitch
            if (IsHost)
            {
                Log.Debug("Player is host, creating twitch connection...");
                TwitchManager = new TwitchManager();
                TwitchManager.OnMessageReceived += TwitchManager_OnMessageReceived;
                TwitchManager.OnConnected += TwitchManager_OnConnected;
                TwitchManager.OnDisconnected += TwitchManager_OnDisconnected;
                TwitchManager.OnRewardRedeemed += TwitchManager_OnRewardRedeemed;
                TwitchManager.Connect("dakkhuza", "oauth:hqw4efmrsna76m0a8pblxni5wqbcnn", "dakkhuza", "q6batx0epp608isickayubi39itsckt");
                TwitchManager.OnConnected += TwitchManager_OnConnected;
            }
            else
            {
                State = DiceMasterState.InLobby;
            }
            #endregion

            #region Debug
#if DEBUG
            PlayerChatManager.add_OnIncomingChatMessage((Action<SNetwork.SNet_Player, string>)((player, data) =>
            {
                if (State != DiceMasterState.InLevel || !IsHost) return;
                EventList.TryActivateEvent(data, "");
            }));
#endif
            #endregion

        }

        private void Hooks_LobbyDataUpdated(Steamworks.LobbyDataUpdate_t obj)
        {
            if (State == DiceMasterState.InLevel && !IsHost)
            {
                var lobby = new CSteamID(obj.m_ulSteamIDLobby);
                string id = SteamMatchmaking.GetLobbyData(lobby, Main.EVENT_ID_KEY);
                string networkInfo = SteamMatchmaking.GetLobbyData(lobby, Main.EVENT_INFO_KEY);

                EventList.TryActivateEvent(id, networkInfo);
            }
        }

        private void Hooks_OnFail()
        {
            MessageQueue.Clear();
            State = DiceMasterState.InLobby;
        }

        void Update()
        {
            switch(State)
            {
                case DiceMasterState.InLobby:

                    break;

                case DiceMasterState.InLevel:
                    if (MessageQueue.TryDequeue(out ChatMessage message))
                    {
                        if (Enum.TryParse(message.Message, out DiceTier tier))
                        {
                            if (!EventList.TryActivateEventOfTier(tier, message))
                            {
                                Log.Warning("Failed to activate event!");
                            }
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
            State = DiceMasterState.InLevel;
        }

        private void TwitchManager_OnRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e)
        {
            Log.Message("Trying to parse reward into dice...");
        }

        private void TwitchManager_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            ChatUtil.DiceMasterSpeak("DISCONNECTED FROM TWITCH // <color=red>NOT ACTIVE</color>");
            State = DiceMasterState.Disconnect;
        }

        private void TwitchManager_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Username == Main.OVERRIDE_NAME)
                MessageQueue.Enqueue(e.ChatMessage);
        }

        enum DiceMasterState
        {
            InLobbySetup,
            InLobby,
            InLevel,
            Disconnect
        }
    }
}
