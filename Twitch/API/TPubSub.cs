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
        TwitchPubSub TwitchPubSub;
        string channelID;
        public void Connect(Secrets secrets)
        {
            try
            {
                string idRegex = "\"id\":\"([0-9]+)\"";
                WebClient webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.Authorization] = $"Bearer {secrets.ImplicitOAuth}";
                webClient.Headers.Add("Client-Id", secrets.ClientID);
                string response = webClient.DownloadString("https://api.twitch.tv/helix/users?login=" + Main.Secret.Channel);
                webClient.Dispose();
                var match = Regex.Match(response, idRegex);

                channelID = match.Groups[1].ToString();
            }
            catch(Exception e)
            {
                Log.Error(e);
            }

            Log.Message("[Twitch PubSub] Creating...");
            TwitchPubSub = new TwitchPubSub();
            TwitchPubSub.OnLog += TwitchPubSub_OnLog;
            TwitchPubSub.OnPubSubServiceConnected += (sender, e) =>
            {
                Log.Message("[Twitch PubSub] Sending topics to listen too...");
                TwitchPubSub.ListenToBitsEvents(channelID);
                TwitchPubSub.SendTopics(secrets.ImplicitOAuth);
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

        private void OnBitsReceived(object sender, OnBitsReceivedArgs e)
        {
            Log.Debug("Bits!");
            Log.Debug($"{e.BitsUsed} bits used");
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            Log.Debug("Channel points!");
            Log.Debug($"{e.RewardTitle} | reward");
        }

        private void TwitchPubSub_OnLog(object sender, OnLogArgs e)
        {
            Log.Debug($"[Twitch PubSub] {e.Data}");
        }
    }
}
