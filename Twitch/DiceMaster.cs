using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using TwitchLib.Client.Models;
using UnityEngine;
using SNetwork;
using Steamworks;
using System.Collections;
using TwitchDice.Components;

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
        /// <summary>
        /// Delay between event activations
        /// </summary>
        private double EventCooldown = 0;
        private const double EventCooldownTime = 15;
        private DiceMasterState State
        {
            get
            {
                return this._state;
            }
            set
            {
                Log.Debug($"DiceMaster State {this._state} => {value}");

                switch(value)
                {
                    case DiceMasterState.InLobby:
                        ChatUtil.DiceMasterSpeak($"READY // {(this.IsHost ? "<color=orange>HOST</color>" : "<color=orange>CLIENT</color>")} // <color=red>WAITING FOR DROP</color>", false);
                        break;
                    case DiceMasterState.InLevel:
                        ChatUtil.DiceMasterSpeak("IN LEVEL // <color=red>GET READY</color> // ", false);
                        break;
                }

                this._state = value;
            }
        }
        private bool? _isHost;
        private bool IsHost
        {
            get
            {
                if (this._isHost.HasValue) return this._isHost.Value;
                if (SNet.Core.TryGetLobbyOwner(out SNet_Player player))
                {
                    this._isHost = player.IsLocal;
                    return this._isHost.Value;
                }
                Log.Error("Couldn't get lobby host :(");
                return false;
            }
        }
        private bool TwitchManagerConnected => this.TwitchManager != null && this.TwitchManager.IsConnected;

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
            PlayerChatManager.add_OnIncomingChatMessage((Action<string, SNetwork.SNet_Player, SNetwork.SNet_Player>)((data, player, _) =>
            {
                Log.Debug("Incoming chat message");

                if (data.StartsWith('.'))
                {
                    string bitValue = data[1..];
                    Log.Debug(bitValue);
                    if (int.TryParse(bitValue, out int bits))
                    {
                        Log.Verbose($"Parsed as {bits}");
                        TwitchManager?.Mock_BitsReceived(player.NickName, bits);
                    }
                    return;
                }

                if (State != DiceMasterState.InLevel || !this.IsHost || Main.Instance.TwitchEnabled)
                {
                    Log.Debug("Skipped activation");
                    return;
                }

                if (!Main.EventManager.TryActivateEvent(data, player.NickName))
                {
                    Log.Warning($"Unable to find event with ID {data}");
                }
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
                this.TwitchManager = new TwitchManager();
                this.TwitchManager.OnMessageReceived += this.TwitchManager_OnMessageReceived;
                this.TwitchManager.OnConnected += this.TwitchManager_OnConnected;
                this.TwitchManager.OnDisconnected += this.TwitchManager_OnDisconnected;
                this.TwitchManager.OnRewardRedeemed += this.TwitchManager_OnRewardRedeemed;
                this.TwitchManager.OnBitsReceived += this.TwitchManager_OnBitsReceived;
                this.TwitchManager.Connect(Main.Secret);
                this.TwitchManager.OnConnected += this.TwitchManager_OnConnected;
            } catch(Exception e)
            {
                Log.Error(e);
                ChatUtil.DiceMasterSpeak("Unable to connect to twitch.", true);
            }
        }

        private void Hooks_OnFail()
        {
            this.EventQueue.Clear();
            this.State = DiceMasterState.InLobby;
        }

        void Update()
        {
            switch(this.State)
            {
                case DiceMasterState.InLobby:
                    EventCooldown = 5;
                    break;

                case DiceMasterState.InLevel:
                    while(this.EventQueue.Count > 0 && EventCooldown <= 0)
                    {
                        Log.Debug("Dequeueing event...");
                        EventInfo info = this.EventQueue.Dequeue();
                        if (Main.EventManager.TryGetEventOfTier(info.Tier, out IDiceEvent? diceEvent))
                        {
                            DiceActivationAnimator.QueueAnimation(diceEvent, info.ActivatorUsername);
                            EventManager.ActivateEvent(diceEvent, info.ActivatorUsername);
                            Log.Debug("Activated event");
                            
                        } else Log.Warning("Failed to activate event!");
                        EventCooldown = EventCooldownTime;
                    }
                    if (EventCooldown > 0)
                    {
                        EventCooldown -= 1 * Time.deltaTime;
                    }
                    break;

                case DiceMasterState.Disconnect:
                    if (this.TwitchManagerConnected) this.TwitchManager.Disconnect();
                    ChatUtil.DiceMasterSpeak("NOT ACTIVE // <color=red>DISCONNECTED</color>");
                    Main.DiceMasterObject = null;
                    Destroy(this);
                    break;
            }
        }

        private IEnumerator RollDieOfTier(DiceTier tier)
        {
            yield break;
        }
        private void Hooks_OnLobbyLeave()
        {
            this.State = DiceMasterState.Disconnect;
        }

        private void TwitchManager_OnConnected(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            this.State = DiceMasterState.InLobby;
        }

        private void RundownManager_OnExpeditionGameplayStarted()
        {
            if (this.State == DiceMasterState.InLobby)
                this.State = DiceMasterState.InLevel;
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
                this.EventQueue.Enqueue(info);
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
                Log.Debug("Added event to event queue");
                this.EventQueue.Enqueue(info);
                return;
            }
            Log.Debug($"{e.BitsUsed} < minimum dice amount");
        }

        private void TwitchManager_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            ChatUtil.DiceMasterSpeak("DISCONNECTED FROM TWITCH // <color=red>NOT ACTIVE</color>");
            this.State = DiceMasterState.Disconnect;
        }

        private void TwitchManager_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            Log.Debug(e.ChatMessage.UserId);
            if (e.ChatMessage.Username != Main.OVERRIDE_NAME) return;

            if (Enum.TryParse(e.ChatMessage.Message, out DiceTier tier))
            {
                EventInfo info = new EventInfo()
                {
                    ActivatorUsername = e.ChatMessage.Username,
                    Tier = tier
                };

                this.EventQueue.Enqueue(info);
                return;
            }

            Main.EventManager.TryActivateEvent(e.ChatMessage.Message);
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
