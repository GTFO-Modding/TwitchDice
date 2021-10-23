using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CellMenu;
using TwitchDice.Twitch;
using UnityEngine;
using SNetwork;
using Player;
using Nidhogg.Managers;
using System.Reflection;
using System.Linq;
using AIGraph;

namespace TwitchDice.Utilities
{
    public class ChatManager : MonoBehaviour
    {
        public ChatManager(IntPtr intPtr) : base(intPtr) { }

        void Update()
        {
            if (!ChatUtil.MessageQueue.TryDequeue(out MessageQueueInfo messageInfo)) return;
            Send(messageInfo.message, messageInfo.chatLogType, messageInfo.networkSync);
        }

        public static void Send(string message, eGameEventChatLogType chatLogType = eGameEventChatLogType.GameEvent, bool networksync = true)
        {
            GuiManager.PlayerLayer.m_gameEventLog.AddLogItem(message, chatLogType);
            CM_PageLoadout.Current.m_gameEventLog.AddLogItem(message, chatLogType);
            CM_PageMap.Current.m_gameEventLog.AddLogItem(message, chatLogType);

            if (!networksync) return;
            ChatMsg chatMsg = new ChatMsg() { Message = message, LogType = (int)chatLogType };
            if (PlayerUtil.IsHost) NetworkingManager.InvokeEvent(typeof(ChatMsg).Name, chatMsg);
        }
    }

    public static class PlayerUtil
    {
        private static bool? _isHost;
        public static bool IsHost
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

        public static PlayerAgent LocalPlayerAgent
        {
            get
            {
                return PlayerManager.GetLocalPlayerAgent();
            }
        }

        public static SNet_Player LocalNetAgent
        {
            get
            {
                return LocalPlayerAgent.Owner;
            }
        }

        public static int PlayerCount
        {
            get
            {
                return PlayerManager.PlayerAgentsInLevel.Count;
            }
        }

        public static bool TryGetRandomPlayerAgent(out PlayerAgent playerAgent, bool IncludeHost = true, List<PlayerAgent> exclude = null, bool excludeDead = false)
        {
            playerAgent = null;
            var list = new List<PlayerAgent>();
            foreach (var item in PlayerManager.PlayerAgentsInLevel)
            {
                if (!IncludeHost && item.IsLocallyOwned) continue;
                if (excludeDead && !item.Alive) continue;
                if (exclude != null)
                    if (exclude.Contains(item)) continue;

                list.Add(item);
            }

            if (list.Count == 0) return false;

            try
            {
                playerAgent = list.GetRandomElement<PlayerAgent>();
                return true;
            } catch(Exception e) { Log.Error(e); }

            return false;
        }

        public static void TeleportToPosition(PlayerAgent player, Vector3 position)
        {
            TimedEvents.Start(Teleport(position));
        }

        private static IEnumerator Teleport(Vector3 pos)
        {
            PlayerUtil.LocalPlayerAgent.PlayerCharacterController.m_updateEnabled = false;
            PlayerUtil.LocalPlayerAgent.PlayerCharacterController.ManualMoveTo(pos);
            yield return new WaitForSeconds(0.5f);
            PlayerUtil.LocalPlayerAgent.PlayerCharacterController.m_updateEnabled = true;
            yield break;
        }
    }

    public static class SpawnUtil
    {
        public static GameObject CreateEmpty(Vector3 position)
        {
            var go = new GameObject();
            go.transform.position = position;
            return go;
        }

        public static List<RaycastHit> GetRandomScatterAround(Vector3 org, int count)
        {
            var list = new List<RaycastHit>();
            for (int i = 0; i < count; i++)
            {
                Vector3 direction = UnityEngine.Random.insideUnitSphere.normalized;
                Ray ray = new Ray(org, direction);
                RaycastHit hit;
                int iterations = 0;
                while (!Physics.Raycast(ray, out hit, 100000, LayerManager.MASK_CAMERA_RAY) && iterations < 100)
                {
                    direction = UnityEngine.Random.insideUnitSphere.normalized;
                    ray = new Ray(org, direction);
                    iterations++;
                }
                list.Add(hit);
            }

            return list;
        }

        public static RaycastHit GetRandomPointAround(Vector3 org)
        {
            Vector3 direction = UnityEngine.Random.insideUnitSphere.normalized;
            Ray ray = new Ray(org, direction);
            RaycastHit hit;
            int iterations = 0;
            while (!Physics.Raycast(ray, out hit, 100000, LayerManager.MASK_CAMERA_RAY) && iterations < 100)
            {
                direction = UnityEngine.Random.insideUnitSphere.normalized;
                ray = new Ray(org, direction);
                iterations++;
            }
            return hit;
        }

        public static void ThrowConsumable(Vector3 org, Vector3 target, PlayerAgent source, pItemData data, int throwForce)
        {
            Vector3 direction = (target - source.EyePosition).normalized;
            var rot = Quaternion.LookRotation(direction, Vector3.up);
            ItemReplicationManager.ThrowItem(data, null, ItemMode.Instance, org, rot, direction * throwForce, source.EyePosition, source.CourseNode, source);
        }
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

    public class MessageQueueInfo
    {
        public string message;
        public eGameEventChatLogType chatLogType;
        public bool networkSync = true;
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

        public static void DiceMasterSpeak(string message, bool networksync = true)
        {
            string formattedMessage = $"<size=200%><color=red>DICE MASTER</color><color=white>: {message}</color></size>";
            Send(formattedMessage, eGameEventChatLogType.Alert, networksync);
            Log.Debug($"DiceMasterSpeak :: {formattedMessage}");
        }

        public static void EventSpeak(IDiceEvent diceEvent, string activator) 
        {
            string tierName = "NO TIER";
            switch(diceEvent.Tier)
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

    public static class NodeUtil
    {
        public static List<AIG_CourseNode> GetReachableNodes(AIG_CourseNode from, int maxNodeDistance)
        {
            AIG_SearchID.IncrementSearchID();
            List<AIG_CourseNode> nodes = new List<AIG_CourseNode>();
            ushort searchID = AIG_SearchID.SearchID;
            Queue<AIG_CourseNode> queue = new Queue<AIG_CourseNode>();
            from.m_searchID = searchID;
            from.m_searchStep = 0;
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                AIG_CourseNode aig_CourseNode = queue.Dequeue();
                if (aig_CourseNode.m_searchStep + 1 <= maxNodeDistance)
                {
                    nodes.Add(aig_CourseNode);
                    for (int i = 0; i < aig_CourseNode.m_portals.Count; i++)
                    {
                        AIG_CoursePortal aig_CoursePortal = aig_CourseNode.m_portals[i];
                        if (aig_CoursePortal.IsTraversable && aig_CoursePortal.m_searchID != searchID)
                        {
                            aig_CoursePortal.m_searchID = searchID;
                            AIG_CourseNode oppositeNode = aig_CoursePortal.GetOppositeNode(aig_CourseNode);
                            if (oppositeNode.m_searchID != searchID)
                            {
                                oppositeNode.m_searchID = searchID;
                                oppositeNode.m_searchStep = aig_CourseNode.m_searchStep + 1;
                                queue.Enqueue(oppositeNode);
                            }
                        }
                    }
                }
            }
            return nodes;
        }
    }

    public static class ColorUtil
    {
        public static string GetDiceColorForTier(DiceTier tier)
        {
            return tier switch
            {
                DiceTier.D3 => Main.COLOR_D3,
                DiceTier.D4 => Main.COLOR_D4,
                DiceTier.D6 => Main.COLOR_D6,
                DiceTier.D8 => Main.COLOR_D8,
                DiceTier.D12 => Main.COLOR_D12,
                DiceTier.D20 => Main.COLOR_D20,
                DiceTier.D50 => Main.COLOR_D50,
                DiceTier.D100 => Main.COLOR_D100,
                _ => "",
            };
        }
    }

    public static class Extensions
    {
        public static int Matches<T>(this IList<T> list, Func<T, bool> matchFN)
        {
            int count = 0;
            foreach (var item in list)
            {
                if (matchFN(item))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> matchFN)
        {
            int count = 0;
            for (int index = 0, length = list.Count; index < length; index++)
            {
                if (matchFN(list[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches<T>(this UnhollowerBaseLib.Il2CppReferenceArray<T> array, Func<T, bool> matchFN)
            where T : UnhollowerBaseLib.Il2CppObjectBase
        {
            int count = 0;
            for (int index = 0, length = array.Length; index < length; index++)
            {
                if (matchFN(array[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches<T>(this UnhollowerBaseLib.Il2CppStructArray<T> array, Func<T, bool> matchFN)
            where T : unmanaged
        {
            int count = 0;
            for (int index = 0, length = array.Length; index < length; index++)
            {
                if (matchFN(array[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Matches(this UnhollowerBaseLib.Il2CppStringArray array, Func<string, bool> matchFN)
        {
            int count = 0;
            for (int index = 0, length = array.Length; index < length; index++)
            {
                if (matchFN(array[index]))
                {
                    count++;
                }
            }

            return count;
        }

        public static Il2CppSystem.Collections.Generic.List<T> Filter<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> filterFN)
        {
            int index = 0;
            while (index < list.Count)
            {
                if (filterFN(list[index]))
                {
                    index++;
                }
                else
                {
                    list.RemoveAt(index);
                }
            }

            return list;
        }

        public static List<T> Filter<T>(this List<T> list, Func<T, bool> filterFN)
        {
            int index = 0;
            while (index < list.Count)
            {
                if (filterFN(list[index]))
                {
                    index++;
                }
                else
                {
                    list.RemoveAt(index);
                }
            }

            return list;
        }

        public static List<T> ToManaged<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            var result = new List<T>();
            for (int index = 0, count = list.Count; index < count; index++)
            {
                result.Add(list[index]);
            }
            return result;
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToUnmanaged<T>(this List<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            for (int index = 0, count = list.Count; index < count; index++)
            {
                result.Add(list[index]);
            }
            return result;
        }

        public static T GetRandomElement<T>(this IList list)
        {
            return (T)list[Main.rnd.Next(list.Count)];
        }

        public static T GetRandomElement<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            return list[Main.rnd.Next(list.Count)];
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
