using System.Collections.Generic;
using TwitchDice.Twitch;

namespace TwitchDice.Utilities
{
    public static class ChatUtil
    {
        public static Queue<MessageQueueInfo> MessageQueue = new Queue<MessageQueueInfo>();

        public static void DiceMasterSpeak(string message, bool networksync = true)
        {
            string formattedMessage = $"<size=200%><color=red>DICE MASTER</color><color=white>: {message}</color></size>";
            Send(formattedMessage, eGameEventChatLogType.Alert, networksync);
            Log.Debug($"DiceMasterSpeak :: {formattedMessage}");
        }

        public static void EventSpeak(IDiceEvent diceEvent, string activator)
        {
            string tierName = "NO TIER";
            switch (diceEvent.Tier)
            {
                case DiceTier.D3:
                    tierName = $"<color={Main.COLOR_D3}>D3</color>";
                    break;
                case DiceTier.D4:
                    tierName = $"<color={Main.COLOR_D4}>D4</color>";
                    break;
                case DiceTier.D6:
                    tierName = $"<color={Main.COLOR_D6}>D6</color>";
                    break;
                case DiceTier.D8:
                    tierName = $"<color={Main.COLOR_D8}>D8</color>";
                    break;
                case DiceTier.D12:
                    tierName = $"<color={Main.COLOR_D12}>D12</color>";
                    break;
                case DiceTier.D20:
                    tierName = $"<color={Main.COLOR_D20}>D20</color>";
                    break;
                case DiceTier.D50:
                    tierName = $"<color={Main.COLOR_D50}>D50</color>";
                    break;
                case DiceTier.D100:
                    tierName = $"<color={Main.COLOR_D100}>D100</color>";
                    break;
            }


            string formattredMessage = $"<size=150%><color=white>>> {activator}</color> rolled a {tierName} :: <color=orange>{diceEvent.EventName}</color></size>";
            Send(formattredMessage, eGameEventChatLogType.Alert);
            Log.Debug($"EventSpeak :: {formattredMessage}");
        }

        public static void Send(string message, eGameEventChatLogType chatLogType = eGameEventChatLogType.GameEvent, bool networksync = true)
        {
            MessageQueue.Enqueue(new MessageQueueInfo() { message = message, chatLogType = chatLogType, networkSync = networksync });
        }
    }
}
