using System;
using System.Collections.Generic;
using TwitchDice.Utilities;
using UnityEngine;
using SNetwork;
using TwitchDice.Twitch.API;

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
            State = DiceMasterState.InLobbySetup;
            Hooks.OnFail += Hooks_OnFail;
            Hooks.OnLobbyLeave += Hooks_OnLobbyLeave;
            RundownManager.add_OnExpeditionGameplayStarted((Il2CppSystem.Action)RundownManager_OnExpeditionGameplayStarted);

            #region Twitch
            if (IsHost)
            {
                if (Main.Instance.TwitchEnabled)
                {
                    Log.Debug("Starting twitch connection...");
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

            PlayerChatManager.add_OnIncomingChatMessage((Action<SNetwork.SNet_Player, string>)((player, data) =>
            {
#if DEBUG
                if (int.TryParse(data, out int result))
                {
                    if (Main.Instance.TryGetDiceTier(result, out DiceTierConfigEntry config))
                    {
                        Log.Debug(config);
                        EventQueue.Enqueue(new EventInfo() { Tier = config.Tier, ActivatorUsername = "DEBUG" });
                    }
                }
#endif

                if (State != DiceMasterState.InLevel || !IsHost || Main.Instance.TwitchEnabled) return;
                Log.Debug("Incoming chat message");
                Main.EventManager.TryActivateEvent(data, player.NickName);
            }));
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

            //Start twitch
            TwitchManager.OnDisconnected += TwitchManager_OnDisconnected;
            TwitchManager.OnConnected += TwitchManager_OnConnected;
            TwitchManager.OnBits += TwitchManager_OnBitsReceived;
            TwitchManager.OnReward += TwitchManager_OnRewardRedeemed;
            Log.Debug("Before start");
            TwitchManager.Start(Main.Secret);
            Log.Debug("After start");
        }

        #region Events
        private void Hooks_OnLobbyLeave()
        {
            State = DiceMasterState.Disconnect;
        }

        private void TwitchManager_OnConnected()
        {
            State = DiceMasterState.InLobby;
        }

        private void RundownManager_OnExpeditionGameplayStarted()
        {
            if (State == DiceMasterState.InLobby)
                State = DiceMasterState.InLevel;
        }

        private void Hooks_OnFail()
        {
            EventQueue.Clear();
            State = DiceMasterState.InLobby;
        }

        private void TwitchManager_OnDisconnected()
        {
            State = DiceMasterState.Disconnect;
        }
        #endregion

        void Update()
        {
            switch(State)
            {
                case DiceMasterState.InLobby:
                    break;

                case DiceMasterState.InLevel:
                    while(EventQueue.Count > 0)
                    {
                        Log.Debug("Dequeueing event...");
                        EventInfo info = EventQueue.Dequeue();
                        if (!Main.EventManager.TryActivateEventOfTier(info.Tier, info.ActivatorUsername))
                        {
                            Log.Warning("Failed to activate event!");
                        } else
                        {
                            Log.Debug("Activated event");
                        }

                    }
                    break;

                case DiceMasterState.Disconnect:
                    ChatUtil.DiceMasterSpeak("NOT ACTIVE // <color=red>DISCONNECTED</color>");
                    ChatUtil.DiceMasterSpeak("<color=orange>///</color> <b>PLEASE RESTART YOUR GAME</b> <color=orange>///</color>");
                    Main.DiceMasterObject = null;
                    Destroy(this);
                    break;
            }
        }

        private void TwitchManager_OnRewardRedeemed(object sender, OnRewardArgs e)
        {
            Log.Debug("Trying to parse reward into dice...");
            if (Main.Instance.TryGetDiceTier(e.Reward, out DiceTierConfigEntry config))
            {
                EventInfo info = new EventInfo()
                {
                    ActivatorUsername = e.Username,
                    Tier = config.Tier
                };
                EventQueue.Enqueue(info);
                return;
            }
            Log.Debug($"Found no associated tier for reward {e.Reward}");
        }

        private void TwitchManager_OnBitsReceived(object sender, OnBitsArgs e)
        {
            Log.Debug("Trying to parse reward into dice...");
            if (Main.Instance.TryGetDiceTier(e.Bits, out DiceTierConfigEntry config))
            {
                EventInfo info = new EventInfo()
                {
                    ActivatorUsername = e.Username,
                    Tier = config.Tier
                };
                Log.Debug("Added event to event queue");
                EventQueue.Enqueue(info);
                return;
            }
            Log.Debug($"{e.Bits} < minimum dice amount");
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
