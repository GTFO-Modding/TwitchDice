using Player;
using SNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TwitchDice.Extensions;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class PlayerUtil
    {
        private static bool? _isHost;
        public static bool IsHost
        {
            get
            {
                if (_isHost.HasValue)
                {
                    return _isHost.Value;
                }

                if (SNet.Core.TryGetLobbyOwner(out SNet_Player player))
                {
                    _isHost = player.IsLocal;
                    return _isHost.Value;
                }
                Log.Error("Couldn't get lobby host :(");
                return false;
            }
        }

        public static PlayerAgent LocalPlayerAgent => PlayerManager.GetLocalPlayerAgent();

        public static SNet_Player LocalNetAgent => LocalPlayerAgent.Owner;

        public static int PlayerCount => PlayerManager.PlayerAgentsInLevel.Count;

        public static bool TryGetRandomPlayerAgent(out PlayerAgent playerAgent, bool IncludeHost = true, List<PlayerAgent> exclude = null, bool excludeDead = false)
        {
            playerAgent = null;
            var list = new List<PlayerAgent>();
            foreach (PlayerAgent agent in PlayerManager.PlayerAgentsInLevel)
            {
                if (!IncludeHost && agent.IsLocallyOwned)
                {
                    continue;
                }

                if (excludeDead && !agent.Alive)
                {
                    continue;
                }

                if (exclude != null && exclude.Contains(agent))
                {
                    continue;
                }

                list.Add(agent);
            }

            if (list.Count == 0)
            {
                return false;
            }

            try
            {
                playerAgent = list.GetRandomElement<PlayerAgent>();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return false;
        }

        public static void TeleportToPosition(PlayerAgent player, Vector3 position)
        {
            TimedEvents.Start(Teleport(position));
        }

        private static IEnumerator Teleport(Vector3 pos)
        {
            PlayerCharacterController localPlayerController = PlayerUtil.LocalPlayerAgent.PlayerCharacterController;

            localPlayerController.m_updateEnabled = false;
            localPlayerController.ManualMoveTo(pos);
            yield return new WaitForSeconds(0.5f);
            localPlayerController.m_updateEnabled = true;
            yield break;
        }
    }
}
