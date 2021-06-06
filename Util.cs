using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CellMenu;
using TwitchDice.Twitch;
using UnityEngine;
using SNetwork;
using Player;

namespace TwitchDice.Util
{
    public class ChatManager : MonoBehaviour
    {
        public ChatManager(IntPtr intPtr) : base(intPtr) { }

        void Update()
        {
            if (!ChatUtil.MessageQueue.TryDequeue(out MessageQueueInfo messageInfo)) return;
            Send(messageInfo.message, messageInfo.chatLogType);
        }

        public static void Send(string message, eGameEventChatLogType chatLogType = eGameEventChatLogType.GameEvent)
        {
            GuiManager.PlayerLayer.m_gameEventLog.AddLogItem(message, chatLogType);
            CM_PageLoadout.Current.m_gameEventLog.AddLogItem(message, chatLogType);
            CM_PageMap.Current.m_gameEventLog.AddLogItem(message, chatLogType);
        }
    }

    public static class PlayerUtil
    {
        public static int PlayerCount
        {
            get
            {
                return PlayerManager.PlayerAgentsInLevel.Count;
            }
        }
        public static bool TryGetRandomPlayerAgent(out PlayerAgent playerAgent, bool IncludeHost = true)
        {
            playerAgent = null;
            var list = new List<PlayerAgent>();
            foreach (var item in PlayerManager.PlayerAgentsInLevel)
            {
                if (IncludeHost == true)
                    list.Add(item);
                else
                {
                    if (!item.IsLocallyOwned)
                        list.Add(item);
                }
            }

            try
            {
                playerAgent = list.GetRandomElement<PlayerAgent>();
                return true;
            } catch(Exception e) { Log.Error(e); }

            return false;
        }
    }

    public static class GOUtil
    {

    }

    public static class NetworkUtil
    {
        /// <summary>
        /// Returns the SNet player object of a random player in the lobby
        /// </summary>
        /// <param name="playerOut"></param>
        /// <returns></returns>
        public static bool TryGetRandomNetworkPlayer(out SNet_Player playerOut)
        {
            var playerList = new List<SNet_Player>();
            playerOut = null;
            foreach (var player in SNet.Lobby.Players)
            {
                playerList.Add(player);
            }
            if (playerList.Count > 0)
            {
                playerOut = playerList.GetRandomElement<SNet_Player>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the lookup ID of a random player in the lobby
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool TryGetRandomNetworkPlayerLookup(out ulong id)
        {
            id = 0;
            if (TryGetRandomNetworkPlayer(out SNet_Player player))
            {
                id = player.Lookup;
                return true;
            }
            return false;
        }
    }

    public struct MessageQueueInfo
    {
        public string message;
        public eGameEventChatLogType chatLogType;
    }

    public struct JsonVector
    {
        public float x;
        public float y;
        public float z;

        public JsonVector(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }
    }

    public static class ChatUtil
    {
        public static Queue<MessageQueueInfo> MessageQueue = new Queue<MessageQueueInfo>();

        public static void DiceMasterSpeak(string message)
        {
            string formattedMessage = $"<size=200%><color=red>DICE MASTER</color><color=white>: {message}</color></size>";
            Send(formattedMessage, eGameEventChatLogType.Alert);
            Log.Debug($"DiceMasterSpeak :: {formattedMessage}");
        }

        public static void EventSpeak(DiceTier tier, string user, string eventName = "UNKNOWN") 
        {
            string tierName = "NO TIER";
            switch(tier)
            {
                case DiceTier.D3:
                    tierName = "<color=white>D3</color>";
                    break;
                case DiceTier.D4:
                    tierName = "<color=#ffe0e0>D4</color>";
                    break;
                case DiceTier.D6:
                    tierName = "<color=#ffc1c1>D6</color>";
                    break;
                case DiceTier.D8:
                    tierName = "<color=#ffa2a2>D8</color>";
                    break;
                case DiceTier.D12:
                    tierName = "<color=#ff8383>D12</color>";
                    break;
                case DiceTier.D20:
                    tierName = "<color=#ff6464>D20</color>";
                    break;
                case DiceTier.D50:
                    tierName = "<color=#ff4545>D50</color>";
                    break;
                case DiceTier.D100:
                    tierName = "<color=red>D100</color>";
                    break;
            }


            string formattredMessage = $"<size=150%><color=white>>> {user}</color> rolled a {tierName} :: <color=orange>{eventName}</color></size>";
            Send(formattredMessage, eGameEventChatLogType.Alert);
            Log.Debug($"EventSpeak :: {formattredMessage}");
        }

        public static void Send(string message, eGameEventChatLogType chatLogType = eGameEventChatLogType.GameEvent)
        {
            MessageQueue.Enqueue(new MessageQueueInfo() { message = message, chatLogType = chatLogType });
        }
    }

    public static class Extensions
    {
        public static T GetRandomElement<T>(this IList list)
        {
            return (T)list[Main.rnd.Next(list.Count)];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Main.rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static Vector3 Vector3(this JsonVector jsonVector)
        {
            return new Vector3(jsonVector.x, jsonVector.y, jsonVector.z);
        }

    }
}
