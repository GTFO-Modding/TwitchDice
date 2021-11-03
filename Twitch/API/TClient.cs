using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace TwitchDice.Twitch.API
{
    /// <summary>
    /// Original client taken from here
    /// https://github.com/Sid-003/HKTwitch
    /// </summary>
    internal class TClient : IDisposable
    {
        private TcpClient _client;
        private StreamReader _output;
        private StreamWriter _input;

        private readonly Secrets _secret;

        public event EventHandler<ChatMessageArgs> OnMessageReceived;
        public event Action<string> RawPayload;

        public event Action<string> ClientErrored;

        public TClient(Secrets secret)
        {
            Log.Debug("[Twitch Client] Created");
            _secret = secret;
            ConnectAndAuthenticate(secret);
            RawPayload += ProcessMessage;
        }

        private void ConnectAndAuthenticate(Secrets secret)
        {
            Log.Debug("[Twitch Client] Connecting to twitch...");
            _client = new TcpClient("irc.twitch.tv", 6667);

            _output = new StreamReader(_client.GetStream());
            _input = new StreamWriter(_client.GetStream())
            {
                AutoFlush = true
            };

            if (!_client.Connected)
            {
                Reconnect(10000);
                return;
            }

            SendMessage($"PASS oauth:{secret.ImplicitOAuth}");
            SendMessage($"NICK {secret.Username}");
            SendMessage($"JOIN #{secret.Channel}");
            Log.Debug("[Twitch Client] Connected!");
        }

        private void Reconnect(int delay)
        {
            //ClientErrored?.Invoke("[Twitch Client] Reconnecting...");
            Log.Debug("[Twitch Client] Reconnecting...");
            Dispose();
            Thread.Sleep(delay);
            ConnectAndAuthenticate(_secret);
        }

        private void ProcessMessage(string message)
        {
            if (message == null)
                return;
            if (message.Contains("PING"))
            {
                SendMessage("PONG :tmi.twitch.tv");
                Log.Debug("[Twitch Client] Pong!");
            }
            else if (message.Contains("PRIVMSG"))
            {
                string user = message.Substring(1, message.IndexOf("!") - 1);
                string cleaned = message.Split(':').Last();

                Log.Debug($"[Twitch Client] {user} : {cleaned}");
                OnMessageReceived?.Invoke(this, new ChatMessageArgs() { User = user, Message = cleaned });
            }
        }

        public void StartReceive()
        {
            while (true)
            {
                try
                {
                    if (!_client.Connected)
                    {
                        Dispose();
                        ConnectAndAuthenticate(_secret);
                    }

                    string message = _output.ReadLine();
                    RawPayload?.Invoke(message);
                }
                catch (Exception e)
                {
                    Log.Error($"[Twitch Client] {e}");
                    ClientErrored?.Invoke("Error occured trying to read stream: " + e);
                    Reconnect(5000);
                }

            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void SendMessage(string message) => _input.WriteLine(message);

        public void Dispose()
        {
            _input.Dispose();
            _output.Dispose();
            _client.Close();
        }
    }

    public class ChatMessageArgs : EventArgs
    {
        public string User;
        public string Message;
    }
}