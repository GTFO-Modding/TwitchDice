using CellMenu;
using GTFO.API;
using System;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public class ChatManager : MonoBehaviour
    {
        public ChatManager(IntPtr intPtr) : base(intPtr) { }

        private void Update()
        {
            if (!ChatUtil.MessageQueue.TryDequeue(out MessageQueueInfo messageInfo))
            {
                return;
            }

            Send(messageInfo.message, messageInfo.chatLogType, messageInfo.networkSync);
        }

        public static void Send(string message, eGameEventChatLogType chatLogType = eGameEventChatLogType.GameEvent, bool networksync = true)
        {
            GuiManager.PlayerLayer.m_gameEventLog.AddLogItem(message, chatLogType);
            CM_PageLoadout.Current.m_gameEventLog.AddLogItem(message, chatLogType);
            CM_PageMap.Current.m_gameEventLog.AddLogItem(message, chatLogType);

            if (!networksync)
            {
                return;
            }

            ChatMsg chatMsg = new ChatMsg() { Message = message, LogType = (int)chatLogType };
            if (PlayerUtil.IsHost)
            {
                NetworkAPI.InvokeEvent(typeof(ChatMsg).Name, chatMsg);
            }
        }
    }

    public class MessageQueueInfo
    {
        public string message;
        public eGameEventChatLogType chatLogType;
        public bool networkSync = true;
    }
}
