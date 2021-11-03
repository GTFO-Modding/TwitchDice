using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace TwitchDice.Twitch.API
{
    public class TPubSub
    {
        private TwitchPubSub TwitchPubSub;
        private string channelID;

        public event EventHandler<OnBitsArgs> OnBits;
        public event EventHandler<OnRewardArgs> OnReward;
        public event Action OnDisconnected;

        public void Connect(Secrets secrets)
        {
            try
            {
                Log.Debug("[Twitch PubSub] Getting channel ID...");
                string idRegex = "\"id\":\"([0-9]+)\"";
                WebClient webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.Authorization] = $"Bearer {secrets.ImplicitOAuth}";
                webClient.Headers.Add("Client-Id", secrets.ClientID);
                string response = webClient.DownloadString("https://api.twitch.tv/helix/users?login=" + Main.Secret.Channel);
                webClient.Dispose();
                var match = Regex.Match(response, idRegex);
                channelID = match.Groups[1].ToString();
                Log.Debug($"[Twitch PubSub] Channel ID: {channelID}");
            }
            catch(Exception e)
            {
                Log.Error(e);
            }

            Log.Debug("[Twitch PubSub] Creating...");

            TwitchPubSub = new TwitchPubSub();

            //Setup events
            TwitchPubSub.OnLog += TwitchPubSub_OnLog;

            TwitchPubSub.OnPubSubServiceConnected += (sender, e) =>
            {
                Log.Debug("[Twitch PubSub] Sending topics to listen too...");
                TwitchPubSub.ListenToBitsEvents(channelID);
                TwitchPubSub.SendTopics(secrets.ImplicitOAuth);
            };

            TwitchPubSub.OnPubSubServiceError += (sender, e) =>
            {
                Log.Error($"[Twitch PubSub] ERROR: {e.Exception}");
            };

            TwitchPubSub.OnPubSubServiceClosed += (sender, e) =>
            {
                Log.Debug($"[Twitch PubSub] Connection closed");
            };

            TwitchPubSub.OnListenResponse += (sender, e) =>
            {
                if (!e.Successful)
                {
                    Log.Error($"[Twitch PubSub] Failed to listen! Response: {e.Response}");
                }
                else
                {
                    Log.Debug($"[Twitch PubSub] Listening to {e.Topic} - {e.Response}");
                }
            };

            TwitchPubSub.OnRewardRedeemed += OnRewardRedeemed;
            TwitchPubSub.OnBitsReceived += OnBitsReceived;

            Log.Debug("[Twitch PubSub] Connecting...");
            TwitchPubSub.Connect();
        }

        public void Disconnect()
        {
            Log.Debug("[Twitch PubSub] Disconnected");
            TwitchPubSub.Disconnect();
            OnDisconnected?.Invoke();
        }

        private void OnBitsReceived(object sender, OnBitsReceivedArgs e)
        {
            Log.Debug($"[Twitch PubSub] {e.BitsUsed} received");
            OnBits?.Invoke(this, new OnBitsArgs(e.BitsUsed, e.Username));
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            Log.Debug($"[Twitch PubSub] {e.RewardTitle} redeemed");
            OnReward?.Invoke(this, new OnRewardArgs(e.RewardTitle, e.DisplayName));
        }

        private void TwitchPubSub_OnLog(object sender, OnLogArgs e)
        {
            Log.Debug($"[Twitch PubSub] {e.Data}");
        }
    }

    public class OnBitsArgs : EventArgs
    {
        public int Bits;
        public string Username;

        public OnBitsArgs(int bits, string username)
        {
            Bits = bits;
            Username = username;
        }
    }

    public class OnRewardArgs : EventArgs
    {
        public string Reward;
        public string Username;

        public OnRewardArgs(string reward, string username)
        {
            Reward = reward;
            Username = username;
        }
    }
}
