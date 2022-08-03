using SNetwork;
using System.Collections.Generic;
using TwitchDice.Extensions;

namespace TwitchDice.Utilities
{
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
}
