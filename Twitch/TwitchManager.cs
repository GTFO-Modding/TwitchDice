using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using System.Net.Http;
using TwitchDice.Twitch;

namespace TwitchDice
{
    //

    /// <summary>
    /// Small wrapper around <c>TwitchLib</c> to help organize Twitch events
    /// </summary>
    public class TwitchManager
    {
        public const string Credit = "Shamelessly yoinked from here https://github.com/JustDerb/RoR2-VsTwitch/blob/30ba6012074e39a8a364c89501ac4d4402dd9aab/Twitch/TwitchManager.cs";

        private TwitchClient TwitchClient = null;
        private TwitchAPI TwitchApi = null;
        private TwitchPubSub TwitchPubSub = null;

        private string Channel;
        public string Username { get; private set; }

        public bool DebugLogs { get; set; }

        public event EventHandler<OnMessageReceivedArgs> OnMessageReceived;
        public event EventHandler<OnRewardRedeemedArgs> OnRewardRedeemed;
        public event EventHandler<OnJoinedChannelArgs> OnConnected;
        public event EventHandler<OnDisconnectedEventArgs> OnDisconnected;
        public event EventHandler<OnBitsReceivedArgs> OnBitsReceived;

        public TwitchManager()
        {
            DebugLogs = false;
        }

        public void Mock_BitsReceived(string username, int bits)
        {
            var arg = new OnBitsReceivedArgs()
            {
                Username = username,
                BitsUsed = bits
            };
            OnBitsReceived?.Invoke(this, arg);
        }

        public void Mock_RewardRedeemed(string username, string rewardName)
        {
            var arg = new OnRewardRedeemedArgs()
            {
                DisplayName = username,
                RewardTitle = rewardName
            };
            OnRewardRedeemed?.Invoke(this, arg);
        }

        public void Connect(Secrets secrets)
        {
            Connect(secrets.Channel, secrets.ImplicitOAuth, secrets.Username, secrets.ClientID);
        }

        public void Connect(string channel, string oauthToken, string username, string clientId)
        {
            var http = new HttpClient();
            Disconnect();
            Log.Message("TwitchManager::Connect");

            if (channel == null || channel.Trim().Length == 0)
            {
                throw new ArgumentException("Twitch channel must be specified!", "channel");
            }
            if (oauthToken == null || oauthToken.Trim().Length == 0)
            {
                throw new ArgumentException("Twitch OAuth password must be specified!", "oauthToken");
            }
            if (username == null || username.Trim().Length == 0)
            {
                throw new ArgumentException("Twitch username must be specified!", "username");
            }

            Channel = channel;
            Username = username;

            Log.Message("[Twitch API] Creating...");
            TwitchApi = new TwitchAPI();
            string twitchApiOauthToken = oauthToken;
            if (twitchApiOauthToken.StartsWith("oauth:"))
            {
                twitchApiOauthToken = twitchApiOauthToken.Substring("oauth:".Length);
            }
            TwitchApi.Settings.AccessToken = twitchApiOauthToken;
            TwitchApi.Settings.ClientId = clientId;
            string channelId = null;
            try
            {
                Log.Message("[Twitch API] Trying to find channel ID...");
                Task<TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse> response = 
                TwitchApi.Helix.Users.GetUsersAsync(null, new List<string>(new string[] { channel }));
                response.Wait();
                
                if (response.Result.Users.Length == 1)
                {
                    channelId = response.Result.Users[0].Id;
                    Log.Message($"[Twitch API] Channel ID for {channel} = {channelId}");
                }
                else
                {
                    throw new ArgumentException($"Couldn't find Twitch user/channel {channel}!");
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    throw ex;
                }
                Console.WriteLine(ex);
            }

            Log.Message("[Twitch Client] Creating...");
            ConnectionCredentials credentials = new ConnectionCredentials(username, oauthToken);
            TwitchClient = new TwitchClient();
            TwitchClient.Initialize(credentials, channel);
            TwitchClient.OnLog += TwitchClient_OnLog;
            TwitchClient.OnJoinedChannel += OnConnected;
            TwitchClient.OnMessageReceived += OnMessageReceived;
            TwitchClient.OnConnected += TwitchClient_OnConnected;
            TwitchClient.OnDisconnected += OnDisconnected;
            Log.Message("[Twitch Client] Connecting...");
            TwitchClient.Connect();

            if (channelId != null && channelId.Trim().Length != 0)
            {
                Log.Message("[Twitch PubSub] Creating...");
                TwitchPubSub = new TwitchPubSub();
                TwitchPubSub.OnLog += TwitchPubSub_OnLog;
                TwitchPubSub.OnPubSubServiceConnected += (sender, e) =>
                {
                    Log.Message("[Twitch PubSub] Sending topics to listen too...");
                    TwitchPubSub.ListenToBitsEvents(channelId);
                    TwitchPubSub.SendTopics(twitchApiOauthToken);
                };
                TwitchPubSub.OnPubSubServiceError += (sender, e) =>
                {
                    Log.Error($"[Twitch PubSub] ERROR: {e.Exception}");
                };
                TwitchPubSub.OnPubSubServiceClosed += (sender, e) =>
                {
                    Log.Message($"[Twitch PubSub] Connection closed");
                };
                TwitchPubSub.OnListenResponse += (sender, e) =>
                {
                    if (!e.Successful)
                    {
                        Log.Error($"[Twitch PubSub] Failed to listen! Response: {e.Response}");
                    }
                    else
                    {
                        Log.Message($"[Twitch PubSub] Listening to {e.Topic} - {e.Response}");
                    }
                };
                TwitchPubSub.OnRewardRedeemed += OnRewardRedeemed;
                TwitchPubSub.OnBitsReceived += OnBitsReceived;
                Log.Message("[Twitch PubSub] Connecting...");
                TwitchPubSub.Connect();
            }
        }

        public void Disconnect()
        {
            Log.Message("TwitchManager::Disconnect");
            if (TwitchClient != null)
            {
                TwitchClient.Disconnect();
                TwitchClient = null;
            }
            if (TwitchPubSub != null)
            {
                TwitchPubSub.Disconnect();
                TwitchPubSub = null;
            }
            if (TwitchApi != null)
            {
                TwitchApi = null;
            }
        }


        public bool IsConnected { get { return TwitchClient != null && TwitchClient.IsConnected; } }

        private void TwitchClient_OnConnected(object sender, OnConnectedArgs e)
        {
            Log.Debug("[Twitch Client] Connected to Twitch using username: " + e.BotUsername);
        }

        private void TwitchPubSub_OnLog(object sender, TwitchLib.PubSub.Events.OnLogArgs e)
        {
            Log.Debug($"[Twitch PubSub] {e.Data}");
        }

        private void TwitchClient_OnLog(object sender, TwitchLib.Client.Events.OnLogArgs e)
        {
            Log.Debug($"[Twitch Client] {e.DateTime}: {e.BotUsername} - {e.Data}");
        }
    }
}
